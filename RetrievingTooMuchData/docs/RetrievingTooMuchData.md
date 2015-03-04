# Retrieving Too Much Data

Applications fetch data for query purposes, or to perform some application-specific processing. Retrieving data, whether from a remote web service, a database, or a file, incurs I/O. Retrieving more data than is necessary to satisfy a business operation can result in unnecessary I/O overhead and can reduce responsiveness. In a cloud environment supporting multiple concurrent instances of an application, this overhead can accumulate to have a significant impact on the performance and scalability of the system.

This anti-pattern typically occurs because:

- The application attempts to minimize the number of I/O requests by retrieving all of the data that it *might* need. This is often a result of overcompensating for the [Chatty I/O][chatty-io] anti-pattern. For example, the [sample code][fullDemonstrationOfProblem] provided with this anti-pattern contains part of a web application enables a customer to browse products that an organization sells. This information is held in the `Products` table in the AdventureWorks2012 database shown in the image below. A simple form of the application fetches the complete details for every product. This is wasteful on at least three counts:

	1. The customer might not be interested in every detail; they would typically need to see the product name, description, price, dimensions, and possibly a thumbnail image. Other related information such as product ratings, reviews, and detailed images might be useful but could be expensive and wasteful to retrieve unless the customer specifically requests it.

	2. Not all of the product details might be relevant to the customer; there could be some properties that are only meaningful to the organization or that should remain hidden from customers.

	3. The customer is unlikely to want to view every product that the organization sells.

![Entity Framework data model based on the Product table in the AdventureWorks2012 database][full-product-table]

- The application was developed by following poor programming or design practice. For example, the following code (taken from the sample application) retrieves product information by using the Entity Framework to fetch the complete details for every product. The code then filters this information to return only the information that the user has requested. The remaining data is discarded. This is clearly wasteful, but commonplace:

**C# web API**

```C#
[HttpGet]
[Route("toomanyfields/products/project_all_fields")]
public async Task<IEnumerable<ProductInfo>> GetProductsProjectAllFieldsAsync()
{
    using (var context = GetContext())
    {
        // Fetch all of the details of every product
        var products = await context.Products
                                    .ToListAsync() //Execute query.
                                    .ConfigureAwait(false);

        // Return only the fields in which the user is interested
        return products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }); //Project fields.;            
    }
}
...
private AdventureWorksContext GetContext()
{
    var connectionString = CloudConfigurationManager.GetSetting("AdventureWorksContext");
    return new AdventureWorksContext(connectionString);
}
```

- Similarly, the application might retrieve data to perform aggregations or other forms of operations. The following sample code (also taken from the sample application) calculated the total sales made by all sales people in the company. The application retrieves every record for orders sold by a sales person (as opposed to orders placed directly by customers), and then iterates through these records to calculate the total sales value:

![Entity Framework data model showing the SalesPerson and SalesOrderHeader tables][product-sales-tables]

**C# web API**

```C#
[HttpGet]
[Route("toomanyrows/sales/total_aggregate_on_client")]
public async Task<decimal> GetTotalSalesAggregateOnClientAsync()
{
    decimal total = 0;

    using(var context = GetEagerLoadingContext())
    {
        // NOTE: This context uses EAGER loading.
        // By loading all the SalePerson records this query also fetches all related SalesOrderHeader collections.
        var salesPersons = await context.SalesPersons
                                        .Include( sp => sp.SalesOrderHeaders)
                                        .ToListAsync()
                                        .ConfigureAwait(false);

        foreach (var salesPerson in salesPersons)
        {
            var orderHeaders = salesPerson.SalesOrderHeaders.ToList();

            total += orderHeaders.Sum(x => x.TotalDue);
        }
    }

    return total;
}
...
private AdventureWorksContext GetEagerLoadingContext()
{
    var connectionString = CloudConfigurationManager.GetSetting("AdventureWorksContext");
    var context = new AdventureWorksContext(connectionString);
    // load eagerly
    context.Configuration.LazyLoadingEnabled = false;
    context.Configuration.ProxyCreationEnabled = false;

    return context;
}
```

- The application retrieves data from a data source by using the `IEnumerable` interface. This interface supports filtering and enumeration of data, but the filtering is performed on the client-side after it has been retrieved from the data source. Technologies such as LINQ to Entities (used by the Entity Framework) default to retrieving data through the `IQueryable` interface, which passes the responsibility for filtering to the data source. However, in some situations an application might reference an operation which is only available to the client and not available in the data source, requiring that the data be returned through the `IEnumerable` interface (by applying the `AsEnumerable` method to an entity collection). The following example shows a LINQ to Entities query that retrieves all products where the `SellStartDate` column lies somewhere in the previous week. LINQ to Entities cannot map the `AddDays` function to an operation in the database, so the query returns every row from the product table to the application where it is filtered. If there are only a small number of rows that match this criterion, this is a waste of bandwidth.

**C# Entity Framework**

``` C#
var context = new AdventureWorks2012Entities();
var query = from p in context.Products.AsEnumerable()
            where p.SellStartDate < DateTime.Now.AddDays(-7) // AddDays cannot be mapped by LINQ to Entities
            select ...;

List<Product> products = query.ToList();
```

## How to detect the problem

Symptoms of retrieving too much data in an application include high latency and low throughput. If the data is retrieved from a data store, then increased contention is also probable. End-users are likely to report extended response times and possible failures caused by services timing out due to increased network traffic and resource conflicts in the data store. These failures could manifest themselves as HTTP 500 (Internal Server) errors or HTTP 503 (Service Unavailable) errors. In these cases, you should examine the event logs for the web server which are likely to contain more detailed information about the causes and circumstances of the errors.

You can perform the following steps to help identify the causes of any problems:

1. Identify slow workloads or transactions by performing load-testing, process monitoring, or other form of instrumentation.

2. Capture any behavioral patterns exhibited by the system. For example, does the performance get worse at 5pm each day, and what happens when the workload exceeds a specific limit in terms of transactions per second or volume of users?

3. Identify the source of the data and the data stores being used.

4. For each data source, run lower level telemetry to observe the behavior of operations end-to-end by using process monitoring, instrumentation, or application profiling.

5. Correlate the telemetry with any behavioral patterns.

6. Determine which data sources or other resources are subject to contention.

7. Identify any slow-running queries that reference these data sources.

8. Perform a resource-specific analysis of the slow-running queries and ascertain how the data is used and consumed.

The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you to examine applications and services systematically.

----------

### Identifying slow workloads

An initial analysis will likely be based on user reports concerning functionality that is running slowly or raising exceptions. Load testing this functionality in a controlled test environment could indicate high latency and low throughput. As an example, the following performance results were obtained by using a load test that simulated 100 concurrent users against the `GetProductsProjectAllFieldsAsync` and `GetTotalSalesAggregateOnClientAsync` methods in the [sample code][fullDemonstrationOfProblem].

![Load-test results for the GetProductsProjectAllFieldsAsync and GetTotalSalesAggregateOnClientAsync methods][Load-Test-Results-Client-Side]

The average response time was 35.7 seconds, but a customer that visits the ecommerce web site using this deployment may have to wait between 22 and 44 seconds to see product information after submitting a request. This is too slow for many users.

----------

**Note:** The deployment used for the load-test was relatively small, but the user load was equally small. The results for the *corrected* solution described in the [Consequences of the solution](#ConsequencesOfTheSolution) section were tested using the same deployment and user load to show the effects of optimizing the data access strategy. The same general principles apply for a larger-scale deployment with more users; the pattern should be similar.

----------

Profiling an application in a test environment can help to identify the following symptoms that characterize operations that retrieve large amounts of data. The exact symptoms will depend on the nature of the resources being accessed but may include:

- Frequent, large I/O requests made to the same resource or data store.

- Contention in a shared resource or data store hosting the requested information.

- Client applications frequently receiving large volumes of incoming data across the network.

- Applications and services spending significant time waiting for I/O to complete.

### Capturing behavioral patterns

Determining whether behavior is influenced by time is a matter of monitoring the performance of the production system over an appropriate period and examining the usage statistics. Any correlation between regular periods of high usage and slowing performance can indicate areas of concern. The performance profile of functionality that is suspected to be slow-running should be closely examined to ascertain whether it matches that of the load-testing performed earlier.

**INFO ON USING APPDYNAMICS OR OTHER MONTORING TOOLS TO BE ADDED HERE?**

Load-testing (in a test environment) this same functionality using step-based user loads can help to highlight the point at which the performance drops significantly. If this load is within the bounds of that expected at periods of peak activity, then the way in which the functionality is implemented should be examined further.

### Identifying data sources

If you suspect that a service is performing poorly because of the way in which data is being retrieved, you should investigate how the application is interacting with the repositories it utilizes. Monitoring the live system, together with a review of the source code (if possible) should help to reveal the data sources being accessed during periods of poor performance.

**MORE INFO ON APPDYNAMICS OR OTHER MONTORING TOOLS TO BE ADDED HERE?**

In the case of the sample application, all the information is held in a single instance of Windows Azure SQL Database.

### Observing the end-to-end behavior of operations that access each data source

For each data source, you should instrument the system to capture the frequency with which each data store is accessed, the volume of data entering and exiting the data store, the timings of these operations (in particular, the latency of requests), and the nature and rate of any errors observed while accessing each data store under loads that are typical of the system. This might involve observing the live system and tracing the lifecycle of each user request, or you can model a series of synthetic workloads and run them against a test system if you require a more controlled environment.

**MORE INFO ON APPDYNAMICS OR OTHER MONTORING TOOLS TO BE ADDED HERE?**

### Correlating the telemetry with behavioral patterns

A slow operation is not necessarily a problem if it is not being performed when the system is under stress or it is not time critical. For example, generating the monthly operational statistics might be a long-running operation, but it can probably be performed as a batch process and run as a low priority job. On the other hand, customers querying the product catalog or placing orders are critical business operations that can have a direct bearing on the profitability of an organization. Therefore you should focus on the telemetry generated by these critical operations to see how the performance varies during periods of high usage. In the sample application, the critical operations result in calls to the `GetProductsProjectAllFieldsAsync` and `GetTotalSalesAggregateOnClientAsync` methods. Note that this process requires careful analysis of the instrumentation data correlated with the end-to-end operations that users are performing.

**WHICH ANALYSIS TOOLS TO USE?**

### Identifying contended resources

In a system that utilizes a single data source, contention is likely to be caused by the volume of requests being directed towards this data source (partitioning the data source may help to resolve this issue if it is suspected to be a cause of poor performance). If the application references multiple data sources, then you should gather the data access statistics for each one and determine whether any of these data sources are likely to be acting as a bottleneck.

**MORE INFO ON APPDYNAMICS OR OTHER MONTORING TOOLS TO BE ADDED HERE?**

Examining data access statistics and other information provided by a data store can produce some useful information about the frequency with which the data store is accessed, the data that is requested, and the processing and network resources required to retrieve this data. For example, Windows Azure SQL Database provides access to query statistics using the Query Performance pane in the Azure SQL Database management portal. This pane shows information about all recently executed queries:

![The Query Performance pane in the Windows Azure SQL Database management portal][QueryPerformanceZoomed]

The `Run Count` column in the results indicates how frequently each query is run. In this case, the following queries have been executed a significant number of times by the load-test:

**SQL**

```SQL

/* Executed 256 times by the load test */
SELECT 
    [Project1].[BusinessEntityID] AS [BusinessEntityID], 
    [Project1].[TerritoryID] AS [TerritoryID], 
    [Project1].[SalesQuota] AS [SalesQuota], 
    [Project1].[Bonus] AS [Bonus], 
    [Project1].[CommissionPct] AS [CommissionPct], 
    [Project1].[SalesYTD] AS [SalesYTD], 
    [Project1].[SalesLastYear] AS [SalesLastYear], 
    [Project1].[rowguid] AS [rowguid], 
    [Project1].[ModifiedDate] AS [ModifiedDate], 
    [Project1].[C1] AS [C1], 
    [Project1].[SalesOrderID] AS [SalesOrderID], 
    [Project1].[RevisionNumber] AS [RevisionNumber], 
    [Project1].[OrderDate] AS [OrderDate], 
    [Project1].[DueDate] AS [DueDate], 
    [Project1].[ShipDate] AS [ShipDate], 
    [Project1].[Status] AS [Status], 
    [Project1].[OnlineOrderFlag] AS [OnlineOrderFlag], 
    [Project1].[SalesOrderNumber] AS [SalesOrderNumber], 
    [Project1].[PurchaseOrderNumber] AS [PurchaseOrderNumber], 
    [Project1].[AccountNumber] AS [AccountNumber], 
    [Project1].[CustomerID] AS [CustomerID], 
    [Project1].[SalesPersonID] AS [SalesPersonID], 
    [Project1].[TerritoryID1] AS [TerritoryID1], 
    [Project1].[BillToAddressID] AS [BillToAddressID], 
    [Project1].[ShipToAddressID] AS [ShipToAddressID], 
    [Project1].[ShipMethodID] AS [ShipMethodID], 
    [Project1].[CreditCardID] AS [CreditCardID], 
    [Project1].[CreditCardApprovalCode] AS [CreditCardApprovalCode], 
    [Project1].[CurrencyRateID] AS [CurrencyRateID], 
    [Project1].[SubTotal] AS [SubTotal], 
    [Project1].[TaxAmt] AS [TaxAmt], 
    [Project1].[Freight] AS [Freight], 
    [Project1].[TotalDue] AS [TotalDue], 
    [Project1].[Comment] AS [Comment], 
    [Project1].[rowguid1] AS [rowguid1], 
    [Project1].[ModifiedDate1] AS [ModifiedDate1]
    FROM ( SELECT 
        [Extent1].[BusinessEntityID] AS [BusinessEntityID], 
        [Extent1].[TerritoryID] AS [TerritoryID], 
        [Extent1].[SalesQuota] AS [SalesQuota], 
        [Extent1].[Bonus] AS [Bonus], 
        [Extent1].[CommissionPct] AS [CommissionPct], 
        [Extent1].[SalesYTD] AS [SalesYTD], 
        [Extent1].[SalesLastYear] AS [SalesLastYear], 
        [Extent1].[rowguid] AS [rowguid], 
        [Extent1].[ModifiedDate] AS [ModifiedDate], 
        [Extent2].[SalesOrderID] AS [SalesOrderID], 
        [Extent2].[RevisionNumber] AS [RevisionNumber], 
        [Extent2].[OrderDate] AS [OrderDate], 
        [Extent2].[DueDate] AS [DueDate], 
        [Extent2].[ShipDate] AS [ShipDate], 
        [Extent2].[Status] AS [Status], 
        [Extent2].[OnlineOrderFlag] AS [OnlineOrderFlag], 
        [Extent2].[SalesOrderNumber] AS [SalesOrderNumber], 
        [Extent2].[PurchaseOrderNumber] AS [PurchaseOrderNumber], 
        [Extent2].[AccountNumber] AS [AccountNumber], 
        [Extent2].[CustomerID] AS [CustomerID], 
        [Extent2].[SalesPersonID] AS [SalesPersonID], 
        [Extent2].[TerritoryID] AS [TerritoryID1], 
        [Extent2].[BillToAddressID] AS [BillToAddressID], 
        [Extent2].[ShipToAddressID] AS [ShipToAddressID], 
        [Extent2].[ShipMethodID] AS [ShipMethodID], 
        [Extent2].[CreditCardID] AS [CreditCardID], 
        [Extent2].[CreditCardApprovalCode] AS [CreditCardApprovalCode], 
        [Extent2].[CurrencyRateID] AS [CurrencyRateID], 
        [Extent2].[SubTotal] AS [SubTotal], 
        [Extent2].[TaxAmt] AS [TaxAmt], 
        [Extent2].[Freight] AS [Freight], 
        [Extent2].[TotalDue] AS [TotalDue], 
        [Extent2].[Comment] AS [Comment], 
        [Extent2].[rowguid] AS [rowguid1], 
        [Extent2].[ModifiedDate] AS [ModifiedDate1], 
        CASE WHEN ([Extent2].[SalesOrderID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
        FROM  [Sales].[SalesPerson] AS [Extent1]
        LEFT OUTER JOIN [Sales].[SalesOrderHeader] AS [Extent2] ON [Extent1].[BusinessEntityID] = [Extent2].[SalesPersonID]
    )  AS [Project1]
    ORDER BY [Project1].[BusinessEntityID] ASC, [Project1].[C1] ASC


/* Executed 579 times by the load test */
SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ProductNumber] AS [ProductNumber], 
    [Extent1].[MakeFlag] AS [MakeFlag], 
    [Extent1].[FinishedGoodsFlag] AS [FinishedGoodsFlag], 
    [Extent1].[Color] AS [Color], 
    [Extent1].[SafetyStockLevel] AS [SafetyStockLevel], 
    [Extent1].[ReorderPoint] AS [ReorderPoint], 
    [Extent1].[StandardCost] AS [StandardCost], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[Size] AS [Size], 
    [Extent1].[SizeUnitMeasureCode] AS [SizeUnitMeasureCode], 
    [Extent1].[WeightUnitMeasureCode] AS [WeightUnitMeasureCode], 
    [Extent1].[Weight] AS [Weight], 
    [Extent1].[DaysToManufacture] AS [DaysToManufacture], 
    [Extent1].[ProductLine] AS [ProductLine], 
    [Extent1].[Class] AS [Class], 
    [Extent1].[Style] AS [Style], 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductModelID] AS [ProductModelID], 
    [Extent1].[SellStartDate] AS [SellStartDate], 
    [Extent1].[SellEndDate] AS [SellEndDate], 
    [Extent1].[DiscontinuedDate] AS [DiscontinuedDate], 
    [Extent1].[rowguid] AS [rowguid], 
    [Extent1].[ModifiedDate] AS [ModifiedDate]
    FROM [Production].[Product] AS [Extent1]
```

In the sample application, tracing the `GetTotalSalesAggregateOnClientAsync` method reveals that the first SQL statement is a result of running the following C# code:

**C#**

```C#
var salesPersons = await context.SalesPersons
                                .Include( sp => sp.SalesOrderHeaders)
                                .ToListAsync()
                                .ConfigureAwait(false);
```

Similarly, tracing the `GetProductsProjectAllFieldsAsync` method shows that the second SQL statement is run by the following C# code:

**C#**
```C#
var products = await context.Products
                            .ToListAsync()
                            .ConfigureAwait(false);
```

### Identifying slow queries

Tracing execution and analyzing the application source code and data access logic might reveal that a number of different queries are performed as the application runs. You should concentrate on those that consume the most resources and take the most time to execute. You can add instrumentation to determine the start and completion times for many database operations enabling you to work out the duration. However, many data stores also provide in-depth information on the way in which queries are performed and optimized. For example, the Query Performance pane in the Azure SQL Database management portal enables you to select a query and drill into the detailed runtime performance information for that query.

![The Query Details pane in the Windows Azure SQL Database management portal][QueryDetails]

The statistics summarize the resources used by this query. 

### Performing a resource-specific analysis of the slow-running queries

Examining the queries frequently performed against a data source, and the way in which an application uses this information, can provide an insight into how key operations might be speeded up. In some cases, it may be advisable to partition resources horizontally if different attributes of the data (columns in a relational table, or fields in a NoSQL store) are accessed separately by different functions; this can help to reduce contention; often 90% of operations are run against 10% of the data held in the various data sources, so spreading this load may improve performance.

Depending on the nature of the data store, you may be able to exploit the features that it implements to efficiently store and retrieve information. For example, if an application requires an aggregation over a number of items (such as a count, sum, min or max operation), SQL databases typically provide aggregate functions that can perform these operations without requiring that an application fetches all of the data and implement the calculation itself. In other types of data store, it may be possible to maintain this information separately within the store as records are added, updated, or removed, again eliminating the requirement of an application to fetch a potentially large amount of data and perform the calculation itself.

If you observe requests that retrieve a large number of fields, examine the underlying source code to determine whether all of these fields are actually necessary. Sometimes these requests are the results of injudicious *SELECT ** operations, or misplaced `.Include` operations in LINQ queries. Similarly, requests that retrieve a large number of entities (rows in a SQL Server database) may be indicative of an application that is not filtering data correctly. Verify that all of these entities are actually necessary, and implement database-side filtering if possible (for example, using a *WHERE* clause in an SQL statement.) For operations that have to support unbounded queries, the system should implement pagination and only fetch a limited number (a *page*) of entities at a time.


----------

**Note:** If analysis shows that none of these situations apply, then retrieving too much data is unlikely to be the cause of poor performance and you should look elsewhere.

----------


## How to correct the problem

Only fetch the data that is required; avoid transmitting large volumes of data that may quickly become outdated or might be discarded, and only fetch the data appropriate to the operation being performed. The following examples describe possible solutions to many of the scenarios listed earlier:

- In the example that retrieves product information, perform the projection at the database rather than fetching and filtering data in the application code:

**C# web API**

```C#
[HttpGet]
[Route("toomanyfields/products/project_only_required_fields")]
public async Task<IEnumerable<ProductInfo>> GetProductsProjectOnlyRequiredFieldsAsync()
{
    using (var context = GetContext())
    {
        return await context.Products
                            .Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }) //Project fields.
                            .ToListAsync() //Execute query.
                            .ConfigureAwait(false);
    }
}
```

----------

**Note:** This code is available in the [sample solution][fullDemonstrationOfSolution] provided with this anti-pattern.

----------

- In the example that aggregates information held in a database, perform the aggregation in the database rather than in the client application code:

**C# web API**

```C#
[HttpGet]
[Route("toomanyrows/sales/total_aggregate_on_database")]
public async Task<decimal> GetTotalSalesAsync()
{
    using (var context = GetContext())
    {
        var query = from sp in context.SalesPersons
                    from soh in sp.SalesOrderHeaders
                    select soh.TotalDue;

        return await query.DefaultIfEmpty(0)
                          .SumAsync()
                          .ConfigureAwait(false);
    }
}
```

- Wherever possible, ensure that LINQ queries are resolved by using the `IQueryable` interface rather than `IEnumerable`. This may be a matter of rephrasing a query to use only the features and functions that can be mapped by LINQ to features available in the underlying data source, or adding user-defined functions to the data source that can perform the required operations on the data before returning it. In the example shown earlier, the code can be refactored to remove the problematic `AddDays` function from the query, allowing filtering to be performed by the database:


**C# Entity Framework**

``` C#
var context = new AdventureWorks2012Entities();

DateTime dateSince = DateTime.Now.AddDays(-7);
var query = from p in context.Products
            where p.SellStartDate < dateSince // AddDays has been factored out. This criterion can be passed to the database by LINQ to Entities
            select ...;

List<Product> products = query.ToList();
```

## <a name="ConsequencesOfTheSolution"></a>Consequences of the solution

The system should spend less time waiting I/O, network traffic should be diminished, and contention for shared data resources should be decreased. This should manifest itself as an improvement in response time and throughput in an application. Performing load-testing against the `GetProductsProjectOnlyRequiredFieldsAsync` and `TotalSalesAsync` methods in the [sample solution][fullDemonstrationOfSolution] shows the following results:

![Load-test results for the GetProductsProjectAllFieldsAsync and GetTotalSalesAggregateOnClientAsync methods][Load-Test-Results-Database-Side]

This load-test was performed on the same deployment and using the same simulated workload of 100 concurrent users as before. The graph shows much lower latency; each request takes takes approximately 0.5 seconds compared to 35.7 seconds in the non-optimized case. The response time and throughput are also much more predictable.

Examining the query access statistics shows that the following queries were performed by the `GetProductsProjectOnlyRequiredFieldsAsync` and `TotalSalesAsync` methods:

**SQL**

```SQL
/* Executed 17910 times by the load-test */
SELECT 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        SUM([Join1].[A1]) AS [A1]
        FROM ( SELECT 
            CASE WHEN ([Project1].[C1] IS NULL) THEN cast(0 as decimal(18)) ELSE [Project1].[TotalDue] END AS [A1]
            FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
            LEFT OUTER JOIN  (SELECT 
                [Extent1].[TotalDue] AS [TotalDue], 
                cast(1 as tinyint) AS [C1]
                FROM [Sales].[SalesOrderHeader] AS [Extent1]
                WHERE [Extent1].[SalesPersonID] IS NOT NULL ) AS [Project1] ON 1 = 1
        )  AS [Join1]
    )  AS [GroupBy1]


/* Executed 38786 times by the load-test */
SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[Product] AS [Extent1]
```

The increased throughput and performance is reflected by the increased number of times these queries ran compared to those in the earlier load-test.

## Related resources
- [The performance implications of IEnumerable vs. IQueryable][IEnumerableVsIQueryable].


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[chatty-io]: http://LINK-TO-CHATTY-IO-ANTI-PATTERN
[IEnumerableVsIQueryable]: https://www.sellsbrothers.com/posts/Details/12614
[full-product-table]:Figures/ProductTable.jpg
[product-sales-tables]:Figures/SalesPersonAndSalesOrderHeaderTables.jpg
[Load-Test-Results-Client-Side]:Figures/LoadTestResultsClientSide.jpg
[Load-Test-Results-Database-Side]:Figures/LoadTestResultsDatabaseSide.jpg
[QueryPerformanceZoomed]: Figures/QueryPerformanceZoomed.jpg
[QueryDetails]: Figures/QueryDetails.jpg
