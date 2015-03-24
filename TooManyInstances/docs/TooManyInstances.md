# Too Many Instances

Many .NET Framework libraries provide abstractions around external resources. Internally, these classes typically manage their own connections to these external resources, effectively acting as brokers that clients can use to request access to a resource. Examples of such classes frequently used by Azure applications include  `System.Net.Http.HttpClient` to communicate with a web service by using the HTTP protocol, `Microsoft.ServiceBus.Messaging.QueueClient` for posting and receiving messages to a Service Bus queue,  `Microsoft.Azure.Documents.Client.DocumentClient` for connecting to an Azure DocumentDB instance, and `StackExchange.Redis.ConnectionMultiplexer` for accessing Azure Redis Cache.

These *broker* classes can be expensive to create. Instead, they are intended to be instantiated once and reused throughout the life of an application. However, it is common to misunderstand how these classes are intended to be used, and instead treat them as resources that should be acquired only as necessary and released quickly, as shown by the following code snippet that demonstrates the use of the `HttpClient` class in a web API controller. The `GetProductAsync` method retrieves product information from a remote service:

**C# web API**
``` C#
public class NewHttpClientInstancePerRequestController : ApiController
{
    // This method creates a new instance of HttpClient and disposes it for every call to GetProductAsync.

    public async Task<Product> GetProductAsync(string id)
    {
        using (var httpClient = new HttpClient())
        {
            var hostName = HttpContext.Current.Request.Url.Host;
            var result = await httpClient.GetStringAsync(string.Format("http://{0}:8080/api/...", hostName));

            return new Product { Name = result };
        }
    }
}
```

----------

**Note:** This code forms part of the [TooManyInstances sample application][fullDemonstrationOfProblem].

----------

This approach might acceptable for client-side applications where the number of HTTP connections being made is likely to be limited, but in a server-side or web application this technique is not scalable. Each user request results in the creation of a new `HttpClient` object. Under a heavy load, the web server can exhaust the number of sockets available, resulting in `SocketException` errors. 

This problem is not restricted to the `HttpClient` class. Creating many instances of other classes that wrap resources or are expensice to create might cause similar issues, or at least slow down the performance of the application they are continually created and destroyed. As a second example, consider the following code showing an alternative implementation of the `GetProductAsync` method; this time the data is retrieved from an external service wrapped by using the `ExpensiveToCreateService` class:

**C# web API**
``` C#
public class NewServiceInstancePerRequestController : ApiController
{
    // This method creates a new instance of ProductRepository and disposes it for every call to GetProductAsync.
    public async Task<Product> GetProductAsync(string id)
    {
        var expensiveToCreateService = new ExpensiveToCreateService();
        return await expensiveToCreateService.GetProductByIdAsync(id);
    }
}
```

In this code, the `ExpensiveToCreateService` could be any shareable service or broker class that takes considerable effort to construct. As with the `HttpClient` example, continually creating and destroying instances of this class might adversely affect the scalability of the system.

----------

**Note:** The key element of this anti-pattern is that an application repeatedly creates and destroys instances of a **shareable** object. If a class is not shareable (if it is not thread-safe), then this anti-pattern does not apply.

----------

## How to detect the problem

Symptoms of the *Too Many Instances* problem include a drop in throughput, possibly with an increase in exceptions indicating exhaustion of related resources (sockets, database connections, file handles, and so on). End-users are likely to report degraded performance and frequent request failures when the system is heavily utilized.

You can perform the following steps to help identify this problem:

1. Identify points at which response times slow down or the system fails due to lack of resources by performing process monitoring of the production system.

2. Examine the telemetry data captured at these points to determine which operations might be creating and destroying large resource-consuming objects at the point of slow-down.

3. Perform load testing of each of the operations identified by step 3. Use a controlled test environment rather than the production system.

4. Review the source code for the possible operations to identify the points at which broker objects are created and destroyed.

The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you to examine applications and services systematically.

----------

### Identifying points of slow-down

Instrumenting each operation in the production system to track the duration of each request and then monitoring the live system can help to provide an overall view of how the requests perform. You should monitor the the system and track which operations are long-running or cause exceptions.

The following image shows the Overview pane of the New Relic monitor dashboard, highlighting operations that have a poor response time and the increased error rate while these operations are running. In this case, it is operations that invoke the `GetProductAsync` method in the `NewHttpClientInstancePerRequest` controller that are worth investigating further:

![The New Relic monitor dashboard showing the sample application creating a new instance of an HttpClient object for each request][dashboard-new-HTTPClient-instance]

You should also look for operations that trigger increased memory use and garbage collection, as well as raised levels of network, disk, or database activity as connections are made, files are opened, or database connections established.

### Examining telemetry data and finding correlations

You should examine stack trace information for operations that are slow-running or that generate exceptions. This information can help to identify whether they are obtaining and releasing resources of the same type, and exception information can be used to determine whether errors are caused by the system exhausting shared resources. The image below shows information captured by thread profiling for the period corresponding to that shown in the previous image. Note that the system spends a significant time opening socket connections and even more time closing them and handling socket exceptions:

![The New Relic thread profiler showing the sample application creating a new instance of an HttpClient object for each request][thread-profiler-new-HTTPClient-instance]

### Performing load-testing

You can use load-testing based on workloads that emulate the typical sequence of operations that users might perform to help identify which parts of a system suffer with resource-exhaustion under varying loads. You should perform these tests in a controlled environment rather than the production system. The following graph shows the throughput of requests directed at the `NewHttpClientInstancePerRequest` controller in the sample application as the user load is increased up to 100  concurrent users. 

![Throughput of the sample application creating a new instance of an HttpClient object for each request][throughput-new-HTTPClient-instance]

The volume of requests handled per second increases at the 10-user point due to the increased workload up to approximately 30 users. At this point, the volume of successful requests reaches a limit and the system starts to generate errors. The volume of these exceptions gradually increases with the user load. These failures are reported by the load test as HTTP 500 (Internal Server) errors. Reviewing the telemetry (shown earlier) for this test case reveals that these errors are caused by the system running out of socket resources as more and more `HttpClient` objects are created.

The second graph below shows the results of a similar test performed by using the `NewServiceInstancePerRequest` controller. This controller does not use `HttpClient` objects, but instead utilizes a custom object (`ExpensiveToCreateService`) to fetch data:

![Throughput of the sample application creating a new instance of the ExpensiveToCreateService for each request][throughput-new-ExpensiveToCreateService-instance]

This time, although the controller does not generate any exceptions, throughput still reaches a plateau while the average response time increases with user-load. *Note that the scale for the response time and throughput are logarithmic, so the rate at which the response time grows is actually more dramatic than might appear at first glance.* Examining the telemetry for this code should reveal that the main causes of this limitation are the time and resources spent creating new instances of the `ExpensiveToCreateService` for each request.

### Reviewing the code

If you have managed to identify which parts of an application are causing exceptions due to resource exhaustion, perform a review of the code or use profiling to find out how shareable objects are being instantiated, used, and destroyed. Where appropriate, refactor code to cache and reuse objects, as described in the following section.

## How to correct the problem

If the class wrapping the external resource is shareable and thread-safe, create a *pool* of reusable instances of the class. The following code snippet shows a simple example (in this case, the pool contains a single instance). The `SingleHttpClientInstance` controller performs the same operation as the `NewHttpClientInstancePerRequest` controller shown earlier, except that the `HttpClient` object is created once, in the constructor, rather than each time the `GetProductAsync` operation is invoked. This approach reuses the same `HttpClient` object sharing the connection across all requests.

**C# web API**
```C#
public class SingleHttpClientInstanceController : ApiController
{
    private static readonly HttpClient HttpClient;

    static SingleHttpClientInstanceController()
    {
        HttpClient = new HttpClient();
    }

    // This method uses the shared instance of HttpClient for every call to GetProductAsync.
    public async Task<Product> GetProductAsync(string id)
    {
        var hostName = HttpContext.Current.Request.Url.Host;
        var result = await HttpClient.GetStringAsync(string.Format("http://{0}:8080/api/...", hostName));

        return new Product { Name = result };
    }
}
```

----------

**Note:** This code is available in the [sample solution][fullDemonstrationOfSolution] provided with this anti-pattern.

----------

Similarly, assuming that the `ExpensiveToCreateService` was also designed to be shareable, you can refactor the `NewServiceInstancePerRequest` controller in the same manner:

**C# web API**
```C#
public class SingleServiceInstanceController : ApiController
{
    private static readonly ExpensiveToCreateService ExpensiveToCreateService;

    static SingleServiceInstanceController()
    {
        ExpensiveToCreateService = new ExpensiveToCreateService();
    }

    // This method uses the shared instance of ExpensiveToCreateService for every call to GetProductAsync.
    public async Task<Product> GetProductAsync(string id)
    {
        return await ExpensiveToCreateService.GetProductByIdAsync(id);
    }
}
```

You should consider the following points:

- In the example shown above, the *pool* consists of a single `ProductRepository` object, and by extension a single `HttpClient` object. If this approach causes contention, consider creating a pool of more than one instance of an object and spreading the workload across these instances.

- Objects that you share across multiple requests **must** be thread-safe. The `HttpClient` class is designed to be used in this manner, but other classes might not be, so check the available documentation.

- In the .NET Framework, many objects that establish connections to external resources are created by using static factory methods of other classes that manage these connections. The objects created are intended to be saved and reused rather than being destroyed and recreated as required. For example, the the [Best Practices for Performance Improvements Using Service Bus Brokered Messaging][best-practices-service-bus] page contains the following comment:

----------

Service Bus client objects, such as `QueueClient` or `MessageSender`, are created through a `MessagingFactory` object, which also provides internal management of connections. You should not close messaging factories or queue, topic, and subscription clients after you send a message, and then re-create them when you send the next message. Closing a messaging factory deletes the connection to the Service Bus service, and a new connection is established when recreating the factory. Establishing a connection is an expensive operation that can be avoided by re-using the same factory and client objects for multiple operations.

----------

- Only use this approach where it is appropriate. You should always release scarce resources once you have finished with them, and acquire them only on an as-needed basis. A common example is a database connection. Retaining an open connection that is not required may prevent other concurrent users from gaining access to the database.

## Consequences of the solution

The system should should be more scalable, offer a higher throughput (the system is wasting less time acquiring and releasing resources and is therefore able to spend more time doing useful work), and report fewer errors as the workload increases. The following graph shows the load-test results for the sample application, using the same workload as before, but invoking the `GetProductAsync` method in the `SingleHttpClientInstance` controller:

![Throughput of the sample application reusing the same instance of an HttpClient object for each request][throughput-single-HTTPClient-instance]

No errors were reported, and the system was amply able to handle an increasing load  of up to over 500 requests per second; the volume of requests capable of being handled closely mirroring the user-load. The average response time was close to half that of the previous test. The result is a system that is much more scalable than before.

Finally, the graph below shows the results of the equivalent load-test for the `SingleServiceInstance` controller. *Note that as before, the scale for the response time and throughput for this graph are logarithmic.*:

![Throughput of the sample application reusing the same instance of an HttpClient object for each request][throughput-single-ExpensiveToCreateService-instance]

The volume of requests handled increases in line with the user-load while the average response time remains low. This is similar to the profile of the code that creates a single `HttpClient` instance.

## Related resources

- [Best Practices for Performance Improvements Using Service Bus Brokered Messaging][best-practices-service-bus].

- [Performance Tips for Azure DocumentDB - Part 1][performance-tips-documentdb]

- [Redis ConnectionMultiplexer - Basic Usage][redis-multiplexer-usage]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[best-practices-service-bus]: https://msdn.microsoft.com/library/hh528527.aspx
[performance-tips-documentdb]: http://blogs.msdn.com/b/documentdb/archive/2015/01/15/performance-tips-for-azure-documentdb-part-1.aspx
[redis-multiplexer-usage]: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Basics.md
[NewRelic]: http://newrelic.com/azure
[AppDynamics]: http://www.appdynamics.co.uk/cloud/windows-azure
[throughput-new-HTTPClient-instance]: Figures/HttpClientInstancePerRequest.jpg
[dashboard-new-HTTPClient-instance]: Figures/HttpClientInstancePerRequestWebTransactions.jpg
[thread-profiler-new-HTTPClient-instance]: Figures/HttpClientInstancePerRequestThreadProfile.jpg
[throughput-new-ExpensiveToCreateService-instance]: Figures/ServiceInstancePerRequest.jpg
[throughput-single-HTTPClient-instance]: Figures/SingleHttpClientInstance.jpg
[throughput-single-ExpensiveToCreateService-instance]: Figures/SingleServiceInstance.jpg
