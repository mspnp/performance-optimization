# Too Many Instances

Many .NET Framework libraries provide abstractions around external resources. Internally, these classes typically manage their own connections to these external resources, effectively acting as brokers that clients can use to request access to a resource. Examples of such classes frequently used by Azure applications include  `System.Net.Http.HttpClient` to communicate with a web service by using the HTTP protocol, `Microsoft.ServiceBus.Messaging.QueueClient` for posting and receiving messages to a Service Bus queue,  `Microsoft.Azure.Documents.Client.DocumentClient` for connecting to an Azure DocumentDB instance, and `StackExchange.Redis.ConnectionMultiplexer` for accessing Azure Redis Cache.

These *broker* classes can be expensive to create. Instead, they are intended to be instantiated once and reused throughout the life of an application, as shown by the following code snippet that demonstrates the use of the `HttpClient` class in a web API controller:

**C#**
``` C#
public class HttpClientDemoController : ApiController
{
    private HttpClient client = null;

    public HttpClientDemoController()
    {
        client = new HttpClient();
        client.BaseAddress = ...;
    }

    ...
    public string Get(int id)
    {
        ...
        var data = client.GetAsync(...);
        ...
        return ...;
    }

    public void Post([FromBody]string value)
    {
        ...
        client.PostAsync(...);
        ...
    }
    ...
}
```

It is often tempting to make each method in a class self-contained by creating the resources needed when the method starts and then releasing them when the method finishes, as shown below. However, creating broker objects when needed and then destroying them when an operation completes can damage the performance of a system, especially if this cycle is repeated on a large scale by thousands of concurrent users:

**C#**
``` C#
public class HttpClientDemoController : ApiController
{
    public string Get(int id)
    {
        using (HttpClient client = new HttpClient())
        {
            ...
            var data = client.GetAsync(...);
            ...
        } 
        return ...;
    }

    public void Post([FromBody]string value)
    {
        using (HttpClient client = new HttpClient())
        {
            ...
            client.PostAsync(...);
            ...
        }
    }
    ...
}
```

Although you might not follow this approach intentionally, it is very easy to implement it inadvertently. In the following snippet, the disposable `ProductRepository` class encapsulates an HTTP connection to a remote data store. An application can call the `GetProductByIdAsync` method to retrieve the details of a specified product:

**C#**
``` C#
public class ProductRepository : IProductRepository, IDisposable
{
    private bool disposed = false;
    private HttpClient httpClient;

    public ProductRepository()
    {
        // Create the HTTP connection object
        httpClient = new HttpClient();
    }

    public async Task<Product> GetProductByIdAsync(string productId)
    {
        // Retrieve the details for the specified product
        var result = await httpClient.GetStringAsync(...).ConfigureAwait(false);
        
        // Parse the results into a new Product object
        Product product = ...;

        return product;
    }

    // Close the HTTP connection when the object is disposed
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            httpClient.Dispose();
        }

        disposed = true;
    }
}
```

An application can use the following code to send a request that fetches the details of a product:

**C#**

``` C#
using (var productRepository = new ProductRepository())
{
    return await productRepository.GetProductByIdAsync(id).ConfigureAwait(false);
}
```
----------

**Note:** This code forms part of the [TooManyInstances sample application][fullDemonstrationOfProblem].

----------

Creating an instance of the `ProductRepository` class involves establishing a new HTTP connection by creating a new `HttpClient` object. The resources used by the connection are recycled when the work has completed and the flow of control reaches the end of the `using` block and the `HttpClient` object is disposed. 

This approach might acceptable for client-side applications where the number of HTTP connections being made is likely to be limited, but in a server-side or web application this technique is not scalable. For example, if the code is embedded in a web API controller as shown below, this code could be run concurrently by many users:

**C#**
``` C#
public class NewInstancePerRequestController : ApiController
{
    /// <summary>
    /// This method creates a new instance of ProductRepository and disposes it for every call to GetProductAsync.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Product> GetProductAsync(string id)
    {
        using (var productRepository = new ProductRepository())
        {
            return await productRepository.GetProductByIdAsync(id).ConfigureAwait(false);
        }
    }
}
```

Each user request results in the creation of a new instance of the `ProductRepository` class, which in turn creates a new `HttpClient` object. Under a heavy load, the web server can exhaust the number of sockets available, resulting in `SocketException` errors. 

This problem is not restricted to the `HttpClient` class. Creating many instances of other classes that wrap other resources might cause similar issues, or at least slow down the performance of the application as broker objects are continually created and destroyed.

## How to detect the problem

Symptoms of the *Too Many Instances* problem include a drop in throughput, possibly with an increase in exceptions indicating exhaustion of related resources (sockets, database connections, file handles, and so on). End-users are likely to report degraded performance and frequent request failures when the system is heavily utilized.

You can perform the following steps to help identify this problem:

1. Identify points at which response times slow down by performing process monitoring of the production system.

2. Examine the telemetry data captured at these points to determine which operations might be creating and destroying large resource-consuming objects at the point of slow-down.

3. Perform load testing of each of the operations identified by step 3. Use a controlled test environment rather than the production system.

4. Review the source code for the possible operations to identify the points at which broker objects are created and destroyed.

The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you to examine applications and services systematically.

----------

### Identifying points of slow-down

Instrumenting each operation in the production system to track the duration of each request and then monitoring the live system can help to provide an overall view of how the requests perform. You should also instrument the system to retrieve telemetry data about memory use when operations start and complete, and tracing how frequently the CLR garbage collector is running to reclaim large objects as well as observing the typical lifecycle of broker objects. 

**NEED TO SHOW THE SAMPLE APPLICATION PERFORMANCE PROFILE (WHICH OPERATIONS ARE RUNNING, HOW LONG THEY TAKE, ETC) USING NEW RELIC/APPDYNAMICS**

### Examining telemetry data and finding correlations

During periods of stress, operations that continually create and destroy large objects can be observed by monitoring the system and noting which operations cause increased activity in memory use and garbage collection. Depending on the nature of the brokers being used, there may also be increased network, disk, or database connectivity as connections are made, files are opened, or database connections established.

**NEED TO SHOW THE SAMPLE APPLICATION TELEMETRY IN NEW RELIC/APPDYNAMICS AND CORRELATE WITH THE PERFORMANCE PROFILE FROM STEP 1**

### Performing load-testing

You can use load-testing based on workloads that emulate the typical sequence of operations that users might perform to help identify which parts of a system suffer with resource-exhaustion under varying loads. You should perform these tests in a controlled environment rather than the production system. The following graph shows the throughput of requests directed at the `NewInstancePerRequest` controller in the sample application as the user load is increased up to 450  concurrent users. Note that as the user load passes 200 concurrent users the number of failed requests suddenly increases. These failures are reported by the load test as HTTP 500 (Internal Server) errors.

![Throughput of the sample application creating a new instance of an HttpClient object for each requests][throughput-new-instance]

In this graph, the scale of the throughput and response times is logarithmic to enable these measures to be shown effectively on the graph. After the initial loading the system stabilizes, supporting approximately 300 requests/second (oscillating between successful and failed requests) with an average response time of between 0.5 and 1 second per request.

### Reviewing the code

If you have managed to identify which parts of an application are causing exceptions due to resource exhaustion, perform a review of the code or use profiling to find out how objects that wrap external resources are being instantiated, used, and destroyed. Where appropriate, refactor code to cache and reuse objects, as described in the following section.

## How to correct the problem

If the class wrapping the external resource is thread-safe, create a pool of reusable instances of the class. The following code snippet shows a simple example. The `SingleInstance` controller performs the same operation as the `NewInstancePerRequest` controller shown earlier, except that the `ProductRepository` object is created once, in the constructor, rather than each time the `GetProductAsync` operation is invoked. This approach reuses the same `HttpClient` object inside the `ProductRepository`, sharing the connection across all requests.

**C#**
```C#
public class SingleInstanceController : ApiController
{
    private static readonly IProductRepository ProductRepository;

    static SingleInstanceController()
    {
        ProductRepository = new ProductRepository();
    }

    /// <summary>
    /// This method uses the shared instance of IProductRepository for every call to GetProductAsync.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Product> GetProductAsync(string id)
    {
        return ProductRepository.GetProductByIdAsync(id);
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

[Link to the related sample][fullDemonstrationOfSolution]

## Consequences of the solution

The system should should be more scalable, offer a higher throughput (the system is wasting less time acquiring and releasing resources and is therefore able to spend more time doing useful work), and report fewer errors as the workload increases. The following graph shows the load-test results for the sample application, using the same workload as before, but invoking the `GetProductAsync` method in the `SingleInstance` controller:

![Key indicators load-test results for the Chunky API in the Chatty I/O sample application][throughput-single-instance]

No errors were reported, and the system was amply able to handle an increasing load (up to nearly 3000 requests per second) with relatively low response time (between 0.07 and 0.15 seconds on average per request).

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
[throughput-new-instance]: Figures/InstancePerRequest.jpg
[throughput-single-instance]: Figures/SingleInstance.jpg
