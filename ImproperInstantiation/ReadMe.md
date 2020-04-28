# ImproperInstantiation Sample Code

The ImproperInstantiation sample code comprises the following items:

* ImproperInstantiation solution file

* AzureCloudService

* UserProfileServiceWebRole

* WebRole WebAPI project

The WebRole WebAPI project contains four controllers:

* `NewHttpClientInstancePerRequestController`

* `NewServiceInstancePerRequestController`

* `SingleHttpClientInstanceController`

* `SingleServiceInstanceController`

The `NewServiceInstancePerRequestController` and `SingleServiceInstanceController` both call the
`ExpensiveToCreateService.GetProductByIdAsync` method. The `ExpensiveToCreateService` class is 
designed to support shared instances. This class uses a delay to simulate setup and configuration.
However, the `NewServiceInstancePerRequestController` and `SingleServiceInstanceController` handle
the lifetime of the `ExpensiveToCreateService` instance differently:

* The `NewServiceInstancePerRequestController` creates a new instance of
`ExpensiveToCreateService` for every call to
`NewServiceInstancePerRequestController.GetProductAsync`:

**C#**

``` C#
public async Task<Product> GetProductAsync(string id)
{
    var expensiveToCreateService = new ExpensiveToCreateService();
    return await expensiveToCreateService.GetProductByIdAsync(id);
}
```

* The `SingleServiceInstanceController` creates a static instance of `ExpensiveToCreateService`
and uses it during the lifetime of the process:

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



The `NewHttpClientInstancePerRequestController` and `SingleHttpClientInstanceController` both use 
an instance of the `HttpClient` to send requests to the `UserProfileServiceWebRole`. As with the 
previous pair of controllers, they handle the lifetime of the `HttpClient` instance differently:

* The `NewHttpClientInstancePerRequestController` creates a new instance of `HttpClient` and
disposes it for every call to `NewHttpClientInstancePerRequestController.GetProductAsync`:

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

* The `SingleHttpClientInstanceController` creates a static instance of `HttpClient` and uses it
during the lifetime of the controller:

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

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the AzureCloudService project and then click *Publish* to deploy the project to Azure.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the application.
For details of the load testing strategy for this sample, see [Load Testing][Load Testing].

## Dependencies

This project requires Azure SDK 2.5

[Load Testing]: docs/LoadTesting.md
