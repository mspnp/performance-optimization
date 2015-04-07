# SynchronousIO Sample Code

The SynchronousIO sample code illustrates techniques for retrieving information from a web service and returning it to a client. The sample comprises the following items:

* SynchronousIO solution file

* AzureCloudService

* CreateFileToUpload project

* WebRole WebAPI project

The sample simulates fetching information from a data store. The data returned is a
`UserProfile` object (defined in the Models folder in the WebRole project):

**C#**
``` C#
public class UserProfile
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

The code that actually retrieves the data is located in the `FakeUserProfileService`
class, located in the WebRole project. This class exposes the following three methods:
***C#***
``` C#
public class FakeUserProfileService : IUserProfileService
{
    public UserProfile GetUserProfile()
    {
        ...
    }

    public async Task<UserProfile> GetUserProfileAsync()
    {
        ...
    }

    public Task<UserProfile> GetUserProfileWrappedAsync()
    {
        ...
    }
}
```
These methods demonstrate the synchronous, task-based asynchronous, and wrapped async
techniques for fetching data. The methods return hard-coded values, but simulate the
delay expected when retrieving information from a remote data store.

WebRole is a Web API project. It contains five controllers:

* `AsyncController`

* `AsyncUploadController`

* `SyncController`

* `SyncUploadController`

* `WrappedSyncController`

The `AsyncController`, `SyncController`, and `WrappedSyncController` WebAPI
controllers call the corresponding methods of the `FakeUserProfileService` class.

The `AsyncUploadController` and `SyncUploadController` WebAPI controllers call
corresponding methods in the Azure Blob storage sdk to upload the "FileToUpload.txt"
file to Blob storage.

The CreateFileToUpload project is a console app that can be used to generate a file
named "FileToUpload.txt" that is 10 MB in size.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the AzureCloudService project and then
click *Publish* to deploy the project to Azure.


## Load testing

You can use [Visual Studio Online](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx)  to
load test your application.

For more realistic results, configure the user load to simulate bursts of traffic with
periods of low usage between bursts of high usage. In order to raise and lower the
user load within a load test, you will need to create a [custom load test plugin](https://msdn.microsoft.com/en-us/library/ms243153.aspx).

## Dependencies
This project requires Azure SDK 2.5
