# MonolithicPersistence Sample Code

The MonolithicPersistence sample code comprises the following items:

* MonolithicPersistence solution file

* AzureCloudService

* MonolithicPersistence.WebRole WebAPI project

The MonolithicPersistence.WebRole project contains two controllers:

* `MonoController`

* `PolyController`

The `Post` action of both controllers add a new `PurchaseOrderHeader` record to the
[AdventureWorks2012][AdventureWorks2012] database deployed in the cloud, and then
create a log record that describes the operation just performed. The following snippet
shows the code for the `MonoController`. The `PolyController` is very similar, except
that the log record is written to a different database:

**C#**
``` C#
public class MonoController : ApiController
{
    private static readonly string ProductionDb = CloudConfigurationManager.GetSetting("ProductionSqlDbCnStr");
    public const string LogTableName = "MonoLog";

    public async Task<IHttpActionResult> PostAsync()
    {
        await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

        await DataAccess.LogAsync(ProductionDb, LogTableName);

        return Ok();
    }
}
```

## Configuring the project

Both controllers use the [AdventureWorks2012][AdventureWorks2012] database stored by
using Azure SQL Database. Create the database by using the Azure Management Portal and
add the connection string to the `ProductionSqlDbCnStr` setting in the Service
Configuration files in the AzureCloudService project.

The `PolyController` also requires a separate Azure SQL Database to hold the log
table. Create a new database on another SQL server by using the Azure Management
Portal and add the connection string to the `LogSqlDbCnStr` setting in the
Service Configuration files in the AzureCloudService project.

Note that the new Azure portal provides a simplified version of the database (AdventureWorksLT). The AdventureWorksLT database uses a different schema from that expected by this sample application which might not function correctly unless the full [AdventureWorks2012][AdventureWorks2012] database is installed.

The tables used to hold log records are created automatically when the service starts running.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the AzureCloudService project and then click *Publish* to deploy the project to Azure.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the application.

## Dependencies

This project requires:

* Azure SDK 2.5

* An instance of the [AdventureWorks2012] database

* An empty Azure SQL Database instance running on a different SQL server

[AdventureWorks2012]: https://msftdbprodsamples.codeplex.com/releases/view/37304
