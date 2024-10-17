# NoCaching Sample Code

It is a sample implementation to ilustrate [No Caching antipattern](https://learn.microsoft.com/azure/architecture/antipatterns/no-caching/)

The NoCaching sample code comprises the following items:

- Azure SQL database with [AdventureWorksLT sample](https://learn.microsoft.com/sql/samples/adventureworks-install-configure?view=sql-server-ver16&tabs=ssms#deploy-to-azure-sql-database)

The NoCaching Web API project contains two controllers:

- `NoCacheController`

- `CacheController`

The `NoCacheController` exposes a number of actions that retrieve data from the AdventureWorksLT database. The information is fetched directly from the database(by using the Entity Framework) and is not cached. The following code snippet shows an example:

**C#**

```C#
// Controller
  [ApiController]
  [Route("[controller]")]
  public class NoCacheController(IQueryService _queryService) : ControllerBase
  {
      [HttpGet("products/{id}")]
      public async Task<IActionResult> GetProduct(int id)
      {
          var product = await _queryService.GetProductAsync(id);
          if (product == null)
          {
              return NotFound();
          }
          return Ok(product);
      }

      [HttpGet("productCategories/{productSubcategoryId}")]
      public async Task<IActionResult> GetProductCategories(int productSubcategoryId)
      {
          var product = await _queryService.GetProductCategoryAsync(productSubcategoryId);
          if (product == null)
          {
              return NotFound();
          }
          return Ok(product);
      }
  }
// Service 
public class QueryService(AdventureWorksProductContext _context) : IQueryService
{
    public async Task<ProductDTO> GetProductAsync(int id)
    {
        var product = await _context.Products
            .Where(p => p.ProductID == id)
            .Select(x => new ProductDTO { Name = x.Name, ProductID = x.ProductID })
            .FirstOrDefaultAsync();

        return product;
    }

    public async Task<ProductCategoryDTO> GetProductCategoryAsync(int subcategoryId)
    {
        var subcategories = await _context.ProductCategories
                             .Include(x => x.Products)
                             .Where(x => x.ProductCategoryID == subcategoryId)
                             .Select(x => new ProductCategoryDTO
                             {
                                 ProductCategoryID = x.ProductCategoryID,
                                 ParentProductCategoryID = x.ParentProductCategoryID,
                                 Name = x.Name,
                                 Rowguid = x.Rowguid,
                                 ModifiedDate = x.ModifiedDate,
                                 Products = x.Products.Select(p => new ProductDTO
                                 {
                                     ProductID = p.ProductID,
                                     Name = p.Name
                                 }).ToList()
                             })
                             .FirstOrDefaultAsync();
        return subcategories;
    }
}
```

The `CacheController` provides the same actions. The difference is that the code for these actions a different context class that implements the cache-aside pattern (using .Net [MemoryCache](https://learn.microsoft.com/aspnet/core/performance/caching/memory)):

**C#**

```C#
// Controller
 [ApiController]
 [Route("[controller]")]
 public class CacheController(ICacheQueryService _cacheQueryService) : ControllerBase
 {
     [HttpGet("products/{id}")]
     public async Task<IActionResult> GetProduct(int id)
     {
         var product = await _cacheQueryService.GetProductAsync(id);
         if (product == null)
         {
             return NotFound();
         }
         return Ok(product);
     }

     [HttpGet("productCategories/{productSubcategoryId}")]
     public async Task<IActionResult> GetProductCategories(int productSubcategoryId)
     {
         var product = await _cacheQueryService.GetProductCategoryAsync(productSubcategoryId);
         if (product == null)
         {
             return NotFound();
         }
         return Ok(product);
     }
 }
// Service that caches the database requests
 public class CacheQueryService(IMemoryCache _memoryCache, IQueryService _queryService) : ICacheQueryService
 {
     public async Task<ProductDTO> GetProductAsync(int id)
     {
         return await _memoryCache.GetOrCreateAsync($"product-{id}", async entry =>
         {
             entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
             return await _queryService.GetProductAsync(id);
         });
     }

     public async Task<ProductCategoryDTO> GetProductCategoryAsync(int subcategoryId)
     {
         return await _memoryCache.GetOrCreateAsync($"productCategory-{subcategoryId}", async entry =>
         {
             entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
             return await _queryService.GetProductCategoryAsync(subcategoryId);
         });
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
   cd NoCaching
   ```

1. Log into Azure and create an empty resource group.

   ```bash
   az login
   az account set -s <Name or ID of subscription>

   export USER=<Microsoft Entra Id user to connect the database>
   export USER_OBJECTID=<Microsoft Entra Id user's object id>
   export USER_TENANTID=<User's Tenant>

   LOCATION=eastus
   RESOURCEGROUP=rg-no-caching-${LOCATION}

   az group create --name ${RESOURCEGROUP} --location ${LOCATION}

   ```

1. Deploy the supporting Azure resources.  
   It will create a database that only allows Microsoft Entra ID users, including the AdventureWorksLT sample

   ```bash
   az deployment group create --resource-group ${RESOURCEGROUP}  \
                        -f ./bicep/main.bicep  \
                        -p user=${USER} \
                        userObjectId=${USER_OBJECTID} \
                        userTenantId=${USER_TENANTID}
   ```

1. Configure database connection string

   On appsettings.json you need to complete with your server and database name.

   ```bash
   "Server=tcp:<yourServer>.database.windows.net,1433;Database=<yourDatabase>;Authentication=ActiveDirectoryDefault; Encrypt=True;TrustServerCertificate=false;Connection Timeout=30;",
   ```

1. Authenticate with a Microsoft Entra identity

   It uses [Active Directory Default](https://learn.microsoft.com/sql/connect/ado-net/sql/azure-active-directory-authentication?view=sql-server-ver16#setting-microsoft-entra-authentication) which requires authentication via AZ CLI or Visual Studio.

1. Enable your computer to reach the Azure Database:

   - Go to the Database Server.
   - In the Network section, allow your IP address.

1. Run proyect locally

   Execute the API and then you will be able to call both endpoints

## :broom: Clean up resources

Most of the Azure resources deployed in the prior steps will incur ongoing charges unless removed.

```bash
az group delete -n ${RESOURCEGROUP} -y
```

## Contributions

Please see our [Contributor guide](./CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact <opencode@microsoft.com> with any additional questions or comments.

With :heart: from Azure Patterns & Practices, [Azure Architecture Center](https://azure.com/architecture).