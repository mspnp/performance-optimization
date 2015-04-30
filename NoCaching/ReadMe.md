# NoCaching Sample Code

The NoCaching sample code comprises the following items:

* NoCaching solution file

* AzureCloudService

* NoCaching.WebRole WebAPI project

* NoCaching class library

* [Detailed Documentation][docs]

The NoCaching.WebRole WebAPI project contains two controllers:

* `NoCacheController`

* `CacheController`

The `NoCacheController` exposes a number of actions that retrieve data from the
AdventureWorks2012 database. The information is fetched directly from the database
(by using the Entity Framework) and is not cached. The following code snippet shows
an example:

**C#**

``` C#
public async Task<IHttpActionResult> GetPerson(int id)
{
    IPersonRepository repository = new PersonRepository();
    var person = await repository.GetAsync(id);

    if (person == null) return NotFound();
    return Ok(person);
}
```

The `CacheController` provides the same actions. The difference is that the code for
these actions a different context class that implements the cache-aside pattern:

**C#**

``` C#
public async Task<IHttpActionResult> GetPerson(int id)
{
    IPersonRepository repository = new CachedPersonRepository(new PersonRepository());
    var person = await repository.GetAsync(id);

    if (person == null) return NotFound();
    return Ok(person);
}
```

In this code snippet, the `CachedPersonRepository` class provides a wrapper around
the `PersonRepository` class. The `CachedPersonRepository` class checks to see
whether the requested information has been previously retrieved and cached, otherwise
it uses a `PersonRepository` object to fetch the information from the database which
is then added to the cache. The cache is implemented by using [Azure Redis Cache][AzureRedisCache]:

**C#**

``` C#
public class CachedPersonRepository : IPersonRepository
{
    private readonly PersonRepository _innerRepository;

    public CachedPersonRepository(PersonRepository innerRepository)
    {
        _innerRepository = innerRepository;
    }

    public async Task<Person> GetAsync(int id)
    {
        return await CacheService.GetAsync<Person>("p:" + id, () => _innerRepository.GetAsync(id)).ConfigureAwait(false);
    }
}
```

## Configuring the project

This project uses the [AdventureWorks2012][AdventureWorks2012] database stored by
using Azure SQL Database. Create the database by using the Azure Management Portal
and add the connection string to the `AdventureWorksConnectionString` setting in the
ServiceConfiguration files for the AzureCloudService project.

Note that the new Azure portal provides a simplified version of the database
(AdventureWorksLT). The AdventureWorksLT database uses a different schema from that
expected by this sample application which might not function correctly unless the
full [AdventureWorks2012][AdventureWorks2012] database is installed.

This project also requires an instance of [Azure Redis Cache][AdventureWorks2012].
Create the cache by using the new Azure Portal and add the connection settings to the
`RedisConfiguration` setting in the ServiceConfiguration files

## Deploying the project to Azure

In Visual Studio Solution Explorer, right-click the AzureCloudService project and
then click *Publish* to deploy the project to Azure.

## Load testing the project

You can use [Visual Studio Online to load test](http://www.visualstudio.com/en-us/get-started/load-test-your-app-vs.aspx) the
application.
For details of the load testing strategy for this sample, see [Load Testing][Load Testing].

## Dependencies

This project requires:

* Azure SDK 2.5

* An instance of the [AdventureWorks2012] database

* An instance of [Azure Redis Cache][AzureRedisCache]

[docs]: docs/NoCaching.md
[AzureRedisCache]: http://azure.microsoft.com/services/cache/
[AdventureWorks2012]: https://msftdbprodsamples.codeplex.com/releases/view/37304
[Load Testing]: docs/LoadTesting.md
