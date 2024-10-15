# ExtraneousFetching Sample Code

It is a sample implementation to ilustrate [Extraneous Fetching antipattern](https://learn.microsoft.com/azure/architecture/antipatterns/extraneous-fetching/)

The ExtraneousFetching sample code comprises the following items:

- ExtraneousFetching Application

- Azure SQL database with [AdventureWorksLT sample](https://learn.microsoft.com/sql/samples/adventureworks-install-configure?view=sql-server-ver16&tabs=ssms#deploy-to-azure-sql-database)

The Api project contains two controllers:

- `UnnecessaryFieldsController`

- `UnnecessaryRowsController`

The `GetAllFieldsAsync` action of the `UnnecessaryFieldsController` retrieves a collection of product IDs and product names from the AdventureWorksLT database and returns the result. The action fetches all the details of every product from the database before returning only the data in the product ID and product name fields:

**C#**

```C#
[HttpGet]
[Route("api/allfields")]
public async Task<IActionResult> GetAllFieldsAsync()
{
    // execute the query
    var products = await context.Products.ToListAsync();

    // project fields from the query results
    var result = products.Select(p => new DTOs.ProductInfo { Id = p.ProductID, Name = p.Name });

    return Ok(result);
}
```

The `GetRequiredFieldsAsync` action of the `UnnecessaryFieldsController` performs the same task but only fetches the product ID and product name from the database:

**C#**

```C#
 [HttpGet]
 [Route("api/requiredfields")]
 public async Task<IActionResult> GetRequiredFieldsAsync()
 {
     // project fields as part of the query itself
     var result = await context.Products
         .Select(p => new DTOs.ProductInfo { Id = p.ProductID, Name = p.Name })
         .ToListAsync();

     return Ok(result);
 }
```

The `AggregateOnClientAsync` action of the `UnnecessaryRowsController` calculates the total value of sales made by a salesperson recorded in the database. To do this, it retrieves the details of every sale from the `SalesOrderHeader` table and then iterates through the results to perform the calculation:

**C#**

```C#
  [HttpGet]
  [Route("api/aggregateonclient")]
  public async Task<IActionResult> AggregateOnClientAsync()
  {
       // fetch all order totals from the database
       var orderAmounts = await context.SalesOrderHeaders.Select(soh => soh.TotalDue).ToListAsync();
       // sum the order totals here in the controller
       var total = orderAmounts.Sum();
       return Ok(total);
  }
```

The `AggregateOnDatabaseAsync` action of the `UnnecessaryRowsController` also calculates the total value of sales but uses the database to perform the aggregation by using the `Sum` function:

**C#**

```C#
   [HttpGet]
   [Route("api/aggregateondatabase")]
   public async Task<IActionResult> AggregateOnDatabaseAsync()
   {
       // fetch the sum of all order totals, as computed on the database server
       var total = await context.SalesOrderHeaders.SumAsync(soh => soh.TotalDue);
       return Ok(total);
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
   cd ExtraneousFetching
   ```

1. Log into Azure and create an empty resource group.

   ```bash
   az login
   az account set -s <Name or ID of subscription>

   export USER=<Microsoft Entra Id user to connect the database>
   export USER_OBJECTID=<Microsoft Entra Id user's object id>
   export USER_TENANTID=<User's Tenant>

   LOCATION=eastus
   RESOURCEGROUP=rg-extraneous-fetching-${LOCATION}

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
