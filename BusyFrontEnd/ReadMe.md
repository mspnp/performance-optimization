# BusyFrontEnd Sample Code

The BusyFrontEnd sample code is composed of:
* BusyFrontEnd solution file
* AzureCloudService
* WebRole WebAPI project
* WorkerRole project
* Common.Logic class library

The WebRole WebAPI project has two controllers:
* `WorkInFrontEndController`
* `WorkInBackgroundController`


The WorkInFrontEndController's Get action creates a new thread which executes a call the Calculator.RunLongComputation method.

**C#**

``` C#
public void Get(double number)
{
    new Thread(() =>
    {
        Trace.WriteLine("Number: " + number);
        var result = Calculator.RunLongComputation(number);
        Trace.WriteLine("Result: " + result);
    }).Start();

}
```

The WorkInBackgroundController's Get action puts a message on a queue for processing by the WorkerRole.

**C#**

``` C#
public Task Get(double number)
{
    return ServiceBusQueueHandler.AddWorkLoadToQueueAsync(
            QueueClient,
            QueueName,
            number);
}
```

## Configure
The WorkInBackgroundController and WorkerRole communicate with the use of an Azure Service Bus Queue. Please create an Azure Service Bus Queue instance and provide its connection string in the AzureCloudService ServiceConfiguration files.

## Deploying to Azure
Right-click on the AzureCloudService and select "Publish" to deploy to Azure.

## Load testing
You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) your application.

## Dependencies
Azure SDK 2.5
