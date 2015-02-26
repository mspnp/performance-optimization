# Chatty I/O

Network calls and other I/O operations are inherently slow compared to compute tasks. Each I/O request typically incorporates significant overhead, and the cumulative effect of a large number of requests can have a significant impact on the performance and responsiveness of the system.

Common examples of chatty I/O operations include:

- Sending a series of requests that constitute a single logical operation to a web service. This often occurs when a system attempts to follow an object-oriented paradigm and handle remote objects as though they were local items held in application memory. This approach can result in an excessive number of network round-trips. For example, the following web API mirrors the structure of product categories, subcategories, products, and price histories in the AdventureWorks2012 database by exposing the contents of each table as separate REST resources (a product category contains one or more subcategories, a subcategory contains one or more products, and each product has one or more product price history records). 

**C# web API**

```C#
public class ChattyProductController : ApiController
{
    [HttpGet]
    [Route("chattyproduct/{categoryId}")]
    // Return the details of the specified product category
    public async Task<ProductCategory> GetProductCategoryAsync(int categoryId)
    {
        ...
    }

    [HttpGet]
    [Route("chattyproduct/productsubcategories/{categoryId}")]
    // Return a list of the subcategories in the specified category
    public async Task<IEnumerable<ProductSubcategory>> GetProductSubCategoriesInCategoryAsync(int categoryId)
    {
        ...
    }
       
    [HttpGet]
    // Return a list of products in the specified subcategory
    [Route("chattyproduct/products/{subcategoryId}")]
    public async Task<IEnumerable<Product>> GetProductsInSubCategoryAsync(int subcategoryId)
    {
        ...
    }

    [HttpGet]
    // Return a list of the product price history records for the specified product
    [Route("chattyproduct/productlistpricehistory/{productId}")]
    public async Task<IEnumerable<ProductListPriceHistory>> GetProductListPriceHistoryAsync(int productId)
    {
        ...
    }

    ...
}
```

----------

**Note:** This code forms part of the [ChattyIO sample application][fullDemonstrationOfProblem].

----------

If the user is running a web application that displays the product catalog organized by category, each operation has to perform a number of requests:

1. Retrieve the details of the product category,

2. Retrieve the details of every subcategory in this category,

3. For each subcategory, retrieve the details of every product in this subcategory,

4. For each product, retrieve the product price history.


This process results in multiple requests as shown in the C# client code snippet below:

**C# client**

```C#
var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("...");
...
// Retrieve the details of the product category
var categoryResponse = await httpClient.GetAsync("chattyproduct/" + categoryId);
var category = await categoryResponse.Content.ReadAsAsync<ProductCategory>();

// Retrieve the details of every subcategory in this category
var subCategoriesResponse = await httpClient.GetAsync("chattyproduct/productsubcategories/" + category.ProductCategoryId);
var subCategories = await subCategoriesResponse.Content.ReadAsAsync<IEnumerable<ProductSubcategory>>();

// For each subcategory, retrieve the details of every product in this subcategory
foreach (var subCategory in subCategories)
{
    var productsResponse =
        await httpClient.GetAsync("chattyproduct/products/" + subCategory.ProductSubcategoryId);
    var products = await productsResponse.Content.ReadAsAsync<IEnumerable<Product>>();

    // For each product, retrieve the product price history
    foreach (var product in products)
    {
        var productListPriceResponse =
        await httpClient.GetAsync("chattyproduct/productlistpricehistory/" + product.ProductId);
        var productListPrices =
            await productListPriceResponse.Content.ReadAsAsync<IEnumerable<ProductListPriceHistory>>();
        productListPrices.ToList().ForEach((plp) => product.ProductListPriceHistory.Add((plp)));
    }
}
...
```

- Saving individual records to a database. For example, an application that enables a user to modify multiple records might save changes made to each record individually. The next example shows part of an application built using the Entity Framework. The application stores information about customers' orders in a SQL database. An order can comprise multiple line items. To create a new order, the application creates a `SalesOrderHeader` record together with `SalesOrderDetail` records for each line item. The application saves the changes back to the database after each record has been added. Each time a save operation occurs, the Entity Framework performs the following tasks, each of can incur further network and/or file I/O:

	- Open a connection to the database,
	
	- Start a new transaction,
	
	- Insert the new data into the appropriate table in the database,
	
	- Commit the transaction (assuming that the insert succeeds), and
	
	- Close the connection to the database.

**C#**

```C#
var context = ...;

// Create the sales order header
var header = new SalesOrderHeader()
{
    OrderDate = DateTime.Now,
    CustomerID = 29825,
    ...
};

// Save the new sales order header to the database
context.SalesOrderHeaders.Add(header);
await context.SaveChangesAsync();

// Create the first line item for this sales order
var details1 = new SalesOrderDetail()
{
    ProductID = 776,
    OrderQty = 3,
    ...
};

// Save the line item to the database
header.SalesOrderDetails.Add(details1);
await context.SaveChangesAsync();

// Create the second line item
var details2 = new SalesOrderDetail()
{
    ProductID = 778,
    OrderQty = 2,
    ...
};

// Save the second line item
header.SalesOrderDetails.Add(details2);
await context.SaveChangesAsync();
```

- Reading and writing to a file on disk. Performing file I/O involves opening a file and moving to the appropriate point before reading or writing data. When the operation is complete the file might be closed to save operating system resources. An application that continually reads and writes small amounts of information to a file will generate significant I/O overhead. Note that repeated small write requests can also lead to file fragmentation, slowing subsequent I/O operations still further. The following example shows part of an application that creates new *Customer* objects and writes customer information to a file. The details of each customer are stored by using the *SaveCustomerToFileAsync* method immediately after it is created. Note that the *FileStream* object utilized for writing to the file is created and destroyed automatically by virtue of being managed by a *using* block. Each time the *FileStream* object is created the specified file is opened, and when the *FileStream* object is destroyed the file is closed. If this method is invoked repeatedly as new customers are added, the I/O overhead can quickly accumulate.

**C#**

```C#
[Serializable]
public struct Customer
{
    public int Id;
    public string Name;
    ...
}
...
// Create a new customer and save it to a file
var cust = new Customer(...);
await SaveCustomerToFileAsync(cust);
...
// Save a single customer object to a file
private async Task SaveCustomerToFileAsync(Customer cust)
{
    using (Stream fileStream = new FileStream(CustomersFileName, FileMode.Append))
    {
        BinaryFormatter formatter = new BinaryFormatter();
        byte [] data = null;
        using (MemoryStream memStream = new MemoryStream())
        {
            formatter.Serialize(memStream, cust);
            data = memStream.ToArray();
        }
        await fileStream.WriteAsync(data, 0, data.Length);
    }
}
```


## How to detect the problem

Symptoms of a chatty API include high latency and low throughput. End-users are likely to report extended response times and possible failures caused by services timing out due to increased contention for I/O resources.

### Load-testing the application

Performing load-testing against the Chatty I/O sample application by using the client code shown earlier shows the following characteristics:

![Key indicators load-test results for the Chatty I/O sample application][key-indicators-chatty-io]

This load test was performed by using a simulated workload of 100 concurrent users. The graph shows a high degree of latency (the average test time was 77.3 seconds) and relatively low throughput (1.22 tests per second). If each test represents a user querying the product catalog to find the details of products in a specified category then a user may have to wait for over a minute to see the results.

### Monitoring the application

APPDYNAMICS

### Using data access statistics
Examining data access statistics and other information provided by a data store acting as the repository can yield some useful information, such as which queries being repeated most frequently. For example, Microsoft SQL Server provides the `sys.dm_exec_query_stats` management view which contains statistical information for recently executed queries. The text for each query is available in the `sys.dm_exec-query_plan` view. You can use a tool such as SQL Server Management Studio to run the following SQL query and determine the frequency with which queries are performed:

**SQL**
```SQL
SELECT UseCounts, Text, Query_Plan
FROM sys.dm_exec_cached_plans 
CROSS APPLY sys.dm_exec_sql_text(plan_handle)
CROSS APPLY sys.dm_exec_query_plan(plan_handle)
```


**CHANGE FROM HERE**
The `UseCount` column in the results indicates how frequently each query is run. In the following image, the third query has been run 256049 times; this is significantly more than any other query:

![Results of querying the dynamic management views in SQL Server Management Server][Dynamic-Management-Views]

The text of this query is:

**SQL**
```SQL
(@p__linq__0 int)SELECT TOP (2) 
[Extent1].[BusinessEntityId] AS [BusinessEntityId], 
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName]
FROM [Person].[Person] AS [Extent1]
WHERE [Extent1].[BusinessEntityId] = @p__linq__0
```

In the [CachingDemo sample application][fullDemonstrationOfProblem] shown earlier this query is the result of the request generated by the Entity Framework. This query is repeated each time the `GetByIdAsync` method runs. The value of the `id` parameter passed in to this method replaces the `p__linq__0` parameter in the SQL query.


### Profiling the application

If necessary, profiling an application can help to identify the following symptoms that characterize chatty I/O operations. The exact symptoms will depend on the nature of the resources being accessed:

- Applications and services becoming I/O bound.

- A large number of small network requests made by an application instance to the same service.

- A large number of small requests made by an application instance to the same data store.

- A large number of small I/O requests made to the same file.

## How to correct the problem

Reduce the number of I/O requests by packaging the data that your application reads or writes into larger, fewer requests.

**REWORK THIS BIT**

In the web API example shown earlier, rather than exposing individual properties of *User* objects through the REST interface, provide a method that retrieves an entire *User* object within a single request.

**C# web API**

```C#
public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public char? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
...
public class UserController : ApiController
{
    ...
    // GET: /Users/{id}
    [HttpGet]
    [Route("users/{id:int}")]
    public HttpResponseMessage GetUser(int id)
    {
        ...
    }
}
```

**C# client**

```C#
var client = new HttpClient();
client.BaseAddress = new Uri("...");
...
// Fetch the data for user 1
HttpResponseMessage response = await client.GetAsync("users/1");
response.EnsureSuccessStatusCode();
var user = await response.Content.ReadAsStringAsync();
...
```

In the Entity Framework example, you should save all the changes as a batch by using a single transaction. This approach prevents the application from creating and closing multiple connections and transmitting the data for each operation individually, reducing the volume of I/O required. As a side-benefit, this can also help to maintain consistency; if the `SalesOrderHeader` and `SalesOrderDetail` records are saved individually, there is a possibility that a concurrent process could query this order before all the details have been stored and be presented with incomplete information.

**C#**
```C#
var context = ...;

// Create the sales order header
var header = new SalesOrderHeader()
{
    OrderDate = DateTime.Now,
    CustomerID = 29825,
    ...
};
context.SalesOrderHeaders.Add(header);

// Create the first line item for this sales order
var details1 = new SalesOrderDetail()
{
    ProductID = 776,
    OrderQty = 3,
    ...
};

header.SalesOrderDetails.Add(details1);

// Create the second line item
var details2 = new SalesOrderDetail()
{
    ProductID = 778,
    OrderQty = 2,
    ...
};

header.SalesOrderDetails.Add(details2);

// Save the new sales order header and details to the database in a single transaction
await context.SaveChangesAsync();
```

In the file I/O example, you could buffer data in memory and provide a separate operation that writes the buffered data to the file as a single operation. This approach reduces the overhead associated with repeatedly opening and closing the file, and helps to reduce fragmentation of the file on disk.

**C#**
```C#
[Serializable]
public struct Customer
{
    public int Id;
    public string Name;
    ...
}
...

// In-memory buffer for customers
List<Customer> customers = new List<Customers>();
...

// Create a new customer and add it to the buffer
var cust = new Customer(...);
customers.Add(cust);

// Add further customers to the list as they are created
...

// Save the contents of the list, writing all customers in a single operation
await SaveCustomerListToFileAsync(customers);
...

// Save a list of customer objects to a file
private async Task SaveCustomerListToFileAsync(List<Customer> customers)
{
    using (Stream fileStream = new FileStream(CustomersFileName, FileMode.Append))
    {
        BinaryFormatter formatter = new BinaryFormatter();
        foreach (var cust in customers)
        {
            byte[] data = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                formatter.Serialize(memStream, cust);
                data = memStream.ToArray();
            }
            await fileStream.WriteAsync(data, 0, data.Length);
        }
    }
}
```

As well as buffering data for output, you should also consider caching data retrieved from a service; this can help to reduce the volume of I/O being performed by avoiding repeated requests for the same data. For more information, see the [Caching Guidance][caching-guidance].

You should consider the following points:

- When reading data, do not make your I/O requests too large. An application should only retrieve the information that it is likely to use. It may be necessary to partition the information for an object into two chunks; *frequently accessed data* that accounts for 90% of the requests and *less frequently accessed data* that is used only 10% of the time. When an application requests data, it should be retrieve  the *90%* chunk. The *10%* chunk should only be fetched if necessary. If the *10%* chunk constitues a large proportion of the information for an object this approach can save significant I/O overhead.
- When writing data, avoid locking resources for longer than necessary to reduce the chances of contention during a lengthy operation. If a write operation spans multiple data stores, files, or services then adopt an eventually consistent approach (see [Data Consistency guidance][data-consistency-guidance] for details).
- Data buffered in memory to optimize write requests is vulnerable if the application crashes and may be lost.



[Link to the related sample][fullDemonstrationOfSolution]

## Consequences of the solution
The system should spend less time performing I/O, and contention for I/O resources should be decreased. This should manifest itself as an improvement in response time and throughput in an application. Performing load-testing against the Chatty I/O sample application by using the `chunky` API yields the following results:

![Key indicators load-test results for the Chunky API in the Chatty I/O sample application][key-indicators-chunky-io]

This load test was performed by using the same simulated workload of 100 concurrent users as before. This time the graph lower latency (the average test time was 4.33 seconds compared to 77.3 seconds in the earlier test) and higher throughput (23 tests per second compared to 1.22 in the earlier test). A user querying the product catalog to find the details of products in a specified category will now have to wait under 5 seconds to see the results.

## Related resources

- [Data Consistency guidance][data-consistency-guidance].

- [Caching Guidance][caching-guidance]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[data-consistency-guidance]: http://LINK-TO-CONSISTENCY-GUIDANCE-WHEN-PUBLISHED
[caching-guidance]: https://msdn.microsoft.com/library/dn589802.aspx
[key-indicators-chatty-io]: Figures/ChattyIO.jpg
[key-indicators-chunky-io]: Figures/ChunkyIO.jpg
