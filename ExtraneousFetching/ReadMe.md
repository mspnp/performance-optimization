# ExtraneousFetching Sample Code

The ExtraneousFetching sample code comprises the following items:

* ExtraneousFetching solution file

* ExtraneousFetching WebAPI project

* ExtraneousFetching.DataAccess class library

The WebRole WebAPI project contains two controllers:

* `UnnecessaryFieldsController`

* `UnnecessaryRowsController`

The `GetAllFieldsAsync` action of the `UnnecessaryFieldsController` retrieves a collection of product IDs and product names from the AdventureWorks2012 database and returns the result. The action fetches all the details of every product from the database before returning only the data in the product ID and product name fields:

**C#**

``` C#
public async Task<IHttpActionResult> GetAllFieldsAsync()
{
    using (var context = GetContext())
    {
        var products = await context.Products.ToListAsync(); // Execute query.

        var result = products.Select(p => new ProductInfo { Id = p.ProductId, Name = p.Name }); // Project fields.

        return Ok(result);
    }
}
```

The `GetRequiredFieldsAsync` action of the `UnnecessaryFieldsController` performs the same task but only fetches the product ID and product name from the database:

**C#**

``` C#
public async Task<IHttpActionResult> GetRequiredFieldsAsync()
{
    using (var context = GetContext())
    {
        var result = await context.Products
            .Select(p => new ProductInfo {Id = p.ProductId, Name = p.Name}) // Project fields.
            .ToListAsync(); // Execute query.

        return Ok(result);
    }
}
```

The `AggregateOnClientAsync` action of the `UnnecessaryRowsController` calculates the total value of sales made by a salesperson recorded in the database. To do this, it retrieves the details of every sale from the `SalesOrderHeader` table and then iterates through the results to perform the calculation:

**C#**

``` C#
public async Task<IHttpActionResult> AggregateOnClientAsync()
{
    using (var context = GetEagerLoadingContext())
    {
        var salesPersons = await context.SalesPersons
            .Include(sp => sp.SalesOrderHeaders) // This include forces eager loading.
            .ToListAsync();

        decimal total = 0;
        foreach (var salesPerson in salesPersons)
        {
            var orderHeaders = salesPerson.SalesOrderHeaders;

            total += orderHeaders.Sum(x => x.TotalDue);
        }

        return Ok(total);
    }
}
```

The `AggregateOnDatabaseAsync` action of the `UnnecessaryRowsController` also calculates the total value of sales but uses the database to perform the aggregation by using the `Sum` function:

**C#**

``` C#
public async Task<IHttpActionResult> AggregateOnDatabaseAsync()
{
    using (var context = GetContext())
    {
        var query = from sp in context.SalesPersons
                    from soh in sp.SalesOrderHeaders
                    select soh.TotalDue;

        var total = await query.DefaultIfEmpty(0).SumAsync();

        return Ok(total);
    }
}
```

## Configuring the project

This project uses the [AdventureWorks2012][AdventureWorks2012] database stored by using Azure SQL Database. Create the database by using the Azure Management Portal and add the connection string to the `AdventureWorksContext` connection string in the web.config file for the ExtraneousFetching WebAPI project.

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the ExtraneousFetching.WebApi project and then click *Publish*. Publish the project to an Azure website.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the application.

## Dependencies

This project requires:

* Azure SDK 2.5

* An instance of the [AdventureWorks2012] database 

[AdventureWorks2012]: https://msftdbprodsamples.codeplex.com/releases/view/37304

