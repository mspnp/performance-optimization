# BusyFrontEnd Sample Code

The BusyFrontEnd sample code comprises the following items:

* BusyFrontEnd solution file

* AzureCloudService

* WebRole WebAPI project

* WorkerRole project

* Common.Logic class library

The WebRole WebAPI project contains three controllers:

* `WorkInFrontEndController`

* `WorkInBackgroundController`

* `UserProfileController`


The `Get` action of the `WorkInFrontEndController` creates a new thread which invokes the static `Calculator.RunLongComputation` method:

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

The `Get` action of the `WorkInBackgroundController` posts a message to a queue for processing by the worker role:

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
The worker role listens for incoming messages and performs the equivalent processing to the `Calculator.RunLongComputation` method over each one.

The `UserProfileController` exposes a `Get` operation that performs a small piece of simulated processing. This processing is intended to run concurrently with the `Get` actions of the other controllers to demonstrate the effects of performing work in the foreground and background on unrelated business operations:

**C#**

``` C#
public UserProfile Get(int id)
{
    //Simulate processing
    return new UserProfile() {FirstName = "Alton", LastName = "Hudgens"};
}
```

## Configuring the project

The `WorkInBackgroundController` uses an Azure Service Bus Queue to send messages to the worker role. Use the Azure Management Portal to create an Azure Service Bus Queue and add the connection string for this queue to the AzureCloudService ServiceConfiguration files.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the AzureCloudService project and then click *Publish* to deploy the project to Azure.
.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the application.
For details of the load testing strategy for this sample, see [Load Testing][Load Testing].

## Dependencies

This project requires Azure SDK 2.5

[Load Testing]: docs/LoadTesting.md
