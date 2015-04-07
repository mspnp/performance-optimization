# ExtraneousFetching Sample Code

The ExtraneousFetching sample code comprises the following items:

* ExtraneousFetching solution file

* ExtraneousFetching WebAPI project (*Note that this project is an Azure web application and not a cloud service*)

* ExtraneousFetching.DataAccess class library

The WebRole WebAPI project contains two controllers:

* `UnnecessaryFieldsController`

* `UnnecessaryRowsController`

The `GetAllFieldsAsync` action of the `UnnecessaryFieldsController` retrieves a
collection of product IDs and product names from the AdventureWorks2012 database and
returns the result. The action fetches all the details of every product from the
database before returning only the data in the product ID and product name fields:

**C#**

``` C#
public async Task<IHttpActionResult> GetAllFieldsAsync()
{
    using (var context = new AdventureWorksContext())
    {
        // execute the query
        var products = await context.Products.ToListAsync();

        // project fields from the query results
        var result = products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name });

        return Ok(result);
    }
}
```

The `GetRequiredFieldsAsync` action of the `UnnecessaryFieldsController` performs the
same task but only fetches the product ID and product name from the database:

**C#**

``` C#
public async Task<IHttpActionResult> GetRequiredFieldsAsync()
{
    using (var context = new AdventureWorksContext())
    {
        // project fields as part of the query itself
        var result = await context.Products
            .Select(p => new ProductInfo {Id = p.ProductId, Name = p.Name})
            .ToListAsync();

        return Ok(result);
    }
}
```

The `AggregateOnClientAsync` action of the `UnnecessaryRowsController` calculates the
total value of sales made by a salesperson recorded in the database. To do this, it
retrieves the details of every sale from the `SalesOrderHeader` table and then
iterates through the results to perform the calculation:

**C#**

``` C#
public async Task<IHttpActionResult> AggregateOnClientAsync()
{
    using (var context = new AdventureWorksContext())
    {
        // fetch all order totals from the database
        var orderAmounts = await context.SalesOrderHeaders.Select(soh => soh.TotalDue).ToListAsync();

        // sum the order totals here in the controller
        var total = orderAmounts.Sum();

        return Ok(total);
    }
}
```

The `AggregateOnDatabaseAsync` action of the `UnnecessaryRowsController` also
calculates the total value of sales but uses the database to perform the aggregation
by using the `Sum` function:

**C#**

``` C#
public async Task<IHttpActionResult> AggregateOnDatabaseAsync()
{
    using (var context = new AdventureWorksContext())
    {
        // fetch the sum of all order totals, as computed on the database server
        var total = await context.SalesOrderHeaders.SumAsync(soh => soh.TotalDue);

        return Ok(total);
    }
}
```

## Configuring the project

This project uses the [AdventureWorks2012][AdventureWorks2012] database stored by
using Azure SQL Database. Create the database by using the Azure Management Portal and
add the connection string to the `AdventureWorksContext` connection string in the
web.config file for the ExtraneousFetching WebAPI project. Note that the new Azure
portal provides a simplified version of the database (AdventureWorksLT). The
AdventureWorksLT database uses a different schema from that expected by this sample
application which might not function correctly unless the full
[AdventureWorks2012][AdventureWorks2012] database is installed.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the ExtraneousFetching.WebApi project
and then click *Publish*. Publish the project to an Azure website.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the
application.

## Dependencies

This project requires:

* Azure SDK 2.5

* An instance of the [AdventureWorks2012] database

[AdventureWorks2012]: https://msftdbprodsamples.codeplex.com/releases/view/37304
