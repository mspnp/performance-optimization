# Synchronous I/O

A synchronous I/O operation blocks the calling thread while the I/O completes. The calling thread is effectively suspended and unable to perform useful work during this interval. The result is that processing resources are wasted. In a cloud-based web application or service which serves multiple concurrent requests, this approach can adversely affect the scalability of the system.

Common examples of synchronous I/O include:

- Retrieving or persisting data to a database or any type of persistent storage.

- Sending a request to a web service and waiting for a response.

- Posting a message or retrieving a message from a queue.

- Writing to or reading from a local file.

This anti-pattern typically occurs because:

- It appears to be the most intuitive way to perform an operation. For example, the following code looks to be the obvious way to upload a file to an Azure blob storage:

**C#**

``` C#
CloudStorageAccount storageAccount = ...;
CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
CloudBlobContainer container = blobClient.GetContainerReference("uploadedfiles");
container.CreateIfNotExists();

CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

// Create or overwrite the "myblob" blob with contents from a local file.
using (var fileStream = System.IO.File.OpenRead(HttpContext.Current.Server.MapPath(@"../FileToUpload.txt")))
{
    blockBlob.UploadFromStream(fileStream);
}
```

- The application requires a response from the request, as shown by the following code example. The `GetUserProfile` method is part of a web service that retrieves user profile information from a User Profile service (through the `UserServiceProfileProxy` class which handles all of the connection and routing details) and then returns it to a client. The thread running the `GetUserProfile` method will be blocked waiting for the response from the User Profile service:

**C#**

``` C#
public class SyncController : ApiController
{
    private readonly IUserProfileService _userProfileService;

    public SyncController()
    {
        _userProfileService = new UserProfileServiceProxy();
    }

    /// <summary>
    /// This is a synchronous method that calls the synchronous GetUserProfile method.
    /// </summary>
    /// <returns>A UserProfile instance</returns>
    public UserProfile GetUserProfile()
    {
        var userProfile = _userProfileService.GetUserProfile();
        return userProfile;
    }
}
```

Note that the code for these two examples form part of the [Synchronous I/O sample application][fullDemonstrationOfProblem].

- The application uses a library which performs I/O and that does not provide asynchronous operations. For example:

**C#**

``` C#
var result = LibraryIOOperation();
// Wait while the method completes

Console.WriteLine("{0}", result);
```

- The system is tuned to expect a constant load where synchronous I/O may be more optimal. This can happen with IIS optimization and may be missed during load testing where the system may be exposed to an artificially distributed (constant) stream of requests directed at a limited set of I/O resources. Performing operations synchronously in this scenario can help to preserve CPU and memory resources as requests are queued rather than being competing for the same data on seperate threads. However, in the real world, requests are more likely to be distributed across a wider number of items and occur at varying rates, in which case performing synchronous I/O operations becomes an overhead rather than an advantage.

- Synchronous I/O operations are hidden in an external library used by the application. A single synchronous I/O call embedded deep within a series of cascading operations can block an entire call chain.

## How to detect the problem
From the viewpoint of a user running an application that performs synchronous I/O operations, the system can seem unresponsive or appear to hang periodically. The application may even fail with timeout exceptions. A web server hosting a service that performs internal I/O operations synchronously may become overloaded. Operations within the web server may fail as a result, causing HTTP 500 (Internal Server Error) response messages. Additionally, in the case of a web server such as IIS, incoming client requests might be blocked until a thread becomes available. This can result in excessive request queue lengths, resulting in HTTP 503 (Service Unavailable) response messages being passed back to the caller.

### Reviewing the application

If you are a designer or developer familiar with the way in which application is implemented, you could perform a code review to identify the source of any I/O operations that the application performs. I/O operations that appear to be synchronous should be evaluated to determine whether they are likely to cause a blockage as the number of requests increases.

----------
**Note:** I/O operations that are expected to be very short lived and which are unlikely to cause contention, typically accessing fast, local resources (such as small private files on an SSD drive), might be best left as synchronous operations. In these cases the overhead of creating and managing a separate task, dispatching it to another thread, and synchronizing with that thread when the task completes might outweigh the benefits of performing an operation asynchronously.

----------

### Load-testing the application

Load-testing can help to identify whether a system is being constrained by performing synchronous I/O operations. As user load increases, performing synchronous I/O operations can result in exponential increases in latency for requests and consequent low throughput. This behavior typically occurs when the user load reaches thousands of concurrent requests. A good detection strategy to determine whether synchronous I/O may be a problem is to load-test the system against a work load that increases from a small number to thousands of concurrent user requests and examine how the response rate and latency varies. As an example, the following table illustrates the performance of the synchronous `GetUserProfile` method in the `SyncController` controller in the sample application under varying loads.

#Users | Requests/sec | Latency secs
------| ---------| ------------
20 | 8 | 2.5
1000 | 115 | 8.87
2000 | 138 | 14.64
3000 | 178 | 16.97
4000 | 200 | 20.10
5000 | 223 | 22.51
6000 | 226 | 26.62
7000 | 242 | 29.01

As the workload increases, the rate at which requests are handled initially jumps due to the spare capacity in the system being utilized. However, the change in this rate is not linear and will eventually reach a plateau. The latency also increases significantly but it too plateaus. One reason for this is that requests are arriving faster than the system can handle them. Web servers such as IIS queue incoming requests as they arrive and only process these requests when a thread becomes available (IIS manages its own thread pool). If a request performs synchronous I/O operations, the thread is blocked while these operations complete. As the queue length grows, requests start to time out. This is characterized by the number of errors that are returned; the rate at which these errors occur increases with the load after the web server is running at full capacity. At this point, the average latency is roughly inline with the timeout period, which is a constant. 

----------

**Note:** Throughput should not be determined by the volume of requests per second and the latency alone, but must also consider the rate at which requests fail.

----------

Generally, in a system that performs synchronous I/O operations, response times will fluctuate as the user load varies. Additionally, as the user load diminishes, it may take some time for the system to recover and manage to process the majority of requests successfully. The following graph shows the performance profile of a typical web application that uses synchronous I/O. The test starts with 6000 concurrent users. Initially requests are successful; the first requests are handled very quickly as the system has plenty of spare capacity, but as more requests arrive they take longer to process until the volume of requests that are queued exceeds the capacity of the system to process them. At this point, errors start occurring and this is indicated by the spikes in the average test time. As the user load drops, the system starts to recover and the average test times become more stable as requests are handled successfully (the system is able to drain the request queue faster than new requests are added). Notice also that there is a lag in the change in response time as the user load steps down; this is due to the backlog effects of the queue. As the user load increases, so does the response time (with a similar lag), until the point at which the system becomes overloaded again (6000 users) when the spikes in the response time are indicative of many requests resulting in an error. Finally, as the user load decreases, the system recovers.

![Performance chart for a web application performing synchronous I/O operations][sync-performance]

You should consider the following points when load-testing a system to detect effects due to synchronous I/O:

- Examine how the throughput and response time vary as the user load increases.

- Perform testing with `step-up` user loads to simulate user loads with spikes that are expected to exceed the capacity of the system, and examine how the system recovers when the user load drops.

- Many business scenarios might result in an application performing a mixture of synchronous I/O operations together with other forms of processing. Such asymmetric workloads can cause capacity variations in the system. When you are load-testing the system, use a testing profile that simulates the asymmetric workload of the real business environment.

### Monitoring web server performance

For Azure web applications and web roles, it is also worth monitoring the performance of the IIS web server. In particular, you should pay attention to the ASP.NET request queue length to ascertain whether requests are being blocked waiting for available threads to process them during periods of high activity. You can gather this information by enabling Azure diagnostics. You can retrieve the performance counter data from the `WADPerformanceCountersTable` table generated by Azure diagnostics.The key performance counters to examine are `\ASP.NET\Requests Queued` and `\ASP.NET\Requests Rejected`. The article [Analyzing logs collected with Azure Diagnostics][analyzing-logs] provides more information on collecting and analyzing diagnostics.

## How to correct the problem
Replace synchronous I/O operations with asynchronous requests. Performing an I/O operation asynchronously can free the current thread to continue performing meaningful work rather than being blocked waiting for a slow device or network to respond. This helps to improve the utilization of expensive compute resources.

Some libraries provide asynchronous versions of the available I/O operations. For example, the following code uploads data to Azure blob storage asynchronously:

**C#**

``` C#
CloudStorageAccount storageAccount = ...;
CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
CloudBlobContainer container = blobClient.GetContainerReference("uploadedfiles");
await container.CreateIfNotExistsAsync();

CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

// Create or overwrite the "myblob" blob with contents from a local file.
using (var fileStream = System.IO.File.OpenRead(HttpContext.Current.Server.MapPath(@"../FileToUpload.txt")))
{
    await blockBlob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
}
```

The `UploadFromStreamAsync` method creates a new `Task` on which to perform the file upload operation. This task can run asynchronously on a separate thread from the code that called it. Note that this code uses the `await` operator to return control to the calling environment while the asynchronous operation is performed. The subsequent code effectively acts as a continuation that runs when the asynchronous operation has completed.

A well-designed service should also provide asynchronous operations. The example below illustrates an asynchronous version of the web service that returns user profile information. The `GetUserProfileAsync` method depends on an asynchronous version of the User Profile service proxy which doesn't block the calling thread:

**C#**

``` C#
public class AsyncController : ApiController
{
    private readonly IUserProfileService _userProfileService;

    public AsyncController()
    {
        _userProfileService = new UserProfileServiceProxy();
    }

    /// <summary>
    /// This is an synchronous method that calls the Task based GetUserProfileAsync method.
    /// </summary>
    /// <returns>A UserProfile instance</returns>
    public Task<UserProfile> GetUserProfileAsync()
    {
        var userProfile = _userProfileService.GetUserProfileAsync();
        return userProfile;
    }
}
```

For libraries that do not provide asynchronous versions of operations, it may be possible to create asynchronous wrappers around selected synchronous methods. However, you should follow this approach with caution. While this strategy may improve responsiveness on the thread invoking the asynchronous wrapper (which is useful if the thread is handling the user interface), it actually consumes more resources; an additional thread may be created, and there is additional overhead associated with synchronizing the work performed by this thread. Consequently, **this approach might not be scalable and should not be used for server-side code**. For more information, see the article [Should I expose asynchronous wrappers for synchronous methods?][async-wrappers]:

**C#**

``` C#
// Asynchronous wrapper around synchronous library method
private async Task<int> LibraryIOOperationAsync()
{
    return await Task.Run(() => LibraryIOOperation());
}

...
// Invoke the asynchronous wrapper using a task
var libraryTask = LibraryIOOperationAsync();

// Use a continuation to handle the result of the LibraryIOOperation method
libraryTask.ContinueWith((task) =>
{
    Console.WriteLine("Result from LibraryIOOperation is {0}", task.Result);
});

// Processing continues while the LibraryIOOperation method is run asynchronously
Console.WriteLine("Work performed while LibraryIOOperation is running asynchronously");
```


[Link to the related sample][fullDemonstrationOfSolution]

## Consequences of the solution

The system should be able to support more concurrent user requests than before, and as a result be more scalable. This can be determined by performing load testing before and after making any changes to the code and then comparing the results. Repeating the load-testing experiment described earlier but using asynchronous `GetUserProfileAsync` method in the `AsyncController` controller in the sample application yields the following results: 

#Users | Requests/sec | Latency secs
-------| ---------| ------------
20 | 8 | 2.5
1000 | 489 | 2.09
2000 | 950 | 2.13
3000 | 1389 | 2.17
4000 | 1732 | 2.32
5000 | 1871 | 2.68
6000 | 1887 | 3.19
7000 | 1890 | 3.71

This time, the latency remains low and does not fluctuate with increased load to the same extent that the synchronous scenario does. Requests are queued as before and dispatched to a thread in the IIS thread pool when one becomes available. However, the IIS thread is not blocked by the operation being performed and once the asynchronous work has started the thread can be released to process the next queued request. For more information, visit [Using Asynchronous Methods in ASP.NET 4.5][AsyncASPNETMethods].

For comparison purposes with the earlier example, the following graph shows the performance profile of a typical web application that uses asynchronous I/O. Notice that response time is less dependent on the user load and remains reasonably level until the point at which the system becomes overloaded. Recovery time when the user load drops is much quicker. 

![Performance chart for a web application performing asynchronous I/O operations][async-performance]

Functionally, the system should remain unchanged. Monitoring the request queue lengths should reveal that requests are retrieved more quickly rather than timing out as they spend less time blocked waiting for an available thread.

You should note that improving I/O performance may cause other parts of the system to become bottlenecks. Alleviating the effects of synchronous I/O operations that were previously constraining performance may cause overloading of other parts of the system. For example, unblocking threads could result in an increased volume of concurrent requests to network resources resulting in network connection starvation, or an increased number of concurrent data accesses could result in a data store throttling requests. **Therefore it may be necessary to scale or redsign the I/O resources; increase the number of web servers or partition data stores to help reduce contention.**


## Related resources

- [Should I expose asynchronous wrappers for synchronous methods?][async-wrappers]

- [Using Asynchronous Methods in ASP.NET 4.5][AsyncASPNETMethods]

- [Analyzing logs collected with Azure Diagnostics][analyzing-logs]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[async-wrappers]: http://blogs.msdn.com/b/pfxteam/archive/2012/03/24/10287244.aspx
[AsyncASPNETMethods]: http://www.asp.net/web-forms/overview/performance-and-caching/using-asynchronous-methods-in-aspnet-45
[analyzing-logs]: https://msdn.microsoft.com/library/azure/dn904284.aspx
[sync-performance]: Figures/SyncPerformance.jpg
[async-performance]: Figures/AsyncPerformance.jpg
