# Too Many Instances

Many .NET Framework libraries provide abstractions around external resources. Internally, these classes typically manage their own connections to these external resources. Examples of such classes frequently used by Azure applications include  `System.Net.Http.HttpClient` to communicate with a web service by using the HTTP protocol, `Microsoft.ServiceBus.Messaging.QueueClient` for posting and receiving messages to a Service Bus queue,  `Microsoft.Azure.Documents.Client.DocumentClient` for connecting to an Azure DocumentDB instance, and `StackExchange.Redis.ConnectionMultiplxer` for accessing Azure Redis Cache.

You can adopt the same approach in your own code. In the following snippet, the disposable `ProductRepository` class encapsulates an HTTP connection to a remote data store. An application can call the `GetProductByIdAsync` method to retrieve the details of a specified product:

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
Creating an instance of the `ProductRepository` class involves establishing a new HTTP connection. The resources used by the connection are recycled when the work has completed and the flow of control reaches the end of the `using` block. 

This approach is acceptable for client-side applications where the number of connections being made is likely to be limited, but in a server-side or web application this technique is not scalable. For example, if the code is embedded in a web API controller as shown below, this code could be run concurrently by many users:

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

This problem is not restricted to the `HttpClient` class. Creating many instances of other classes that wrap other resources (such as `System.Net.Http.HttpClient`, `Microsoft.ServiceBus.Messaging.QueueClient`,  `Microsoft.Azure.Documents.Client.DocumentClient`, or `StackExchange.Redis.ConnectionMultiplxer` mentioned earlier) can cause similar issues.

## How to detect the problem

Symptoms of the *Too Many Instances* problem include a drop in throughput with an increase in exceptions indicating exhaustion of related resources (sockets, database connections, file handles, and so on). End-users are likely to report frequent request failures.

### Load-testing the application

You can use load-testing based on workloads that emulate the typical sequence of operations that users might perform to help identify which parts of a system suffer with resource-exhaustion under varying loads. The following graph shows the throughput of requests directed at the `NewInstancePerRequest` controller in the sample application as the user load is increased from 100 to 1,000 users. Note that as the user load passes 700 concurrent users the number of failed requests suddenly increases. These failures are reported by the load test as HTTP 500 (Internal Server) errors.

![Throughput of the sample application creating a new instance of an HttpClient object for each requests][throughput-new-instance]

### Instrumenting the application

A key task is to identify the actual causes of these exceptions and where they are being raised. You can achieve this by instrumenting the application and logging the exceptions as they occur in the `ProductRepository` class. In this example, the vast majority of these failures are caused by `SocketException` errors raised in the `GetProductByIdAsync` method.

In situations where you are less sure of the source of exceptions, then you will need to capture stack traces as exceptions are raised and use this information to track back to the point at which the exceptions originated.

### Monitoring the application

**THIS IS A BIT VAGUE - SHOULD WE ADD MORE DETAIL, AND IF SO, WHAT?**
Monitoring the application will help to determine the how frequently specific resources are accessed and for how long. A performance monitoring tool such as [New Relic][NewRelic] or [AppDynamics][AppDynamics] provide features that can be used to track Azure web applications and cloud services.

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

No errors were reported, and the request rate remained relatively static at nearly 600 requests per second on average (this compares with 135 requests per second for the earlier example).

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
