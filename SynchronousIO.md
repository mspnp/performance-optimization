# Synchronous I/O

A synchronous I/O operation blocks the calling thread while the I/O completes. The calling thread is effectively suspended and unable to perform useful work during this interval. The result is that processing resources are wasted. In a cloud-based web application or service which serves multiple concurrent requests, this approach can adversely affect the scalability of the system.

Common examples of synchronous I/O include:

- Retrieving or persisting data to a database and waiting for the outcome of a transaction.

- Uploading or downloading large volumes of data to and from remote storage and waiting for verification that the operation was successful.

- Sending a request to a web service and waiting for a response.

- Writing to a local file and waiting for the data to be saved.

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


## How to detect the problem
From the viewpoint of a user running an application that performs synchronous I/O operations, the system can seem unresponsive or appear to hang periodically. The application may even fail with timeout exceptions.

An operator monitoring a system that performs synchronous I/O operations may observe the following phenomena:

- The `Processor\% Processor Time` performance counter for each processor is low for extended periods of time *(NOTE: NEED TO SPECIFY A VALUE e.g. below 30%)*.
- The `System\Processor Queue Length` performance counter indicates that many processes are blocked awaiting CPU resources *(NOTE: NEED TO QUANTIFY - WHAT SHOULD THE IDEAL QUEUE LENGTH BE?)*
- The `Memory\Available Bytes` performance counter regularly indicates that the amount of free memory is less than 10% of the available memory.
- If the application is performing disk I/O, the `PhysicalDisk\Avg. Disk Queue Length` for disks accessed by the application indicate a persistent backlog of I/O requests. A sustained queue length above 5 could indicate a disk subsystem bottleneck (**Note:** The application should ensure that disks used for paging process memory are kept distinct from those used to store application data).
- The `HttpWebRequest` performance counters on a machine hosting a web service ... *(NOTE: MORE DETAILS TO BE ADDED)*

Profiling the application can identify long-running method-calls and the amount of CPU time spent while running these methods. If a long-running method accounts for a minimal amount of CPU time, then examine this method to determine whether it is blocking by performing synchronous I/O operations *(NOTE: NEED TO QUANTIFY THIS AND POSSIBLY ADD MORE DETAILS)*.

## How to correct the problem
Replace synchronous I/O operations with asynchronous requests.

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
**This needs verification**.

The system should be able to support more concurrent user requests than before, and as a result be more scalable. This can be determined by performing load testing before and after making any changes to the code and then comparing the results. Functionally, the system should remain unchanged. Monitoring the system and analyzing the key performance counters described earlier should indicate that the system spends less time blocked by synchronous I/O and the CPUs are more active.

## Related resources

- [Should I expose asynchronous wrappers for synchronous methods?][async-wrappers]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[async-wrappers]:http://blogs.msdn.com/b/pfxteam/archive/2012/03/24/10287244.aspx