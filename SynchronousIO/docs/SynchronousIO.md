# Performing Synchronous I/O

A synchronous I/O operation blocks the calling thread while the I/O completes. The calling thread is effectively suspended and unable to perform useful work during this interval. The result is that processing resources are wasted. In a cloud-based web application or service which serves multiple concurrent requests, this approach can adversely affect the vertical scalability of the system.

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

- The application requires a response from the request, as shown by the following code example. The `GetUserProfile` method is part of a web service that retrieves user profile information from a User Profile service (through the `FakeUserProfileService` class which handles all of the connection and routing details) and then returns it to a client. The thread running the `GetUserProfile` method will be blocked waiting for the response from the User Profile service:

**C#**

``` C#
public class SyncController : ApiController
{
    private readonly IUserProfileService _userProfileService;

    public SyncController()
    {
        _userProfileService = new FakeUserProfileService();
    }

    // This is a synchronous method that calls the synchronous GetUserProfile method.
    public UserProfile GetUserProfile()
    {
        return _userProfileService.GetUserProfile();
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

- Synchronous I/O operations are hidden in an external library used by the application. A single synchronous I/O call embedded deep within a series of cascading operations can block an entire call chain.

## How to detect the problem
From the viewpoint of a user running an application that performs synchronous I/O operations, the system can seem unresponsive or appear to hang periodically. The application may even fail with timeout exceptions. A web server hosting a service that performs internal I/O operations synchronously may become overloaded. Operations within the web server may fail as a result, causing HTTP 500 (Internal Server Error) response messages. Additionally, in the case of a web server such as IIS, incoming client requests might be blocked until a thread becomes available. This can result in excessive request queue lengths, resulting in HTTP 503 (Service Unavailable) response messages being passed back to the caller.

### Reviewing the application

Perform a code review to identify the source of any I/O operations that the application performs. I/O operations that appear to be synchronous should be evaluated to determine whether they are likely to cause a blockage as the number of requests increases.

----------

**Note:** I/O operations that are expected to be very short lived and which are unlikely to cause contention, typically accessing fast, local resources (such as small private files on an SSD drive), might be best left as synchronous operations. In these cases the overhead of creating and managing a separate task, dispatching it to another thread, and synchronizing with that thread when the task completes might outweigh the benefits of performing an operation asynchronously.

----------

### Load-testing the application

Load-testing can help to identify whether a system is being constrained by performing synchronous I/O operations. As an example, the following graph illustrates the performance of the synchronous `GetUserProfile` method in the `SyncController` controller in the sample application under varying loads.

----------

**Note:** The sample application was deployed to a 2-instance cloud service comprising medium virtual machines. Also note that the synchronous operation is timed to take 2 seconds, so the minimum response time will be slightly over 2 seconds.

----------

![Performance chart for the sample application performing synchronous I/O operations][sync-performance]

As the load increases, so does the average response time and the throughput measured in requests per second. The total number of requests completed during the test interval (10 minutes) was 51254.

In isolation, it is not necessarily apparent from this test that performing synchronous I/O is a problem (unless the response time is outside of agreed SLAs). However, if processing resources are more constrained (either as a result of a much larger volume of users, or using smaller scale hosts) then you may see results similar to the following graph:

![Performance chart for the sample application running on a constrained host performing synchronous I/O operations][limited-sync-performance]

In this load-test, the response time and throughput initially follow a similar pattern to that of the previous test (they both increase gradually, although the system is slower due to the resource limitations of the smaller virtual machines), until the system reaches a tipping point when the workload reaches 2550 users. At this time a large number of errors start to occur. The following table breaks down the information presented by the graph:

Users | Requests/sec | Avg Response Time secs | Failed Requests/sec | Successful Requests/sec
---|---|---|---|---
50 | 23.4 | 2.08 | 0 | 23.4
550 | 53 | 9.74 | 0.13 | 52.87
1050 | 76.5 | 12.5 | 0.08 | 76.42
1550 | 93.7 | 15.5 | 0.58 | 93.12
2050 | 104 | 18.1 | 1.90 | 102.91
2550 | 335 | 7.42 | 227 | 108
3050 | 132 | 20.3 | 22 | 110
3550 | 1599 | 1.35 | 1491 | 108
4050 | 172 | 18.7 | 46.1 | 125.9
4550 | 175 | 17.5 | 35.8 | 139.2

The rate of successful requests is not shown on the graph but is calculated by subtracting the rate of failed requests from the rate of all requests. Apart from the final couple of figures (which are possibly outliers), the system appears to support a maximum of approximately 110 successful requests per second. To understand why this is so, consider that the application runs as a Web role. Incoming requests are queued by the IIS web server and handed to a thread running in the ASP.NET thread pool. Because each operation performs I/O synchronously, the thread is blocked until the operation completes. As the workload increases, the rate at which requests are received also increases up to the point at which all of the ASP.NET threads in the thread pool are allocated and being blocked. At this time, any further incoming requests cannot be not fulfilled immediately and wait in the queue until a running operation completes and a thread becomes available. As the queue length grows, requests start to time out. This is characterized by the number of errors that are returned. Although the volume of requests appears to increase at this point, the proportion of these requests that result in an error also increases. In total, the load-test reported a failure rate of 66.3%.

----------

**Note:** This load-test illustrates an important point: **Throughput should not be determined by the volume of requests per second and the response time alone, but must also consider the rate at which requests fail.**

----------

For comparison purposes with the earlier examples, the following graph illustrates the performance of the asynchronous `GetUserProfileAsync` method in the `AsyncController` controller in the sample application under the same varying loads and conditions as the first test (a 2-instance set of medium sized virtual machines):

![Performance chart for the sample application performing asynchronous I/O operations][async-performance]

This time the throughput is far higher. This is because the ASP.NET threads handling the incoming requests are not blocked by I/O requests. Instead, they become available to process further requests. In the same period of time as the previous test, the system successfully handled 567067 requests; a nearly 10-fold increase in throughput.

Repeating the tests in the more constrained environment using small virtual machines generates the following results:

Users | Requests/sec | Avg Response Time secs | Failed Requests/sec | Successful Requests/sec
---|---|---|---|---
50 | 23.4 | 2.10 | 0 | 23.4
550 | 195 | 2.75 | 0.83 | 194.17
1050 | 284 | 3.4 | 1.22 | 282.78
1550 | 328 | 4.15 | 0.77 | 327.23
2050 | 332 | 5.38 | 3.93 | 328.07
2550 | 353 | 6.6 | 11.1 | 341.9
3050 | 358 | 7.88 | 20.8 | 337.2
3550 | 356 | 9.35 | 33.5 | 322.5
4050 | 324 | 14.3 | 46.2 | 277.8
4550 | 326 | 12.8 | 67.4 | 258.6

![Performance chart for the sample application performing asynchronous I/O operations][limited-async-performance]

The system still reaches a tipping point, but at a far higher workload than before (at approximately 4050 users). Additionally, the maximum number of consistently successful requests is also higher at approximately 330 requests per second (the final two figures are probably outliers, as in the earlier example). The error rate reported was 6.33%. This test shows that even in a constrained environment, replacing synchronous I/O operations with asynchronous requests can lead to big performance gains.

----------

**Note:** the maximum queue length supported by IIS is 5000 outstanding requests, regardless of the size of the host virtual machine. Once this number of requests is exceeded they may start failing. Scaling the size of virtual machines vertically will not help. In this situation, you should scale horizontally and spread the load over more instances.

----------

To summarize, you can use the following strategy when load-testing a system to detect effects due to synchronous I/O:

1. Perform a load-test with a large workload (many thousands of users). The ASP.NET thread pool only contains approximately 100 threads initially to satisfy requests, so requests that perform synchronous I/O operations will show how a high latency.

2. Perform a pair of load-tests; one with 100 users and the second with 5000 users. Compare the results for response time, throughput, and error rates.

3. Perform testing with *step-up* user loads to simulate workloads with spikes that are expected to exceed the capacity of the system. Examine how the system responds and determine whether it matches the performance profile of a system that performs synchronous or asynchronous I/O operations.

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
await container.CreateIfNotExistsAsync().ConfigureAwait(false);

CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

// Create or overwrite the "myblob" blob with contents from a local file.
using (var fileStream = File.OpenRead(HostingEnvironment.MapPath("~/FileToUpload.txt")))
{
    await blockBlob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
}
```

The `UploadFromStreamAsync` method creates a new `Task` on which to perform the file upload operation. This I/O operation is started and the thread is released, becoming available to perform other work. When the I/O operation completes, the next available thread is used to continue the processing for the remainder of the method (the call to `ConfigureAwait(false)` indicates that this does not have to be the same thread that initiated the task.)


----------

**Note:** This code uses the `await` operator to return control to the calling environment while the asynchronous operation is performed. The subsequent code effectively acts as a continuation that runs when the asynchronous operation has completed.

----------

A well-designed service should also provide asynchronous operations. The example below illustrates an asynchronous version of the web service that returns user profile information. The `GetUserProfileAsync` method depends on an asynchronous version of the User Profile service which doesn't block the calling thread:

**C#**

``` C#
public class AsyncController : ApiController
{
    private readonly IUserProfileService _userProfileService;

    public AsyncController()
    {
        _userProfileService = new FakeUserProfileService();
    }

    // This is an synchronous method that calls the Task based GetUserProfileAsync method.
    public Task<UserProfile> GetUserProfileAsync()
    {
        return _userProfileService.GetUserProfileAsync();
    }
}
```

For libraries that do not provide asynchronous versions of operations, it may be possible to create asynchronous wrappers around selected synchronous methods. **However, you should follow this approach with caution.** While this strategy may improve responsiveness on the thread invoking the asynchronous wrapper (which is useful if the thread is handling the user interface), it actually consumes more resources; an additional thread may be created, and there is additional overhead associated with synchronizing the work performed by this thread. For more information, see the article [Should I expose asynchronous wrappers for synchronous methods?][async-wrappers]:

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

The system should be able to support more concurrent user requests than before, and as a result be more scalable. Functionally, the system should remain unchanged. Performing load-tests and monitoring the request queue lengths should reveal that requests are retrieved more quickly rather than timing out as they spend less time blocked waiting for an available thread.

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
[limited-sync-performance]: Figures/LimitedSyncPerformance.jpg
[async-performance]: Figures/AsyncPerformance.jpg
[limited-async-performance]: Figures/LimitedAsyncPerformance.jpg
