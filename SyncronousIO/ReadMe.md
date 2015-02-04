# Synchronous I/O Sample Code

The Synchronous IO sample code code is composed of:
* SyncronousIO solution file
* AzureCloudService
* WebRole WebAPI project

The WebRole WebAPI project has three controllers:
* AsyncController
* SyncController
* WrappedSyncController

These three WebAPI controllers call the respective methods on the UserProfileServiceProxy class. The UserProfileServiceProxy methods demonstrate Synchronous, Task based Asynchronous, and wrapped Async techniques.

**C#**

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
```
## Deploying to Azure
Right-click on the AzureCloudService and select "Publish" to deploy to Azure.

## Load testing
You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) your application.

For more realistic results, configure the user load to simulate bursts of traffic with periods of low usage between bursts of high usage. In order to raise and lower the user load within a load test, you will need to create a [custom load test plugin](https://msdn.microsoft.com/en-us/library/ms243153.aspx).
Carlos may have a load test plug that we could ship.
