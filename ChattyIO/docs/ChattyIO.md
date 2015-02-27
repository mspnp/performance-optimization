# Chatty I/O

Network calls and other I/O operations are inherently slow compared to compute tasks. Each I/O request typically incorporates significant overhead, and the cumulative effect of a large number of requests can have a significant impact on the performance and responsiveness of the system.

Common examples of chatty I/O operations include:

- Reading and writing individual records to a database as seperate requests. The following code snippet shows part of a controller in a service implemented by using Web API. The example is based on the AdventureWorks2012 database. In this database, products are grouped into subcategories and are held in the `Product` and `ProductSubcategory` tables respectively. Pricing information for each product is held in a seperate table named `ProductListPriceHistory`. The  `GetProductsInSubCategoryAsync` method shown below retrieves the details for all products (including price information) for a specified subcategory. The method achieves this by using the Entity Framework to perform the following operations:

	- Fetch the details of the specified subcatgeory from the `ProductSubcategory` table,

	- Find all products in the subcategory by querying the `Product` table,

	- For each product, retrieve the price data from the `ProductPriceListHistory` table. 

**C# web API**

```C#
public class ChattyProductController : ApiController
{
    [HttpGet]
    [Route("chattyproduct/products/{subcategoryId}")]
    public async Task<ProductSubcategory> GetProductsInSubCategoryAsync(int subcategoryId)
    {
        ProductSubcategory productSubcategory = null;

        using (var context = GetContext())
        {
            productSubcategory = await context.ProductSubcategories
                 .Where((psc) => psc.ProductSubcategoryId == subcategoryId)
                 .SingleOrDefaultAsync();
            productSubcategory.Product = await context.Products
                .Where((p) => subcategoryId == p.ProductSubcategoryId)
                .ToListAsync();

            foreach (var prod in productSubcategory.Product)
            {
                var productListPriceHistory = await context.ProductListPriceHistory
                   .Where((pl) => pl.ProductId == prod.ProductId)
                   .ToListAsync();
                prod.ProductListPriceHistory = productListPriceHistory;
            }

        }
        return productSubcategory;
    }
    ...
}
```

----------

**Note:** This code forms part of the [ChattyIO sample application][fullDemonstrationOfProblem].

----------

- Sending a series of requests that constitute a single logical operation to a web service. This often occurs when a system attempts to follow an object-oriented paradigm and handle remote objects as though they were local items held in application memory. This approach can result in an excessive number of network round-trips. For example, the following web API exposes the individual properties of `User` objects through a REST interface. Each method in the REST interface takes a parameter that identifies a user. While this approach is efficient if an application only needs to obtain one selected piece of information, in many cases it is likely that the application will actually require more than one property of a `User` object, resulting in multiple requests as shown in the C# client code snippet.

**C# web API**

```C#
public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public char? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    ...
}
...
public class UserController : ApiController
{
    ...
    // GET: /Users/{id}/UserName
    [HttpGet]
    [Route("users/{id:int}/username")]
    public HttpResponseMessage GetUserName(int id)
    {
        ...
    }

    // GET: /Users/{id}/Gender
    [HttpGet]
    [Route("users/{id:int}/gender")]
    public HttpResponseMessage GetGender(int id)
    {
        ...
    }

    // GET: /Users/{id}/DateOfBirth
    [HttpGet]
    [Route("users/{id:int}/dateofbirth")]
    public HttpResponseMessage GetDateOfBirth(int id)
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
HttpResponseMessage response = await client.GetAsync("users/1/username");
response.EnsureSuccessStatusCode();
var userName = await response.Content.ReadAsStringAsync();

response = await client.GetAsync("users/1/gender");
response.EnsureSuccessStatusCode();
var gender = await response.Content.ReadAsStringAsync();

response = await client.GetAsync("users/1/dateofbirth");
response.EnsureSuccessStatusCode();
var dob = await response.Content.ReadAsStringAsync();
...
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

Symptoms of chatty I/O include high latency and low throughput. End-users are likely to report extended response times and possible failures caused by services timing out due to increased contention for I/O resources.

### Load-testing the application

Performing load-testing against the `GetProductsInSubCategoryAsync` in the `ChattyProduct` controller in sample application yields the following results:

![Key indicators load-test results for the Chatty I/O sample application][key-indicators-chatty-io]

This load test was performed by using a simulated workload of 500 concurrent users. The graph shows a high degree of latency; apart from the ramp-up period at the start of the test and a spike caused by errors at the end, the median time was between 40 and 50 seconds per request (the average page time can be discounted in this case). If each test represents a user querying the product catalog to find the details of products in a specified subcategory then a user might have to wait for nearly a minute to see the results under this load.

----------

**Note:** The application was deployed as a web role on a single-instance cloud service. The results shown in the [Consequences of this solution](#Consequences) section were generated using the same load on the same deployment. A production system would have more nodes and should be able to support more users, but the general principle remains the same.

----------

### Monitoring the application

**APPDYNAMICS INFORMATION TO BE ADDED HERE**

### Using data access statistics

Examining data access statistics and other information provided by a data store acting as the repository can yield some useful information, such as which queries being repeated most frequently. For example, Windows Azure SQL Database provides access to query statistics using the Query Performance pane in the management portal. This pane shows information about all recently executed queries:

![The Query Performance pan in the Windows Azure SQL Database management portal][QueryPerformance1]

The `Run Count` column in the results indicates how frequently each query is run. In this case, the following queries have been executed a significant number of times:

**SQL**
```SQL

/* Executed 80128 times by the load test */
SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[StartDate] AS [StartDate], 
    [Extent1].[EndDate] AS [EndDate],
    [Extent1].[ListPrice] AS [ListPrice],
    FROM [Production].[ProductListPriceHistory] AS [Extent1]
    WHERE [Extent1].[ProductID] = @p__linq__0


/* Executed 2499 times by the load test */
SELECT 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[ProductNumber] AS [ProductNumber], 
    [Extent1].[ListPrice] AS [ListPrice], 
    [Extent1].[ProductSubcategoryId] AS [ProductSubcategoryId]
    FROM [Production].[Product] AS [Extent1]
    WHERE @p__linq__0 = [Extent1].[ProductSubcategoryId]


/* Executed 2502 times by the load test */
SELECT TOP (2) 
    [Extent1].[ProductSubcategoryID] AS [ProductSubcategoryID], 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductSubcategory] AS [Extent1]
    WHERE [Extent1].[ProductSubcategoryID] = @p__linq__0
```

These queries correspond to the requests generated by the Entity Framework.

### Profiling the application

Profiling an application can help to identify the following symptoms that characterize chatty I/O operations. The exact symptoms will depend on the nature of the resources being accessed but may include:

- A large number of small I/O requests made to the same file.

- A large number of small network requests made by an application instance to the same service.

- A large number of small requests made by an application instance to the same data store.

- Applications and services becoming I/O bound.

## How to correct the problem

Reduce the number of I/O requests by packaging the data that your application reads or writes into larger, fewer requests, as illustrated by the following examples:

- In the ChattyIO example, rather than retrieving data by using separate queries, fetch the information required in a single query as shown in the `ChunkyProduct` controller below:

**C# web API**

```C#
public class ChunkyProductController : ApiController
{
    [HttpGet]
    [Route("chunkyproduct/products/{subCategoryId}")]
    public async Task<ProductSubcategory> GetProductCategoryDetailsAsync(int subCategoryId)
    {
        using (var context = GetContext())
        {
            var subCategory = await context.ProductSubcategories
                  .Where((psc) => psc.ProductSubcategoryId == subCategoryId)
                  .Include("Product.ProductListPriceHistory")
                  .SingleOrDefaultAsync();
            return subCategory;
        }
    }
    ...
```

- In the `UserController` example shown earlier, rather than exposing individual properties of User objects through the REST interface, provide a method that retrieves an entire User object within a single request.

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

- In the file I/O example, you could buffer data in memory and provide a separate operation that writes the buffered data to the file as a single operation. This approach reduces the overhead associated with repeatedly opening and closing the file, and helps to reduce fragmentation of the file on disk.

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

## <a name="Consequences"></a>Consequences of the solution
The system should spend less time performing I/O, and contention for I/O resources should be decreased. This should manifest itself as an improvement in response time and throughput in an application. Performing load-testing against the Chatty I/O sample application by using the `chunky` API yields the following results:

![Key indicators load-test results for the Chunky API in the Chatty I/O sample application][key-indicators-chunky-io]

This load test was performed on the same deployment and using the same simulated workload of 500 concurrent users as before. This time the graph shows much lower latency; discounting the errors at the end, each request takes between 7 and 8 seconds on average. A user querying the product catalog to find the details of products in a specified subcategory will now wait for less than 10 seconds to see the results.

**I AM STILL GENERATING THE STATS AND QUERY FOR THIS FINAL PART. I WILL FILL THEM IN TOMORROW**
For comparison purposes, examining the query access statistics shows that the following query was run XYZ times. This query is executed in place of the three SELECT statements shown earlier, and returns the combined data for all products and price information in a given subcategory:

**SQL**
```SQL

/* Executed XYZ times by the load test */
SELECT 

```

## Related resources

- [Data Consistency guidance][data-consistency-guidance].

- [Caching Guidance][caching-guidance]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[data-consistency-guidance]: http://LINK-TO-CONSISTENCY-GUIDANCE-WHEN-PUBLISHED
[caching-guidance]: https://msdn.microsoft.com/library/dn589802.aspx
[key-indicators-chatty-io]: Figures/ChattyIO.jpg
[key-indicators-chunky-io]: Figures/ChunkyIO.jpg
[QueryPerformance1]: Figures/QueryPerf1.jpg

