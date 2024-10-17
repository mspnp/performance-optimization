# ImproperInstantiation Sample Code

It is a sample implementation to ilustrate [Improper Instantiation antipattern](https://learn.microsoft.com/en-us/azure/architecture/antipatterns/improper-instantiation/)

The ImproperInstantiation sample code comprises the following items:

- ImproperInstantiation Web Api

- User Profile Web Api

The ImproperInstantiation Web Api project contains four controllers:

- `NewHttpClientInstancePerRequestController`

- `NewServiceInstancePerRequestController`

- `SingleHttpClientInstanceController`

- `SingleServiceInstanceController`

The `NewServiceInstancePerRequestController` and `SingleServiceInstanceController` both call the `ExpensiveToCreateService.GetProductByIdAsync` method. The `ExpensiveToCreateService` class is
designed to support shared instances. This class uses a delay to simulate setup and configuration.
However, the `NewServiceInstancePerRequestController` and `SingleServiceInstanceController` handle the lifetime of the `ExpensiveToCreateService` instance differently:

- The `NewServiceInstancePerRequestController` creates a new instance of `ExpensiveToCreateService` for every call to `NewServiceInstancePerRequestController.GetProductAsync`:

**C#**

```C#
 [HttpGet("{id}")]
 public async Task<IActionResult> GetProductAsync(string id)
 {
     var expensiveToCreateService = new ExpensiveToCreateService();
     return Ok(await expensiveToCreateService.GetProductByIdAsync(id));
 }
```

- The `SingleServiceInstanceController` delegates the class instantiation to [.NET dependency injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection). In this case, a Singleton (only one instance is created) of `ExpensiveToCreateService` is added.

**C#**

```C#
// on Program.cs a line like
builder.Services.AddSingleton<IExpensiveToCreateService, ExpensiveToCreateService>();

// The cotroller
[ApiController]
[Route("[controller]")]
public class SingleServiceInstanceController(IExpensiveToCreateService expensiveToCreateService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductAsync(string id)
    {
        return Ok(await expensiveToCreateService.GetProductByIdAsync(id));
    }
}
```

The `NewHttpClientInstancePerRequestController` and `SingleHttpClientInstanceController` both use an instance of the `HttpClient` to send requests to the `ImproperInstantiation.UserProfileService`. As with the
previous pair of controllers, they handle the lifetime of the `HttpClient` instance differently:

- The `NewHttpClientInstancePerRequestController` creates a new instance of `HttpClient` and disposes it for every call to `NewHttpClientInstancePerRequestController.GetProductAsync`:

**C#**

```C#
[ApiController]
[Route("[controller]")]
public class NewHttpClientInstancePerRequestController(IConfiguration configuration) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductAsync(string id)
    {
        using (var httpClient = new HttpClient())
        {
            var result = await httpClient.GetStringAsync($"{configuration["api-usuario"]}/UserProfile");

            return Ok(new Product { Name = result });
        }
    }
}
```

There is an [issue](https://learn.microsoft.com/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) where, when an `HttpClient` object is disposed, the underlying socket is not immediately released. Therefore, `HttpClient` is intended to be instantiated once and reused throughout the application's lifetime. Creating a new `HttpClient` instance for every request can exhaust the available sockets under heavy loads.

- The `SingleHttpClientInstanceController` delegates the class instantiation to [.NET dependency injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection). It use [HttpClientFactory](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)

**C#**

```C#
// on Program.cs a line like
builder.Services.AddHttpClient("api-usuario", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["api-usuario"]);
});

// The Controller
[ApiController]
[Route("[controller]")]
public class SingleHttpClientInstanceController(IHttpClientFactory httpClientFactory) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductAsync(string id)
    {
        var httpClient = httpClientFactory.CreateClient("api-usuario");

        var result = await httpClient.GetStringAsync("/UserProfile");

        return Ok(new Product { Name = result });
    }
}
```

## :rocket: Deployment guide

Install the prerequisites and follow the steps to deploy and run the examples.

### Prerequisites

- Permission to create a new resource group and resources in an [Azure subscription](https://azure.com/free)
- Unix-like shell. Also available in:
  - [Azure Cloud Shell](https://shell.azure.com/)
  - [Windows Subsystem for Linux (WSL)](https://learn.microsoft.com/windows/wsl/install)
- [Git](https://git-scm.com/downloads)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- Optionally, an IDE, like [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/).

### Steps

1. Clone this repository to your workstation and navigate to the working directory.

   ```bash
   git clone https://github.com/mspnp/performance-optimization
   cd ImproperInstantiation
   ```

1. Run proyect locally

   Set both web APIs as startup projects. Run them, and you will then be able to call the endpoints.

## Contributions

Please see our [Contributor guide](./CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact <opencode@microsoft.com> with any additional questions or comments.

With :heart: from Azure Patterns & Practices, [Azure Architecture Center](https://azure.com/architecture).
