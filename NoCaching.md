# No Caching

The purpose of caching is to avoid repeatedly retrieving the same information from a resource that is expensive to access, and/or to reduce the need to expend processing resources constructing the same items when they are required by multiple requests. In a cloud service that has to handle many concurrent requests, the overhead associated with repeated operations can impact the performance and scalability of the system. Additionally, if the resource becomes unavailable, then the cloud service may fail when it attempts to retrieve information; using a cache to buffer data can help to ensure that cached data remains available even if the resource is not.

The following code snippet shows an example method that uses the Entity Framework to connect to a database implemented by using Azure SQL Database. The method then fetches an item specified by the `productID` parameter. Each time this method runs, it incurs the expense of communicating with the database. In a system designed to support multiple concurrent users, separate requests might retrieve the same information from the database. The costs associated with repeated requests can accumulate quickly. Additionally, if the system is unable to connect to the database for some reason then requests will fail:


**C# - CODE BELOW TO BE REPLACED WITH CODE FROM SAMPLE**

```C#
public async Task<Product> RetrieveAsync(int productID)
{
    try
    {
        using (var productsModelContext = new AdventureWorks2012Entities())
        {
            var product = await productsModelContext.Products.Where(p => p.ProductID == productID).FirstAsync();
            return product;
        }
    }
    catch(Exception e)
    {
        ...
    }
}
```

This anti-pattern typically occurs because:

- It is easier to write code that reads and writes data directly to a data store.

- There is a perception that users always demand to be presented with the most recent data, and caching  may lead to them being presented with out-of-date information.

- There is a concern over the overhead of maintaining the accuracy and freshness of cached data and the coding complications that this might entail.

- Direct access to data might form part of an on-premises system where network latency is not an issue, the system runs on expensive high-performance hardware, and caching is not considered. If this system is migrated to the cloud network latency is increased, and it is typically hosted on commodity hardware in a remote datacenter. An explicit decision needs to be made to explore the possible performance benefits of caching.

- A lack of awareness that caching is a possibility in a given scenario. A common example concerns the use of etags when implementing a web API. This scenario is described further in the section [How to correct the problem](#HowToCorrectTheProblem) later in this pattern

- The benefits (and sometimes the drawbacks) of using a cache are misunderstood.


[Link to the related sample][fullDemonstrationOfProblem]

## How to detect the problem
A complete lack of caching can lead to poor response times when retrieving data due to the latency when accessing a remote data store, increased contention in the data store, and an associated lack of scalability as more users request data from the store.

An operator monitoring a system that implements a poor (or non-existent) caching strategy may observe the following phenomena:

- Profiling the system may indicate a large number of requests to a data store or service. You can sort the profile data by the number of requests to help identify candidate information for caching. In a system that is not caching data sufficiently, you might see a large number of requests for reference data.

- Examining data access statistics and other information provided by a data store might show the same queries being repeated frequently.

- **Further notes on what to look for when profiling an application**.

- **Notes on key perf counters and metrics - e.g. high network latency with idle threads blocked waiting for the results. Lots of I/O into the cache. Lots of data expiring in the cache, or the cache being flushed very frequently.**

## <a name="HowToCorrectTheProblem"></a>How to correct the problem
You can use several strategies to implement caching. The most popular is the *on-demand* or [*cache-aside*][cache-aside-pattern] strategy. In this strategy, the application attempts to retrieve data from the cache. If the data is not present, the application retrieves it from the data store and adds it to the cache so it will be found next time. To prevent the data from becoming stale, many caching solutions support configurable timeouts, allowing data to automatically expire and be removed from the cache after a specified interval. If the application modifies data, it should write the change directly to the data store and remove the old value from the cache; it will be retrieved and added to the cache the next time it is required. 

This approach is suitable for data that may change regularly, although there may be a window of opportunity during which an application might be served with out-of-date information. The following code snippet shows the `RetrieveAsync` method presented earlier but now including the cache-aside pattern.

----------

**Note:** For simplicity, this example uses the `MemoryCache` class which stores data in process memory, but the same technique is applicable to other caching technologies.

----------


**C# - CODE BELOW TO BE REPLACED WITH CODE FROM SAMPLE**

```C#

// Cache for holding product data
private MemoryCache cache = MemoryCache.Default;

...

public async Task<Product> RetrieveAsync(int productID)
{
    // Attempt to retrieve the product from the cache
    var product = cache[productID.ToString()] as Product;

    // If the item is not currently in the cache, then retrieve it from the database and add it to the cache
    if (product == null)
    {
        try
        {
            using (var productsModelContext = new AdventureWorks2012Entities())
            {
                product = await productsModelContext.Products.Where(p => p.ProductID == productID).FirstAsync();
                cache[productID.ToString()] = product;
            }
        }
        catch(Exception e)
        {
            ...
        }
    }

    return product;
}
```

You should consider the following points concerning caching:

- Your application code should not depend on the availability of the cache. If it is inaccessible your code should not fail, but instead it should fetch data from the the original data source.

- You don't have to cache entire entities. If the bulk of an entity is static but only a small piece is subject to regular changes, then cache the static elements and  retrieve only the dynamic pieces from the data source. This approach can help to reduce the volume of I/O being performed against the data source.

- The possible differences between cached data and data held in the underlying data source mean that applications that use caching for non-static data should be designed to support [eventual consistency][data-consistency-guidance].

- In some cases caching volatile information can prove to be helpful if this information is temporary in nature. For example, consider a device that continually reports status information or some other measurement. If an application chooses not to cache this data on the basis that the cached information will nearly always be outdated, then the same consideration could be true when storing and retrieving this information from a data store; in the time taken to save and fetch this data it may have changed. In a situation such as this, consider the benefits of storing the dynamic information directly in the cache instead of a persistent data store. If the data is non-critical and does not require to be audited, then it does not matter if the occasional change is lost.

- If you are building REST web services, include support for client-side caching by providing a cache-header in request and response messages, and identify versions of objects by using ETags.

- Caching doesn't just apply to data held in a remote data source. You can use caching to save the results of complex computations that are performed regularly. In this way, rather than expending processing resources (and time) repeating such a calculation, an application might be able to retrieve results computed earlier.

- Use an appropriate caching technology. If you are building Azure cloud services or web applications, then using an in-memory cache may not be appropriate because client requests might not always be routed to the same server. This approach also has limited scalability (governed by the available memory on the server). Instead, use a shared caching solution such as [Azure Redis Cache][Azure-cache].

- Falling back to the original data store if the cache is temporarily unavailable may have a scalability impact on the system. While the cache is being recovered, the original data store could be swamped with requests for data, resulting in timeouts and failed connections.

- It might be useful to prime the cache on system startup. The cache can be populated with the data that is most likely to be used.

- Always include instrumentation that detects cache hits and cache misses. This information can be used to tune caching policies (for example, what to cache, and how long to hold it in the cache before it expires).

[Link to the related sample][fullDemonstrationOfSolution]

## Consequences of the solution
Implementing caching may lead to a lack of immediate consistency; applications may not always read the most recent version of a data item. Applications should be designed around the principle of eventual consistency and tolerate being presented with old data. Applying time-based eviction policies to the cache can help to prevent cached data from becoming too stale, but any such policy must be balanced against the expected volatility of the data; data that is highly static and read often can reside in the cache for longer periods than dynamic information that may become stale quickly and which should be evicted more frequently.

## Related resources
- [The Cache-Aside Pattern][cache-aside-pattern].

- [Data Consistency guidance][data-consistency-guidance].

- [API Implementation guidance][API-implementation-guidance].

- [Azure Cache documentation][Azure-cache].

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[cache-aside-pattern]: https://msdn.microsoft.com/library/dn589799.aspx
[data-consistency-guidance]: http://LINK-TO-CONSISTENCY-GUIDANCE
[Azure-cache]: http://azure.microsoft.com/documentation/services/cache/
[API-implementation-guidance]: http://LINK-TO-API-IMPLEMENTATION-GUIDANCE