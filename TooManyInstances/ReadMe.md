# TooManyInstances Sample Code

The TooManyInstances sample code is composed of:
* TooManyInstances solution file
* AzureCloudService
* WebRole WebAPI project

The WebRole WebAPI project has two controllers:
* NewInstancePerRequestController
* SingleInstanceController

These two WebAPI controllers both call the ProductRepository.GetProductByIdAsync method but handle the lifetime of the ProductRepository instance differently. The NewInstancePerRequestController creates a new instance of ProductRepository and disposes it for every call to NewInstancePerRequestController.GetProductAsync.

**C#**

``` C#
public async Task<Product> GetProductAsync(string id)
{
    using (var productRepository = new ProductRepository())
    {
        return await productRepository.GetProductByIdAsync(id).ConfigureAwait(false);
    }
}
```

The SingleInstanceController creates a static instance of ProductRepository and uses it during the lifetime of the process.

**C#**

``` C#
private static readonly IProductRepository ProductRepository;

static SingleInstanceController()
{
    ProductRepository = new ProductRepository();
}

public Task<Product> GetProductAsync(string id)
{
    return ProductRepository.GetProductByIdAsync(id);
}
```

The ProductRepository simulates a class who's instance is intended to be shared. This class has an instance member of type System.Net.Http.HttpClient and uses a delay to simulate setup and configuration. Instances of HttpClient should be shared, otherwise under load, you will run out of TCP sockets. 

## Deploying to Azure
Right-click on the AzureCloudService and select "Publish" to deploy to Azure.

## Load testing
You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) your application.

## Dependencies
Azure SDK 2.5
