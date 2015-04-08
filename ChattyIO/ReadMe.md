# ChattyIO Sample Code

The BusyFrontEnd sample code comprises the following items:

* ChattyIO solution file

* ChattyIO WebAPI project

* ChattyIO.DataAccess class library

The ChattyIO WebAPI project contains two controllers:

* `ChattyProductController`

* `ChunkyProductController`

The `GetProductsInSubCategoryAsync` action of the `ChattyProductController` retrieves the details of all products held, including price histories, in the specified subcategory in the AdventureWorks2012 database. The code connects to the database and fetches the details of the subcategory, the products in the subcategory, and the price history of each product as separate database requests before formatting the data and returning the result.

The `GetProductCategoryDetailsAsync` action of the `ChunkyProductController` performs a similar task except that it fetches the data from the database in a single request.

## Configuring the project

This project uses the [AdventureWorks2012][AdventureWorks2012] database stored by using Azure SQL Database. Create the database by using the Azure Management Portal and add the connection string to the `AdventureWorksProductContext` connection string in the web.config file for the ChattyIO WebAPI project.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the ChattyIO.WebApi project and then click *Publish*. Publish the project to an Azure website.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the application.
For details of the load testing strategy for this sample, see [Load Testing][Load Testing].

## Dependencies

This project requires:

* Azure SDK 2.5

* An instance of the [AdventureWorks2012] database

[AdventureWorks2012]: https://msftdbprodsamples.codeplex.com/releases/view/37304
[Load Testing]: docs/LoadTesting.md
