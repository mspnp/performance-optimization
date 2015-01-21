# Synchronous I/O

A synchronous I/O operation blocks the calling thread while the I/O completes. The calling thread is effectively suspended and unable to perform useful work during this interval. The result is that processing resources are wasted. In a cloud-based web application or service which serves multiple concurrent requests, this approach can adversely affect the scalability of the system.

Common examples of synchronous I/O include:

- Writing data to a local file and waiting for the data to be saved.
- Posting a message to a message queue and waiting for the message queue to acknowledge receipt of the message.
- Sending a request to a web service and waiting for a response.

This anti-pattern typically occurs because:

- It appears to be the most natural means to perform an operation. For example, the following code looks to be the obvious way to post a message to an Azure Service Bus queue:

**C#**

    // Create a QueueClient object to connect to the queue
    QueueClient client = ...;
    // Create a message to post on the queue
    BrokeredMessage message = ...;
    // Post the message
    client.Send(message);


- The application requires a response from the request, as shown by the following code example which sends an HTTP GET request to a web service and then displays the result:
  
**C#**

    // Construct an HTTP web request
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("..."));
    request.Method = "GET";
    request.ContentType = "application/json";
    
    // Send the request and wait for the response
    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
    
    // Process the response
    if (response.StatusCode == HttpStatusCode.OK)
    {
        using (var responseStream = response.GetResponseStream())
        {
            if (responseStream != null)
            {
                using (var responseReader = new StreamReader(responseStream))
                {
                    string result = responseReader.ReadToEnd();
                    Console.WriteLine("{0}", result);
                }
            }
        }
    }


- The application uses a library which performs I/O and that does not provide asynchronous operations. For example:

**C#**

    var result = LibraryIOOperation();
    // Wait while the method completes

    Console.WriteLine("{0}", result);


[Link to the related sample][fullDemonstrationOfProblem]

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

Some libraries provide asynchronous versions of the available I/O operations. For example, the following code posts a message to a Service Bus queue asynchronously.

**C#**

    // Create a QueueClient object to connect to the queue
    QueueClient client = ...;
    // Create a message to post on the queue
    BrokeredMessage message = ...;
    // Post the message asynchrously
    client.SendAsync(message);

    // Processing continues while the Send operation is performed asynchronously
    Console.WriteLine("Processing while message is sent");
    ...


The `SendAsync` method creates a new `Task` on which to perform the Send operation. This task can run asynchronously on a seperate thread from the code that called it. The use of the `SendAsync` method shown in this example is an illustration of the fire-and-forget technique;  the application invokes the `SendAsync` method but does not know whether the task has succeeded. To capture this information, use a continuation that runs when the task completes and has access to state information about the task:

**C#**

    ...
    Task sendTask = client.SendAsync(message);

    // Specify a continuation that runs when the task completes
    sendTask.ContinueWith((task) =>
    {
        // Processing that runs when the task is complete.
        Console.WriteLine("SendAsync task status is {0}", task.Status);
    });
    ...

The `HttpWebResponse` class used to obtain a response to a web request in the example shown earlier provides similar functionality. The `GetResponseAsync` method is an asynchronous version of the `GetResponse` method that also runs by creating a new task. You can use a continuation to capture and process the information returned by the web response:

**C#**

    // Construct an HTTP web request
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("..."));
    request.Method = "GET";
    request.ContentType = "application/json";
    
    // Send the request asynchronously
    var responseTask = request.GetResponseAsync();
    
    // Create a continuation to handle the information returned by the response
    responseTask.ContinueWith((task) =>
    {
        HttpWebResponse response = (HttpWebResponse)task.Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseStream = response.GetResponseStream();
            if (responseStream != null)
            {
                using (var responseReader = new StreamReader(responseStream))
                {
                    string result = responseReader.ReadToEnd();
                    Console.WriteLine("{0}", result);
                }
            }
        }
    });
    
    // Processing continues while the request is sent, and the response is received and processed
    Console.WriteLine("Processing while request is handled");

For libraries that do not provide asynchronous versions of operations, you can create asynchronous wrappers around synchronous methods, as shown in the following example:

**C#**


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


**Note 1:** Only use the asynchronous wrapper strategy for methods that are I/O bound. Following this approach for CPU bound operations offers little benefit, and is likely to actually decrease the overall throughput of the system due to the additional overhead of creating and managing tasks.

**Note 2:** Some recent libraries only provide asynchronous versions of certain methods, to prevent the issue of synchronous I/O from arising in the first place. It may be preferable to switch to one of these libraries rather than adding asynchronous wrappers around inherently synchronous code. An example is the `HttpClient` class in the `System.Net.Http` namespace. This method provides methods such as `GetAsync`, `PostAsync`, `PutAsync`, and `DeleteAsync` for interacting with a REST web service. It can be used as an asynchronous alternative to the `HttpWebRequest` class shown in the earlier examples. 

[Link to the related sample][fullDemonstrationOfSolution]


## How to validate the solution
The system should be able to support more concurrent user requests than before, and as a result be more scalable. This can be determined by performing load testing before and after making any changes to the code and then comparing the results. Functionally, the system should remain unchanged. Monitoring the system and analyzing the key performance counters described earlier should indicate that the system spends less time blocked by synchronous I/O and the CPUs are more active. *(NOTE: NEED TO ADD SOME QUANTIFIABLE GUIDANCE)*

## What problems will this uncover?
*TBD - Need more input from the developers*.

This section is very contextual and may not exist for every pattern. The idea is that fixing one problem in a system will likely reveal other problems that were not visible before. We want to give the reader a sense of what they can expect.
For example, increasing the the throughput of your front-end web service may result in overwhelming a downstream service that was previously thought to "run fine".

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
