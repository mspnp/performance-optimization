# Retrieving Too Much Data

Applications fetch data for query purposes, or to perform some application-specific processing. Retrieving data, whether from a remote web service, a database, or a file, incurs I/O. Retrieving more data than is necessary to satisfy a business operation can result in unnecessary I/O overhead and can reduce responsiveness. In a cloud environment supporting multiple concurrent instances of an application, this overhead can accumulate to have a significant impact on the performance and scalability of the system.

This anti-pattern typically occurs because:

- The application attempts to minimize the number of I/O requests by retrieving all of the data that it *might* need. This is often a result of overcompensating for the [Chatty I/O][chatty-io] anti-pattern. For example, part of an ecommerce web application that enables a customer to browse products might fetch the complete details for every product that an organization sells. This is wasteful on at least three counts:

	1. The customer might not be interested in every detail; they would typically need to see the product name, description, price, dimensions, and possibly a thumbnail image. Other related information such as product ratings, reviews, and detailed images might be useful but could be expensive and wasteful to retrieve unless the customer specifically requests it.

	2. Not all of the product details might be relevant to the customer; there could be some properties that are only meaningful to the organization or that should remain hidden from customers.

	3. The customer is unlikely to want to view every product that the organization sells.

- The application was developed by following poor programming or design practice. For example, the following diagram shows a very simple Entity Framework data model based on the `Products` table in the sample Adventure-Works database. By default, the Entity Framework includes every column from the underlying database table.
![Entity Framework data model based on the Product table in the AdventureWorks2012 database][full-product-table]

If the application uses this model without specifying any restrictions on the fields it queries, the application effectively performs a `SELECT *` SQL request. This request retrieves the values for every column even if much of this data is not germane to the operation being performed:

**C# Entity Framework**

```C#
var context = new AdventureWorks2012Entities(); // Context object for accessing the data model
var query = from p in context.Products
            select p;  // Effectively performs SELECT * FROM Product
```

Similarly, a REST web service built by using the web API might provide simple HTTP GET operations that do not support filtering, depending on the client application to perform this task after the data has been retrieved and transmitted.

The following example shows part of a REST web service built by using the Web API. The `GetProducts` method responds to HTTP GET requests for product information. This method calls the `GetAllProducts` method of a repository object to find all the products in the database. This list is returned to the client. If the client is only interested in products that match specific criteria, the client application filters the data itself. 

The client code shown below selects products in the *Road Bikes* subcategory and discards the others. However, performing this filtering on the client is clearly wasteful of network bandwidth and database I/O on the web server.

**C# web API**

```C#
// The REST web service returns product details in the following format:
public class ProductInfo
{
    public string ProductName { get; set; }
    public string SubCategory { get; set; }
    public string Color { get; set; }
    public decimal ListPrice { get; set; }
    public string Size { get; set; }
}

public class ProductsController : ApiController
{
    private readonly IProductInfoRepository productInfoRepository;
    ...

    // GET: /Products
    [HttpGet]
    [Route("products")]
    public HttpResponseMessage GetProducts()
    {
        try
        {
            var products = this.productInfoRepository.GetAllProducts();
            return Request.CreateResponse(HttpStatusCode.OK, products);
        }
        catch
        {
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Strings.ServerError);
        }
    }
    ...
}
```

**C# client**

```C#
var client = new HttpClient();

client.BaseAddress = new Uri(...);
...

// Send an HTTP GET request to the /products URI
HttpResponseMessage response = await client.GetAsync("products");
response.EnsureSuccessStatusCode();
var data = await response.Content.ReadAsStringAsync();

// Convert the JSON response into a List
var parsedData = JsonConvert.DeserializeObject<List<ProductInfo>>(data);

// Filter the list to find all Road Bikes
var roadBikes = parsedData.FindAll(product => string.Compare(product.SubCategory, "Road Bikes") == 0);
```

----------

**Note:** The `GetAllProducts` method of the `IProductInfoRepository` object is also indicative of poor practice. This method retrieves data for every product from the underlying data store, which can result in punishing amounts of I/O between the web service and the data store.

----------

- The application uses a framework that supports eager-retrieval of related data. This behavior is commonplace in Object-Relational Mapping (ORM) frameworks. The purpose is to anticipate requests for this data and improve response times, but if this feature is misused to retrieve data that is rarely used it actually imposes an I/O overhead rather being an advantage. If the Entity Model from the previous example is extended to include product review information, it could look like this:

![Entity Framework data model showing the Product and ProductReview tables][product-review-tables]

The application could automatically retrieve review information for each product by using the following code:

**C# Entity Framework**

```C#
var context = new AdventureWorks2012Entities(); // Context object for accessing the data model
var query = from p in context.Products.Include("ProductReviews")
            select ...;
```

The `Includes` method causes the Entity Framework to perform a SQL outer join operation against the `ProductReview` table for each `Product` fetched. This is expensive and wasteful if the customer does not actually wish to see this information.

- The application retrieves data from a data source by using the `IEnumerable` interface. This interface supports filtering and enumeration of data, but the filtering is performed on the client-side after it has been retrieved from the data source. Technologies such as LINQ to Entities (used by the Entity Framework) default to retrieving data through the `IQueryable` interface, which passes the responsibility for filtering to the data source. However, in some situations an application might reference an operation which is only available to the client and not available in the data source, requiring that the data be returned through the `IEnumerable` interface (by applying the `AsEnumerable` method to an entity collection). The following example shows a LINQ to Entities query that retrieves all products where the `SellStartDate` column lies somewhere in the previous week. LINQ to Entities cannot map the `AddDays` function to an operation in the database, so the query returns every row from the product table to the application where it is filtered. If there are only a small number of rows that match this criterion, this is a waste of bandwidth.

**C# Entity Framework**

``` C#
var context = new AdventureWorks2012Entities();
var query = from p in context.Products.AsEnumerable()
            where DateTime.Compare(p.SellStartDate, DateTime.Now.AddDays(-7)) < 0 // AddDays cannot be mapped by LINQ to Entities
            select ...;

List<Product> products = query.ToList();
```


[Link to the related sample][fullDemonstrationOfProblem]

## How to detect the problem
*ADD DETAILS - NEED INPUT FROM THE TEAM*

## How to correct the problem
Only fetch the data that is required, and avoid transmitting large volumes of data that may quickly become outdated or might be discarded and only fetch the data appropriate to the operation being performed. 

The following examples describe possible solutions to many of the scenarios listed earlier:

- In the Entity Framework example, add a `select` projection that limits fields retrieved:


**C# Entity Framework**

```C#
var context = new AdventureWorks2012Entities(); // Context object for accessing the data model
var query = from p in context.Products
            select new
            {
                ProductName = p.Name,
                Color = p.Color,
                ListPrice = p.ListPrice,
                Size = p.Size
            }; // Performs SELECT Name, Color, ListPrice, Size FROM Product
```

- In the REST web service described earlier, you could implement pagination to limit the volume of data returned by a single request; the user might have requested to view all 50,000 products stocked by an organization, but the chances are that the user will stop browsing after the first few pages. Furthermore the web service will likely have consumed considerable resources fetching this amount of data, so this approach is not scalable. The following code snippet shows the `GetProducts` method from the earlier example, but amended to support paging. The parameters specify a limit to the number of rows retrieved, and an offset into the dataset. Both of these parameters are optional, but the limit defaults to a reasonably small level. If necessary, the method could also check that the caller does not specify some ridiculously large value for the `limit` parameter to prevent a malicious waste of resources (for example, by an application performing a Denial of Service attack.) Note that the `GetAllProducts` method in the `ProductInfoRepository` class has been similarly changed to avoid retrieving copious amounts of information from the data store:

**C# web API**

```C#
public class ProductsController : ApiController
{
    private readonly IProductInfoRepository productInfoRepository;
    ...

    // GET: /Products
    [HttpGet]
    [Route("products")]
    public HttpResponseMessage GetProducts(int limit=20, int offset=0)
    {
        // Find the number of products specified by the limit parameter
        // starting with the product specified by the offset parameter

        try
        {
            var products = this.productInfoRepository.GetAllProducts(limit, offset);
            return Request.CreateResponse(HttpStatusCode.OK, products);
        }
        catch
        {
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Strings.ServerError);
        }
    }
    ...
}

```

The client application can retrieve data a page at a time. The following code shows how to fetch the details for 10 products starting at the 20th item:

**C# client**

```C#
var client = new HttpClient();

client.BaseAddress = new Uri(...);
...

// Send an HTTP GET request to the /products URI
HttpResponseMessage response = await client.GetAsync("products?limit=10&offset=20");
response.EnsureSuccessStatusCode();
var data = await response.Content.ReadAsStringAsync();

// Convert the JSON response into a List
var parsedData = JsonConvert.DeserializeObject<List<ProductInfo>>(data);
```


----------

**Note:** If you build REST web services by using the [OData protocol support][OData-support] in Visual Studio, pagination is provided as a built-in feature.

----------



- To support server-side filtering of data based on criteria such as the subcategory, you could include the subcategory as an optional parameter to the `GetProducts` method following the same strategy as the `limit` and `offset` parameters. However, a more natural and resource-focussed solution is to consider adding another method to the REST interface; a subcategory is a resource, and products are assigned to a named subcategory are related to that resource. The following code snippets show the `GetProductsBySubcategory` method that responds to requests made to the *Subcategories/{subcategoryname}Products* URI, and the client code that queries this URI to find all Road Bikes. The server utilizes an additional method of the repository, `GetProductsForSubcategory` that retrieves only the products for the specified subcategory:

**C# web API**

```C#
public class ProductsController : ApiController
{
    private readonly IProductInfoRepository productInfoRepository;
    ...

    // GET: /Subcategories/{SubcategoryName}/Products
    [HttpGet]
    [Route("subcategories/{subcategoryname}/products")]
    public HttpResponseMessage GetProductBySubcategory(string subcategoryname)
    {
        try
        {
            var products = this.productInfoRepository.GetProductsForSubcategory(subcategoryname);
            return Request.CreateResponse(HttpStatusCode.OK, products);
        }
        catch
        {
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Strings.ServerError);
        }
    }
    ...
}

```

**C# client**

```C#
var client = new HttpClient();

client.BaseAddress = new Uri(...);
...

// Send an HTTP GET request to the /subcategories/Road Bikes/products URI
HttpResponseMessage response = await client.GetAsync("subcategories/Road Bikes/products");
response.EnsureSuccessStatusCode();
var data = await response.Content.ReadAsStringAsync();
...
```

- Carefully monitor any use that your application makes of eager-retrieval. Although this is a common technique that can be used to fetch data in advance and improve response times, it can easily cascade to fetch large amounts of related data that is unlikely to be used. If you are using a framework such as an ORM to automatically fetch data (such as the `Includes` method of the Entity Framework), make sure that you understand how it operates and consider the effects that it might have on I/O.

- Wherever possible, ensure that LINQ queries are resolved by using the `IQueryable` interface rather than `IEnumerable`. This may be a matter of rephrasing a query to use only the features and functions that can be mapped by LINQ to features available in the underlying data source, or adding user-defined functions to the data source that can perform the required operations on the data before returning it. In the example shown earlier, the code can be refactored to remove the problematic `AddDays` function from the query, allowing filtering to be performed by the database:


**C# Entity Framework**

``` C#
var context = new AdventureWorks2012Entities();

DateTime dateSince = DateTime.Now.AddDays(-7);
var query = from p in context.Products
            where DateTime.Compare(p.SellStartDate, dateSince) < 0 // AddDays has been factored out. This criterion can be passed to the database by LINQ to Entities
            select ...;

List<Product> products = query.ToList();
```


[Link to the related sample][fullDemonstrationOfSolution]


## How to validate the solution
*NEED TO ADD SOME QUANTIFIABLE GUIDANCE - NEED INPUT FROM THE TEAM*

## What problems will this uncover?
*TBD - Need more input from the developers*.


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[chatty-io]: http://LINK-TO-CHATTY-IO-ANTI-PATTERN
[odata-support]: http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options
[full-product-table]:http://i.imgur.com/lFCHegw.jpg
[product-review-tables]:http://i.imgur.com/9erP9E0.jpg
