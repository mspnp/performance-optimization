# Synchronous I/O Sample Code

The Synchronous IO sample code illustrates techniques for retrieving information in a web service and returning it to a client. The sample comprises:
* The SynchronousIO solution file,
* The AzureCloudService project, and
* The WebRole project.

The sample simulates fetching information from a data store. The data returned is a `UserProfile` object (defined in the Models folder in the WebRole project):

**C#**
``` C#
public class UserProfile
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

The code that actually retrieves the data is located in the `UserProfileServiceProxy` class, located in the WebRole project. This class exposes the following three methods:
***C#***
``` C#
public class UserProfileServiceProxy : IUserProfileService
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
These methods demonstrate the synchronous, task-based asynchronous, and wrapped async techniques for fetching data. The methods return hard-coded values, but simulate the delay expected when retrieving information from a remote data store.

WebRole is a Web API project. It includes three controllers:
* `AsyncController`
* `SyncController`
* `WrappedSyncController`

These three WebAPI controllers call the corresponding methods of the `UserProfileServiceProxy` class.

## Deploying to Azure
Right-click the AzureCloudService project and then click *Publish* to deploy the project to Azure.

## Load testing
You can use [Visual Studio Online](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx)  to load test your application.

For more realistic results, configure the user load to simulate bursts of traffic with periods of low usage between bursts of high usage. In order to raise and lower the user load within a load test, you will need to create a [custom load test plugin](https://msdn.microsoft.com/en-us/library/ms243153.aspx).

## Dependencies
- Azure SDK 2.5
