# No Caching

The purpose of caching is to avoid repeatedly retrieving the same information from a resource that is expensive to access, and/or to reduce the need to expend processing resources constructing the same items when they are required by multiple requests. In a cloud service that has to handle many concurrent requests, the overhead associated with repeated operations can impact the performance and scalability of the system. Additionally, if the resource becomes unavailable, then the cloud service may fail when it attempts to retrieve information; using a cache to buffer data can help to ensure that cached data remains available even if the resource is not.

The following code snippet shows an example method that uses the Entity Framework to connect to the [AdventureWorks2012][AdventureWorks2012] sample database implemented by using Azure SQL Database. The method then fetches the details of a customer (returned as a `Person` object) specified by the `id` parameter. Each time this method runs, it incurs the expense of communicating with the database. In a system designed to support multiple concurrent users, separate requests might retrieve the same information from the database. The costs associated with repeated requests can accumulate quickly. Additionally, if the system is unable to connect to the database for some reason then requests will fail:


**C#**

```C#
public async Task<Person> GetByIdAsync(int id)
{
    using (AdventureWorksContext context = new AdventureWorksContext())
    {
        return await context.People
            .Where(p => p.BusinessEntityId == id)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
```

This code forms part of the [CachingDemo sample application][fullDemonstrationOfProblem].

This anti-pattern typically occurs because:

- It is easier to write code that reads and writes data directly to a data store.

- There is a perception that users always demand to see the most recent data, and caching  may lead to them being presented with out-of-date information.

- There is a concern over the overhead of maintaining the accuracy and freshness of cached data and the coding complications that this might entail.

- Direct access to data might form part of an on-premises system where network latency is not an issue, the system runs on expensive high-performance hardware, and caching is not considered. If this system is migrated to the cloud network latency is increased, and it is typically hosted on commodity hardware in a remote datacenter. An explicit decision needs to be made to explore the possible performance benefits of caching.

- A lack of awareness that caching is a possibility in a given scenario. A common example concerns the use of ETags when implementing a web API. 

- The benefits (and sometimes the drawbacks) of using a cache are misunderstood.


## How to detect the problem
A complete lack of caching can lead to poor response times when retrieving data due to the latency when accessing a remote data store, increased contention in the data store, and an associated lack of scalability as more users request data from the store. 

### Reviewing the application

If you are a designer or developer familiar with the structure of the application, and you are aware that the application does not use caching, then this is often an indication that adding a cache might be useful. To identify the information to cache, you need to determine exactly which resources are likely to be accessed most frequently. Slow changing or static reference data that is read frequently are good initial candidates; this could be data retrieved from storage or returned from a web application or remote service. However, all resource access should be verified to ascertain which resources are most suitable; depending on the nature of the application, fast-moving data that is written frequently may also benefit from caching (see the considerations in the [How to correct the problem](#HowToCorrectTheProblem) section for more information.)

----------

**Note:** Remember that a cached resource does not have to be a piece of data retrieved from a data store; it could also be the results of an often-repeated computation.

----------

### Load-testing the application
Performing load testing can help to highlight any problems. Load-testing should simulate the pattern of data access observed in the production environment. You can use a tool such as [Visual Studio Online][VisualStudioOnline] to run load tests and examine the results. 

The following output was generated from load-testing the CachingDemo sample application without using caching. The load-test simulates 500 concurrent users performing a series of HTTP GET operations targeted at 99 different resources (`Person 1` through `Person 99`). The test runs for 5 minutes, generating many thousands of requests.

The first graph shows the performance summary. In this graph, the average response time for each request was 16.42 seconds, and after the initial warm-up phase of the test the CPU utilization was relatively low (approximately 5%); this is because much of the time the tests are waiting for the I/O caused by repeatedly fetching data to complete:

![Performance load-test results for the uncached scenario][Performance-Load-Test-Results-Uncached]

The second graph illustrates the response times for each individual request. It shows the details for each request, and confirms that the average response time was approximately 16.5 seconds:

![Page response times for the uncached scenario][Page-Response-Times-Uncached]

Finally, the tabular summary of the load-test indicates that in 5 minutes, 500 users were able to make 8844 successful requests:

![Performance summary for the uncached scenario ][Performance-Summary-Uncached]

### Using data access statistics

Examining data access statistics and other information provided by a data store acting as the repository might show the same queries being repeated frequently. For example, Microsoft SQL Server provides the `sys.dm_exec_query_stats` management view which contains statistical information for recently executed queries. The text for each query is available in the `sys.dm_exec-query_plan` view. You can use the following SQL query to determine the frequency with which queries are performed:

**SQL**
```SQL
SELECT UseCounts, Text, Query_Plan
FROM sys.dm_exec_cached_plans 
CROSS APPLY sys.dm_exec_sql_text(plan_handle)
CROSS APPLY sys.dm_exec_query_plan(plan_hand
```

In the [CachingDemo sample application][fullDemonstrationOfProblem], the following query corresponds to the request generated by the Entity Framework code shown earlier:

**SQL**
```SQL
(@p__linq__0 int)SELECT TOP (2) 
[Extent1].[BusinessEntityId] AS [BusinessEntityId], 
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName]
FROM [Person].[Person] AS [Extent1]
WHERE [Extent1].[BusinessEntityId] = @p__linq__0
```

This query is repeated each time the `GetByIdAsync` method runs. The value of the `id` parameter passed in to this method replaces the `p__linq__0` parameter in the SQL query.

### Instrumenting the application

While statistical information from the data store may be useful to identify common queries, you should also consider instrumenting applications to provide more information about specific requests while the application is in production. You can then analyze the results to group them by operation. You can use lightweight logging  frameworks such as [NLog][NLog] or [Log4Net][Log4Net] to gather this information. You can also deploy more powerful tools such as [Microsoft Application Insights][AppInsights], [New Relic][NewRelic], or [AppDynamics][AppDynamics] to collect and analyze instrumentation information, but these tools incur more overhead than simple logging, and they should be disabled when not required. 

As an example, if you configure the CachingDemo to capture monitoring data by using [Application Insights][AppInsights], the analytics generated can quickly show you the frequency with which each server request occurs, as shown by the image below. Three different HTTP GET operations (`Person/Get`, `Person/GetAsync`, and `Person/GetCached`) have been performed while the application has been running: 

![AppInsights showing server requests for the CachingDemo application][AppInsights-server-requests]

In this example, `Person\Get` accounts for 68.8% of all requests. You can drill into this request to obtain more information and view the specific instances of this request, including the full URL which identifies the resource being retrieved. A useful feature here is the ability to filter the data. The `URL base` filter displays a count of the number of times each resource has been accessed. In the following image, each resource has been retrieved at least 190 times. The data is filtered by using the URL base `/api/Person/Get/10`, and the Diagnostic search pane displays the details for each access, including the time the access was made. If these accesses occur within a relatively short period of time, then the data returned by accessing this resource is a good candidate for caching. Note that this data may be an entity, or it could be the results of a computation:

![AppInsights showing the details of requests for the Person 10 resource][AppInsights-GET-Person-10-requests]

### Profiling the application

If you require a deeper analysis of the application you can use a profiler such as [PerfView][PerfView] or [ANTS][ANTS] to capture and examine low-level performance information such as I/O request rates, memory usage, and CPU utilization. Performing a detailed profiling of the system may reveal a large number of requests to a data store or service, or repeated processing that performs the same calculation. You can sort the profile data by the number of requests to help identify candidate information for caching. Profiling might not provide the same data under the same load as a production system, and the results can be skewed as a result of the additional profiling overhead. However, you may be able to adjust the results to take this overhead into account.

## <a name="HowToCorrectTheProblem"></a>How to correct the problem
You can use several strategies to implement caching. The most popular is the *on-demand* or [*cache-aside*][cache-aside-pattern] strategy. In this strategy, the application attempts to retrieve data from the cache. If the data is not present, the application retrieves it from the data store and adds it to the cache so it will be found next time. To prevent the data from becoming stale, many caching solutions support configurable timeouts, allowing data to automatically expire and be removed from the cache after a specified interval. If the application modifies data, it should write the change directly to the data store and remove the old value from the cache; it will be retrieved and added to the cache the next time it is required. 

This approach is suitable for data that changes regularly, although there may be a window of opportunity during which an application might be served with out-of-date information. The following code snippet shows the `GetByIdAsync` method presented earlier but now including the cache-aside pattern.

----------

**Note:** This snippet is taken from the [sample code][fullDemonstrationOfSolution] available with this anti-pattern. The sample code uses [Azure Redis Cache][Azure-cache] to store data and the [StackExchange.Redis][StackExchange] client library to communicate with the cache.

----------


**C#**

```C#

public async Task<Person> GetByIdAsync(int id)
{
    // Connect to Azure Redis Cache
    IDatabase cache = ...;

    // Attempt to retrieve the product from the cache
    string key = string.Format("{0}:{1}", ..., id); // Azure Redis Cache expects the key to be passed as a formatted string
    Person value = await cache.GetAsync<T>(key).ConfigureAwait(false);

    // If the item is not currently in the cache, then retrieve it from the database and add it to the cache
    if (value == null)
    {
        value = ...; // Retrieve the data from the database
        if (value != null)
        {
            await cache.SetAsync(key, value).ConfigureAwait(false); // Add the data to the cache
        }
    }

    return value;
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

To determine the efficacy of any caching strategy, you should repeat load-testing after incorporating a cache and compare the results to those generated before the cache was added. The following results are the output generated by load testing the CachingDemo sample solution with caching enabled. The performance summary graph now shows that the average response time for each request was 0.38 seconds. This is an enormous improvement (by a factor of 40) over the average response time for the uncached example. Also, the CPU utilization was much higher throughout the test as less time was being spent waiting for I/O.:

![Performance load-test results for the cached scenario][Performance-Load-Test-Results-Cached]

However, response times should not be considered the only measure of success. The increased volume of requests may have an adverse affect on a service, resulting in transient availability errors if it becomes overloaded at any point. The tabular summary of the load-test indicates that although the system now handled 300,000 requests in 5 minutes (compared to 8844 previously), a large proportion of these requests failed, and only 53,902 were successful:

![Performance summary for the cached scenario][Performance-Summary-Cached]

The excess traffic caused a large number of HTTP 403 (Forbidden) messages, indicating that the web site hosting the service was temporarily unavailable. **Therefore, you should be prepared to scale the web site to support the additional traffic.**

## Related resources
- [The Cache-Aside Pattern][cache-aside-pattern].

- [Data Consistency guidance][data-consistency-guidance].

- [API Implementation guidance][API-implementation-guidance].

- [Azure Cache documentation][Azure-cache].

- [The StackExchange.Redis client library][StackExchange]

- [Microsoft Application Insights][AppInsights]

- [NLog logging for the .NET Framework][Nlog]

- [Apache log4net logging for the .NET runtime][Log4Net]

- [New Relic and Windows Azure][NewRelic]

- [AppDynamics Windows Azure monitoring][AppDynamics]

- [PerfView performance analysis tool][PerfView]

- [ANTS performance profiler][ANTS]

- [Visual Studio Online load testing in the cloud][VisualStudioOnline]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[AdventureWorks2012]:http://msftdbprodsamples.codeplex.com/releases/view/37304
[StackExchange]: https://github.com/StackExchange/StackExchange.Redis
[cache-aside-pattern]: https://msdn.microsoft.com/library/dn589799.aspx
[data-consistency-guidance]: http://LINK-TO-CONSISTENCY-GUIDANCE
[Azure-cache]: http://azure.microsoft.com/documentation/services/cache/
[API-implementation-guidance]: http://LINK-TO-API-IMPLEMENTATION-GUIDANCE
[AppInsights]: http://azure.microsoft.com/documentation/articles/app-insights-get-started/
[NLog]: http://nlog-project.org
[Log4Net]: http://logging.apache.org/log4net
[NewRelic]: http://newrelic.com/azure
[AppDynamics]: http://www.appdynamics.co.uk/cloud/windows-azure
[PerfView]: http://blogs.msdn.com/b/vancem/archive/2011/12/28/publication-of-the-perfview-performance-analysis-tool.aspx
[ANTS]: http://www.red-gate.com/products/dotnet-development/ants-performance-profiler/
[VisualStudioOnline]: http://www.visualstudio.com/get-started/load-test-your-app-vs.aspx
[AppInsights-server-requests]: http://i.imgur.com/4vUXXSb.jpg
[AppInsights-GET-Person-10-requests]: http://i.imgur.com/gcJhG2m.jpg
[Performance-Load-Test-Results-Uncached]: http://i.imgur.com/dMi4sKD.jpg
[Page-Response-Times-Uncached]: http://i.imgur.com/k2alJTO.jpg
[Performance-Summary-Uncached]: http://i.imgur.com/ceZLuCg.jpg
[Performance-Load-Test-Results-Cached]: http://i.imgur.com/tNHaAYa.jpg
[Performance-Summary-Cached]: http://i.imgur.com/chA8iDu.jpg