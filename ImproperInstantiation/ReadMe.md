# TooManyInstances Sample Code

The TooManyInstances sample code is composed of:
* TooManyInstances solution file
* AzureCloudService
* UserProfileServiceWebRole
* WebRole WebAPI project

The WebRole WebAPI project has four controllers:
* `NewHttpClientInstancePerRequestController`
* `NewServiceInstancePerRequestController`
* `SingleHttpClientInstanceController`
* `SingleServiceInstanceController`

The NewServiceInstancePerRequestController and SingleServiceInstanceController both call the `ExpensiveToCreateService.GetProductByIdAsync` method but handle the lifetime of the` `ExpensiveToCreateService` instance differently. 
The `NewServiceInstancePerRequestController` creates a new instance of `ExpensiveToCreateService` for every call to `NewServiceInstancePerRequestController.GetProductAsync`.

**C#**

``` C#
public async Task<Product> GetProductAsync(string id)
{
    var expensiveToCreateService = new ExpensiveToCreateService();
    return await expensiveToCreateService.GetProductByIdAsync(id);
}
```

The `SingleServiceInstanceController` creates a static instance of `ExpensiveToCreateService` and uses it during the lifetime of the process.

**C#**

``` C#
private static readonly ExpensiveToCreateService ExpensiveToCreateService;

static SingleServiceInstanceController()
{
    ExpensiveToCreateService = new ExpensiveToCreateService();
}

public async Task<Product> GetProductAsync(string id)
{
    return await ExpensiveToCreateService.GetProductByIdAsync(id);
}
```

The `ExpensiveToCreateService` simulates a class whose instance is intended to be shared. This class uses a delay to simulate setup and configuration. 

The NewHttpClientInstancePerRequestController and SingleHttpClientInstanceController both use 'HttpClient' to call the UserProfileServiceWebRole but handle the lifetime of the 'HttpClient' instance differently. 
The NewHttpClientInstancePerRequestController creates a new instance of 'HttpClient' and disposes it for every call to 'NewHttpClientInstancePerRequestController.GetProductAsync'.

**C#**

``` C#
public async Task<Product> GetProductAsync(string id)
{
    using (var httpClient = new HttpClient())
    {
        var hostName = HttpContext.Current.Request.Url.Host;
        var result = await httpClient.GetStringAsync(string.Format("http://{0}:8080/api/userprofile", hostName));

        return new Product { Name = result };
    }
}
```

The 'SingleHttpClientInstanceController' creates a static instance of `HttpClient` and uses it during the lifetime of the process.

**C#**

``` C#
private static readonly HttpClient HttpClient;

static SingleHttpClientInstanceController()
{
    HttpClient = new HttpClient();
}

public async Task<Product> GetProductAsync(string id)
{
    var hostName = HttpContext.Current.Request.Url.Host;
    var result = await HttpClient.GetStringAsync(string.Format("http://{0}:8080/api/userprofile", hostName));

    return new Product { Name = result };
}
```

## Deploying to Azure
Right-click on the AzureCloudService and select "Publish" to deploy to Azure.

## Load testing
You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) your application.

## Dependencies
Azure SDK 2.5
