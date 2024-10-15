# ChattyIO Sample Code

It is a sample implementation to ilustrate [Chatty I/O antipattern](https://learn.microsoft.com/azure/architecture/antipatterns/chatty-io/)

The ChattyIO sample code comprises the following items:

- ChattyIO Application

- Azure SQL database with [AdventureWorksLT sample](https://learn.microsoft.com/sql/samples/adventureworks-install-configure?view=sql-server-ver16&tabs=ssms#deploy-to-azure-sql-database)

The ChattyIO WebAPI project contains two controllers:

* `ChattyProductController`

* `ChunkyProductController`

The `GetProductsInSubCategoryAsync` action of the `ChattyProductController` retrieves the details of all products held, including sales order details, in the specified subcategory in the AdventureWorksLT database. The code connects to the database and fetches the details of the subcategory, the products in the subcategory, and the saless order details of each product as separate database requests before formatting the data and returning the result.

The `GetProductCategoryDetailsAsync` action of the `ChunkyProductController` performs a similar task except that it fetches the data from the database in a single request.

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
   cd ChattyIO
   ```

1. Log into Azure and create an empty resource group.

   ```bash
   az login
   az account set -s <Name or ID of subscription>

   export USER=<Microsoft Entra Id user to connect the database>
   export USER_OBJECTID=<Microsoft Entra Id user's object id>
   export USER_TENANTID=<User's Tenant>

   LOCATION=eastus
   RESOURCEGROUP=rg-chatty-io-${LOCATION}

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