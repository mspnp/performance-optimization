# Busy Front End

In an interactive web application, running resource-intensive tasks as part of the user interface processing can result in poor response times and high latency for user operations. One often-considered technique to improve response times is to offload a resource-intensive task onto a separate thread. This strategy enables the user interface to continue functioning while the processing is performed in the background. 

As an example, the following code shows part web site hosted by an organization specializing in graphics services. This organization offers digital image processing to customers. Customers can upload photographs to the service by sending an HTTP `POST` request to the *images* URI (uploading is asynchronous). The response message contains a unique ID for the image. When the photograph has been uploaded, the customer can invoke the `ProcessImage` request (the request should include the ID of the image). Image processing may take some time so it is also performed asynchronously to avoid delaying the user. A customer can query whether image processing has completed by sending an HTTP `GET` request to the *images/{imageID}/iscomplete* URI (where imageID is the unique identifier of the photograph being processed. Finally, when processing is complete, the customer an issue an HTTP `GET` request to the *images/imageID* URI to retrieve the image.

**C# Web API**
```C#
[RoutePrefix("frontendimageprocessing")]
public class FrontEndImageProcessingController : ApiController
{
    [Route("images")]
    [HttpPost]
    public async Task<HttpResponseMessage> Post()
    {
        // Logic to upload an image
        ...
        return Request.CreateResponse(HttpStatusCode.OK);
    }

    [Route("processimage")]
    [HttpPost]
    public HttpResponseMessage ProcessImage()
    {
        new Thread(() =>
        {
            // Image processing logic
            ...
        }).Start();

        return Request.CreateResponse(HttpStatusCode.Accepted);
    }

    [Route("images/{imageID}/iscomplete")]
    [HttpGet]
    public bool IsImageProcessingComplete(int imageID)
    {
        // Poll to see whether processing of the specified image has finished
        bool isFinished = ...
        return isFinished;
    }

    [Route("images/{imageID}")]
    [HttpGet]
    public HttpResponseMessage Get(int imageID)
    {
        // Logic to retrieve the processed image
        ...
        return Request.CreateResponse(HttpStatusCode.OK);
    }
}
```

The primary concern with this web application is the resource requirements of the `ProcessImage` method. Although it runs on a background thread, alleviating the user of the need to wait for the result before continuing with further work, it can still consume considerable processing resources. These resources are shared with the other operations being performed by other concurrent users. If a moderate number of users issue requests to perform image processing at the same time, then the overall performance of the system is likely to suffer, causing a slowdown in all operations; users might experience a significant slowing of the image upload and download operations, for example.

----------

**Note:** The `FrontEndImageProcessing` controller is included in the [sample code][fullDemonstrationOfProblem] available with this anti-pattern.

----------

This problem typically occurs when an application is developed as a monolithic whole, with the entire business processing combined into a single tier shared with the user interface.

## How to detect the problem

Symptoms of a busy front end in an application include high latency during periods when resource-intensive tasks are being performed. These tasks can starve other requests of the processing power they require, causing them to run more slowly. End-users are likely to report extended response times and possible failures caused by services timing out due to lack of processing resources in the web server. These failures could manifest themselves as HTTP 500 (Internal Server) errors or HTTP 503 (Service Unavailable) errors. In these cases, you should examine the event logs for the web server which are likely to contain more detailed information about the causes and circumstances of the errors.

You can perform the following steps to help identify the causes of any problems:

1. Identify points at which response times slow down by performing process monitoring of the production system.

2. Examine the telemetry data captured at these points to determine the mix of operations being performed and the resources being utilized by these operations and find any correlations between repeated occurrences of poor response times and the volumes/combinations of each operation that are running at that point.

3. Perform load testing of each possible operation to identify the *bad actors* (operations that are consuming resources and starving other operations).

4. Review the source code for the possible bad actors to identify the reasons for excessive resource consumption.

The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you to examine applications and services systematically.

----------

### Identifying points of slow-down

Instrumenting each method to track the duration and resources consumed by each requests and then monitoring the live system can help to provide an overall view of how the requests compete with each other. During periods of stress, slow-running resource hungry requests will likely impact other operations, and this behavior can be observed by monitoring the system and noting the drop-off in performance. 

The following image shows some of the statistics in the Metrics Explorer gathered by using Microsoft Application Insights against an installation of a cloud service that hosts the `FrontEndImageProcessing` controller. The cloud service comprised two small web role instances under a relatively constant load averaging 100 users. You should note the volumes of each request and the average response time. In this case, the `processimage` request accounted for the largest average response time: 

![Application Insights metrics showing the relative performance of each request][App-Insights-Metrics-Front-End-All-Requests]

### Examining telemetry data and finding correlations

A detailed analysis of the telemetry data for each request should indicate the resources that are being utilized. You should look for areas of contention; resources that are shared by multiple concurrent requests. Resources may be memory or CPU utilization, files on disk, or they could be other items such as database connections or threads in a thread pool. This information can help you to form a hypothesis concerning which requests are proving to be the most disruptive to others. In the example application, the `processimage` request is suspected to be a major cause of CPU contention.

### Performing load-testing to identify *bad actors*

Having identified the candidate disruptive requests in the system, you should perform tests in a controlled environment to demonstrate any correlations between these requests and the overall performance of the system. As an example, you can perform a series of load tests that include and then omit each request in turn to see the effects. The graph below shows the results of a load-test performed by using 100 simulated users against an identical deployment of the cloud service hosting the `FrontEndImageProcessing` controller. The load-test performed an equal mix (25%) for each of the requests exposed by the controller (`Post`, `ProcessImage`, `IsImageProcessingComplete`, and `Get`)

![Initial load-test results for the FrontEndImageProcessing controller][Initial-Load-Test-Results-Front-End]

The graph shows how many calls to each request were completed over time. Overall, the test made 676 successful requests over a 5 minute period. Given that this workload represents 100 users, then each user had to wait on average 44.4 seconds on average to complete a request.

Repeating the load-test with the `ProcessImage` request omitted and the remaining tests distributed equally (33%) yields the following results:

![Load-test results for the FrontEndImageProcessing controller with the ProcessImage tests omitted][Second-Load-Test-Results-Front-End]

This time the test complete 11973 successful requests. The average response time per user dropped to 2.5 seconds on average.

Overall, these results suggest that the code for the `ProcessImage` request is worthy of further scrutiny.

### Reviewing the source code

The final stage is to examine the source code for each of the `bad actors` previously identified. In the case of the `ProcessImage` method, the development team was aware that this request could take a considerable amount of time which is why the processing is performed on an asynchronous thread. In this way a user issuing the request does not have to wait for processing to complete before being able to continue with the next task:

**C#**
```C#
public HttpResponseMessage ProcessImage()
{
    new Thread(() =>
    {
        // Image processing logic
        ...
    }).Start();

    return Request.CreateResponse(HttpStatusCode.Accepted);
}
```

However, although this approach notionally improves response time for the user, it introduces a small overhead associated with creating and managing a new thread. Additionally, the work performed by this method still consumes CPU, memory, and other resources. Enabling this process to run asynchronously might actually be damaging to performance as users can possibly trigger a large number of these operations simultaneously, in an uncontrolled manner. In turn, this has an effect on any other operations that the server is attempting to perform.

## How to correct the problem

You should move processes that might consume significant resources to a separate tier, and control the way in which these processes run to prevent competition from causing resource starvation. 

With Azure, you can offload the image processing work to a set of worker roles. The `ProcessImage` request in the web role can submit the details of the request to a queue, and instances of the web role can pick up these requests and perform the necessary tasks. The web role is then free to focus on user-facing tasks. Furthermore, the queue acts as a natural load-leveller, buffering requests until a worker role instance is available. If the queue length becomes too long, you can configure auto-scaling to start additional worker role instances, and shut these instances down when the workload eases.

The following code snippet shows the amended version of the web API controller and the `ProcessImage` method:

**C# web API**
```C#
[RoutePrefix("backgroundimageprocessing")]
public class BackgroundProcessingController : ApiController
{
    private const string ServiceBusConnectionString = ...;
    private const string AppSettingKeyServiceBusQueueName = ...;

    private readonly QueueClient QueueClient;
    private readonly string QueueName;

    public BackgroundProcessingController()
    {
        var serviceBusConnectionString = ...;
        QueueName = ...;
        QueueClient = ServiceBusQueueHandler.GetQueueClientAsync(serviceBusConnectionString, QueueName).Result;
    }

    ...

    [Route("processimage")]
    [HttpPost]
    public HttpResponseMessage ProcessImage()
    {
        ServiceBusQueueHandler.AddWorkLoadToQueueAsync(QueueClient, QueueName, ...);
        return Request.CreateResponse(HttpStatusCode.Accepted);
    }
}
```

The worker role listens for incoming messages on the queue and performs the image processing:

**C#**
```C#
public class WorkerRole : RoleEntryPoint
{
    ...
    private QueueClient _queueClient;
    ...

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
        this._queueClient.OnMessageAsync(
            async (receivedMessage) =>
            {
                try
                {
                    // Process the message
                    var data = receivedMessage.GetBody<string>();
    
                    // Perform image processing 
                    ...

                    await receivedMessage.CompleteAsync();
                }
                catch
                {
                    receivedMessage.Abandon();
                }
            });
        ...
    }
    ...
}
```
----------

**Note:** The `BackgroundProcessing` controller and the worker role are included in the [sample code][fullDemonstrationOfSolution] available with this anti-pattern.

----------

You should consider the following points:

- This architecture complicates the structure of the solution. In the example, you must ensure that you handle queuing and dequeuing safely to avoid losing requests in the event of a failure.

- The processing environment must be sufficiently scalable to handle the expected workload and meet the required throughput targets.

- Using a web role is simply one solution. Azure also provide other options such as [WebJobs][WebJobs]. 


## Consequences of the solution

Running the [sample solution][fullDemonstrationOfSolution] in a production environment and using Application Insights to monitor performance generated the following results:

![Application Insights metrics for the BackgroundImageProcessing controller][App-Insights-Metrics-Background-All-Requests]

The load was similar to that shown earlier, but the response times of the `Post`, `Get`, and `IsImageProcessingComplete` requests is now much faster. A similar volume of requests was made to the `ProcessImage` method, but you should not compare the response time for these requests to those shown earlier as the times are simply a measure of how long the it took to post a message to a queue.

Repeating the controlled load-test over 5 minutes for 100 users submitting a mixture of all four requests gives the following results:

![Load-test results for the BackgroundImageProcessing controller][Load-Test-Results-Background]

This graph confirms the improvement in performance of the `Post`, `Get`, and `IsImageProcessingComplete` requests as a result of offloading the processing for the `ProcessImage` requests.

Relocating resource-hungry processing to a separate set of processes should improve responsiveness for most requests, but the resource-hungry processing itself may take longer (this duration is not illustrated in the two graphs above, and requires instrumenting and monitoring the worker role.) If there are insufficient worker role instances available to perform the resource-hungry workload, jobs might be queued or otherwise held pending for an indeterminate period. However, it might be possible to expedite critical jobs that must be performed quickly by using a priority queuing mechanism.

## Related resources

- [Azure Service Bus Queues][ServiceBusQueues]

- [Queue-Based Load Leveling Pattern][QueueBasedLoadLeveling] 

- [Priority Queue Pattern][PriorityQueue]

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[WebJobs]: http://www.hanselman.com/blog/IntroducingWindowsAzureWebJobs.aspx
[ServiceBusQueues]: https://msdn.microsoft.com/library/azure/hh367516.aspx
[QueueBasedLoadLeveling]: https://msdn.microsoft.com/library/dn589783.aspx
[PriorityQueue]: https://msdn.microsoft.com/library/dn589794.aspx
[App-Insights-Metrics-Front-End-All-Requests]: Figures/AppInsightsAllTests.jpg
[Initial-Load-Test-Results-Front-End]: Figures/InitialLoadTestResultsFrontEnd.jpg
[Second-Load-Test-Results-Front-End]: Figures/SecondLoadTestResultsFrontEnd.jpg
[App-Insights-Metrics-Background-All-Requests]: Figures/AppInsightsBackground.jpg
[Load-Test-Results-Background]: Figures/LoadTestResultsBackground.jpg

