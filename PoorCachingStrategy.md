# Poor Caching Strategy

The purpose of caching is to avoid repeatedly retrieving the same information from a resource that is expensive to access, and/or to reduce the need to expend processing resources constructing the same items when they are required by multiple requests. In a cloud service that has to handle many concurrent requests, the overhead associated with repeated operations can impact the performance and scalability of the system. Additionally, if the resource becomes unavailable, then the cloud service may fail when it attempts to retrieve information; using a cache to buffer data can help to ensure that cached data remains available even if the resource is not.

The following code snippet shows an example method that uses the Entity Framework to connect to a database implemented by using Azure SQL Database. The method then fetches an item specified by the `productID` parameter. Each time this method runs, it incurs the expense of communicating with the database. In a system designed to support multiple concurrent users, separate requests might retrieve the same information from the database. The costs associated with repeated requests can accumulate quickly. Additionally, if the system is unable to connect to the database for some reason then requests will fail:


**C#**

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

You should also be aware that in many situations a caching strategy that attempts to cache highly volatile information can be just as bad for performance and scalability as not caching any data; the system wastes memory and processing resources maintaining the information in the cache when it might be more efficient to simply retrieve the data from the original source.

This anti-pattern typically occurs because:

- It is easier to write code that reads and writes data directly to a data store. 
- There is a perception that users always demand to be presented with the most recent data, and caching  may lead to them being presented with out-of-date information.
- There is a concern over the overhead of maintaining the accuracy and freshness of cached data and the coding complications that this might entail.
- Direct access to data might form part of a functional prototype that operates in-house, but is not addressed (or is forgotten) when the system is further developed and deployed to the cloud.
- A lack of awareness that caching is a possibility in a given scenario. A common example concerns the use of etags when implementing a web API. This scenario is described further in the section **How to correct the problem** later in this pattern
- The benefits (and sometimes the drawbacks) of using a cache are misunderstood.
- *OTHERS?*

[Link to the related sample][fullDemonstrationOfProblem]

## How to detect the problem
A complete lack of caching can lead to poor response times when retrieving data due to the latency when accessing a remote data store, increased contention in the data store, and an associated lack of scalability as more users request data from the store.

Conversely, over-eager caching (caching data that is highly volatile or that is unlikely to be used subsequently) can lead to the system spending a high proportion of its time managing cached data rather than performing useful work.

An operator monitoring a system that implements a poor (or non-existent) caching strategy may observe the following phenomena:

- Profiling the system may indicate a large number of requests to a data store or service. You can sort the profile data by the number of requests to help identify candidate information for caching. In a system that is not caching data sufficiently, you might see a large number of requests for reference data.
- Examining data access statistics and other information provided by a data store might show the same queries being repeated frequently.
- *Further notes on what to look for when profiling an application*.
- *Notes on key perf counters and metrics - e.g. high network latency with idle threads blocked waiting for the results. Lots of I/O into the cache. Lots of data expiring in the cache, or the cache being flushed very frequently.*

## How to correct the problem
You can use several strategies to implement caching. The most popular are:

- The *on-demand* or [*cache-aside*][cache-aside] strategy. The application attempts to retrieve data from the cache. If the data is not present, the application retrieves it from the data store and adds it to the cache so it will be found next time. To prevent the data from becoming stale, many caching solutions support configurable timeouts, allowing data to automatically expire and be removed from the cache after a specified interval. If the application modifies data, it should write the change directly to the data store and remove the old value from the cache; it will be retrieved and added to the cache the next time it is required. This approach is suitable for data that may change regularly, although there may be a window of opportunity during which an application might be served with out-of-date information. The following code snippet shows the `RetrieveAsync` method presented earlier but now including the cache-aside pattern.

    **Note:** For simplicity, this example uses the `MemoryCache` class which stores data in process memory, but the same technique is applicable to other caching technologies.

**C#**

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

- The *background data-push* strategy. A background service populates the cache and pushes modified data into the cache on a regular schedule. An application reading data always retrieves it from the cache. Modifications are written directly back to the data store. This approach is most suitable for very static data or for situations that don't always require the most recent information. This strategy is useful for priming a cache when it is first created or whan an application that is likely to use this data starts running.
**QUESTION: SHOULD WE INCLUDE SOME CODE FOR THIS STRATEGY?**

If you are building REST web services, you should understand that caching is an important part of the HTTP protocol that can greatly improve service performance When a client application requests an object from a REST web service, the response message can include an ETag (Entity Tag). An ETag is an opaque string that indicates the version of an object; each time an object changes the Etag is also modified (how the ETag is generated and updated is an implementation detail ofthe web service). This ETag should be saved locally by the client application. If the client issues a subsquent request for the same item, it should include the ETag as part of the request. The web service can then determine whether the object has changed since it was last retrieved. If the current ETag for the object is the same as that specified by the client, then the web service can simply return a response that indicates that the data is unchanged. However, if the current ETag is different, the web service can return the new data together with the new ETag. In this way, if an object is large, using ETags can save the time and resources required to transmit the object back to the client.

**QUESTION: SHOULD WE INCLUDE SOME CODE FOR THE ETAG SCENARIO?**

You should consider the following points when determining how and whether to implement caching:

- Your application code should not depend on the availability of the cache. If it is inaccessible your code should not fail, but instead it should fetch data from the the original data source. **REFERENCE THE INCORRECT SERVICE FAILURE HANDLING ANTI-PATTERN?**
- You don't have to cache entire entities. If the bulk of an entity is static but only a small piece is subject to regular changes, then cache the static elements and  retrieve only the dynamic pieces from the data source. This approach can help to reduce the volume of I/O being performed against the data source.
- The possible differences between cached data and data held in the underlying data source mean that applications that use caching for non-static data should be designed to support [eventual consistency][eventual-consistency].
- In some cases caching volatile information can prove to be helpful if this information is temporary in nature. For example, consider a device that continually reports status information or some other measurement. If an application chooses not to cache this data on the basis that the cached information will nearly always be outdated, then the same consideration could be true when storing and retrieving this information from a data store; in the time taken to save and fetch this data it may have changed. In a situation such as this, consider the benefits of storing the dynamic information directly in the cache instead of a persistent data store. If the data is non-critical and does not require to be audited, then it does not matter if the occasional change is lost.
- Use a portable serialization format for cached data that is independent of an specific application.
- Caching doesn't just apply to data held in a remote data source. You can use caching to save the results of complex computations that are performed regularly. In this way, rather than expending processing resources (and time) repeating such a calculation, an application might be able to retrieve results computed earlier.
- Use an appropriate caching technology. If you are building Azure cloud services or web applications, then using an in-memory cache may not be appropriate because client requests might not always be routed to the same server. This approach also has limited scalability (governed by the available memory on the server). Instead, use a shared caching solution such as [Azure Redis Cache][Azure-Redis-Cache].  Note that in some cases an application may benefit from using a combination of both techniques, with a local in-memory cache storing information that is retrieved from a shared cache. However, it is important to implement policies that reduce the chances of data cached locally from becoming stale with respect to data held in the shared cache.
- Falling back to the original data store if the cache is temporarily unavailable may have a scalability impact on the system; while the cache is being recovered, the original data store could be swamped with requests for data, resulting in timeouts and failed connections. A strategy that you should consider is to implement a local, private cache in each instance of an application together with the shared cache that all application instances access. When the application retrieves an item, it can check first in its local cache, then the shared cache, and finally the original data store. The local cache can be populated using the data in the shared cache, or the database if the shared cache is unavailable. This approach requires careful configuration to prevent the local cache becoming too stale with respect to the shared cache, but it acts as a buffer if the shared cache is unreachable.
- Always include instrumentation that detects cache hits and cache misses. This information can be used to tune caching policies (for example, what to cache, and how long to hold it in the cache before it expires).

[Link to the related sample][fullDemonstrationOfSolution]


## How to validate the solution
TBD.
*(NOTE: NEED TO ADD SOME QUANTIFIABLE GUIDANCE)*

## What problems will this uncover?
*TBD - Need more input from the developers*.


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[cache-aside]: https://msdn.microsoft.com/library/dn589799.aspx
[eventual-consistency]: http://LINK TO CONSISTENCY GUIDANCE
[Azure-Redis-Cache]: http://azure.microsoft.com/documentation/services/cache/