# BusyDatabase Sample Code

The BusyDatabase sample code comprises the following items:

* BusyDatabase solution file

* AzureCloudService

* BusyDatabase WebAPI project

* BusyDatabase.Support class library

The BusyDatabase WebAPI project contains two controllers:

* `TooMuchProcSqlController`

* `LessProcSqlController`

The `Get` action of both controllers returns an XML formatted list of order details.
The `TooMuchProcSqlController` runs the Transact-SQL statement defined in the
TooMuchProcSql.sql file in the BusyDatabase.Support project to retrieve and format the
data by using Azure SQL Database. The `LessProcSqlController` uses the simpler
Transact-SQL query defined in the LessProcSql.sql file in the BusyDatabase.Support
project to retrieve the data and uses the XML library of the .NET Framework to format
the result.

## Configuring the project

Both controllers use the [AdventureWorks2012][AdventureWorks2012] database stored by
using Azure SQL Database. Create the database by using the Azure Management Portal and
add the connection string to the `connectionString` app setting in the web.config file
for the BusyDatabase WebAPI project.

Note that the new Azure portal provides a simplified version of the database (AdventureWorksLT). The AdventureWorksLT database uses a different schema from that expected by this sample application which might not function correctly unless the full [AdventureWorks2012][AdventureWorks2012] database is installed.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the AzureCloudService project and then
click *Publish* to deploy the project to Azure.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the
application.

## Dependencies

This project requires:

* Azure SDK 2.5

* An instance of the [AdventureWorks2012] database

[AdventureWorks2012]: https://msftdbprodsamples.codeplex.com/releases/view/37304
