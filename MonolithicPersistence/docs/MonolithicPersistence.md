# Monolithic Persistence

All business applications use data, and they need to store this data somewhere. Many existing business applications make use of a single repository for holding the bulk of the information required. Modern cloud-based systems sometimes follow this same approach and attempt to store information either in a single repository or a small set of tightly-coupled data stores. This strategy is typically aimed at keeping the data storage requirements simple by using well-understood technology, and might appear to make sense initially. A distributed application often has additional functional and non-functional requirements though, and besides the raw business information an application might also need to store:

- Formatted reporting data,

- Documents,

- Images and blobs, 

- Cached information and other temporary data used to improve system performance,

- Queued messages,

- Application log and audit data.

The temptation might be to try and record all of this information in the same repository, but this approach might not be appropriate for the following reasons:

- It can cause severe contention in the repository, leading to slow response times or data store connection failures.

- The data store might not be optimized to match the requirements of the structure of every piece data or the operations that the application performs on this data. For example, queuing messages requires fast first-in first-out capability whereas business data typically requires a data store that is better tuned to supporting random access capabilities.

----------

**Note:** For historical reasons the single repository selected is often a SQL database as this is the form of data storage that is best understood by many designers. However, the principles of this anti-pattern apply regardless of the type of repository.

----------

The following code snippet shows a web API controller that simulates the actions performed as part of a web application. The `Monolithic` controller provides an HTTP POST operation that adds a new record to a database and also records the result to a log. The log is held in the same database as the business data. The details of the database operations are implemented by a set of static methods the `DataAccess` class:

**C# web API**
```C#
public class MonoController : ApiController
{
    private static readonly string ProductionDb = ...;
    public const string LogTableName = "MonoLog";

    public async Task<IHttpActionResult> PostAsync([FromBody]string value)
    {
        await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

        await DataAccess.LogAsync(ProductionDb, LogTableName);

        return Ok();
    }
}
```

----------

**Note:** This code forms part of the [MonolithicPersistence sample application][fullDemonstrationOfProblem] available with this anti-pattern.

----------

The application uses the same database for two distinctly different purposes; to store business data and to record telemetry information. The rate at which log records are generated is likely to impact the performance of the business operations. Additionally, if a third party utility (such as application process monitor) regularly reads and processes the log data, then the activities of this utility can also affect the performance of business operations.

## How to detect the problem

Using a single data store for telemetry and business data can result in the data store becoming a point of contention; a large number of very different requests could be competing for the same data storage resources. In the sample application, as more and more users access the web application, the system will likely slow down markedly and eventually fail as the system runs out of SQL Server connections and throttles applications attempting to read or write the database.

You can perform the following steps to help identify the causes of any problems resulting from data store contention:

1. Instrument the system and performing process monitoring under everyday conditions.

2. Use the telemetry data to identify periods of poor performance.

3. Identify the use of the data stores that are accessed during periods of poor performance.

4. Capture the low-level telemetry data for these stores at these times.

5. Identify contended data storage resources.

6. For each contended resource, examine how it is used.


The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you to examine applications and services systematically.

----------

### Instrumenting and monitoring the system

This step is a matter of configuring the system to record the key statistics required to capture the performance signature of the application. You should capture timing information for each operation as well as the points at which the application reads and writes data. If possible, monitor the system running for a few days in a production environment and capture the telemetry of obtain a real-world view of how the system is used. If this is not possible, then perform scripted load-testing using a realistic volume of virtual users performing a typical series of operations.

As an example, the following graph shows the load-test results for a scenario in which a step load of up to 1000 concurrent users issue HTTP POST requests to the `Monolithic` controller. 

![Load-test performance results for the SQL-based controller][MonolithicScenarioLoadTest]

As the load increases to 700 users so does the throughput. At the 700-user point throughput levels off and the system appears to be running at its maximum capacity. The average response gradually increases with user load as the system is unable to keep up with demand.

### Identifying periods of poor performance

If you are monitoring the production system, you might notice patterns in the way in which the system performs. For example, response times might drop off significantly at the same time each day. This could be caused by a regular workload or background job that is scheduled to run at this time, or it could be due to the behavioral factors of the users (are users more likely to access the system at specific times?) You should focus on the telemetry data for these events.

You should look for matches in increased response times and throughput against any likely causes, such as increased database activity or I/O to shared resources. If any such correlations exist, then the database or shared resource could be acting as a bottleneck.

----------

**Note:** The cause of performance deterioration could be an external event. In the example application, an operator purging the SQL log could generate a significant load on the database server and cause a slow-down in business operations even if the user-load is relatively low at the time.

----------

### Identifying data stores accessed during periods of poor performance

Monitoring the the data stores used by system should provide an indication of how they are being accessed during the periods of poor performance. In the case of the sample application, the data indicated that poor performance coincided with a significant volume of requests to the SQL database holding the business and log data. As an example, the Monitor pane in the Azure Management Portal for the database used by the sample application showed that during load-testing the Database Throughput Unit (DTU) utilization reached 100% shortly after 5 minutes. This roughly equates with the point at which the throughput shown in the previous graph plateaued:

![The database monitor in the Azure Management Portal showing resource utilization of the database][MonolithicDatabaseUtilization]

A DTU is a measure of the available capacity of the system and is a combination of the CPU utilization, memory allocation, and the rate of read and write operations being performed. Each SQL database server allocates a quota of resources to applications measured in DTUs. The volume of DTUs available to an application depend on the service tier and performance level of the database server; creating an Azure SQL database using the Basic service tier provides 5 DTUs, while a database using the Premium Service Tier and P3 Performance Level has 800 DTUs available. When an application reaches the limit defined by the available DTUs database performance is throttled. At this point, throughput levels off but response time is likely to increase as database requests are queued. This is what happened during the load-test.

### Capturing low-level telemetry for data stores

The data stores themselves should also be instrumented to capture the low-level details of the activity that occurs. In the sample application, during the load-test the data access statistics showed a high volume of insert operations performed against the `PurchaseOrderHeader` table and the `MonoLog` table in the AdventureWorks2012 database:

![The data access statistics for the sample application][MonolithicDataAccessStats]

----------

**Note:** There are several entries for statements that insert data into the `MonoLog` table because the database server has generated different query plans at different times during the load-test based on the size of the table and other environmental factors.

----------

### Examining contended resources

At this point you can conduct a review of the source code focussing on the points at which the contended resources are accessed by the application. While reviewing the code, look for situations such as:

- Data that is logically separate being written to the same store.

- Information being held in a data store that is not best suited for the operations being performed.

- Data which has significantly different usage patterns that share the same store, such as data that is written frequently but read relatively infrequently and vice versa.

## How to correct the problem

Data which has different usage patterns or that is logically distinct can be partitioned into separate data stores. The data storage mechanism selected should be appropriate to the pattern of use for each data set. Additionally, you may be able to partition a single data store and structure the data to avoid hot-spots that are subject to high-levels of contention, or replicate data that is read often but written infrequently to spread the load across data stores.

As an example, the code below is very similar to the `Monolithic` controller except that the log records are written to a different database running on a separate server. This approach helps to reduce pressure on the database holding the business information:

**C# web API**
```C#
public class PolyController : ApiController
{
    private static readonly string ProductionDb = ...;
    private static readonly string LogDb = ...;
    public const string LogTableName = "PolyLog";

    public async Task<IHttpActionResult> PostAsync([FromBody]string value)
    {
        await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

        await DataAccess.LogAsync(LogDb, LogTableName);

        return Ok();
    }
}

```

----------

**Note:** This snippet is taken from the [sample code][fullDemonstrationOfSolution] available with this anti-pattern.

----------

You should consider the following points when determining the most appropriate scheme for storing business and operational data:

- Partition data by the way in which it is used and where it is accessed. For example, don't store log information and audit records in the same data store. Although both types of data are written sequentially, they have significantly different requirements (log records are dispensable and should not contain confidential information whereas audit records must be permanent and may contain sensitive data).

- Use a storage technology that is most appropriate to the data access pattern for each type of data item. For example, store formatted reports and documents in a document database such as [DocumentDB][DocumentDB], use a specialized solution such as [Azure Redis Cache][Azure-cache] for caching temporary data, or use [Azure Table Storage][Azure-Table-Storage] for holding information written and accessed sequentially such as log files.

## Consequences of the solution

Spreading the load across data stores reduces contention and helps to improve overall the performance of the system under load. You could also take the opportunity to assess the data storage technologies used and rework selected parts of the system to use a more appropriate storage mechanism, although making changes such as this may involve thorough retesting of the system functionality.

For comparison purposes, the following graph shows the results of performing the same load-tests as before but logging records to the separate database.

![Load-test performance results using the Polyglot controller][PolyglotScenarioLoadTest]

The pattern of the throughput is similar to that of the earlier graph, but the volume of requests supported when the performance levels out is approximately 500 requests per second higher. The response time is also marginally lower. However, these statistics do not tell the full story. Examining the utilization of the business database by using the Azure Management Portal reveals that DTU utilization now peaks at 67.85%:

![The database monitor in the Azure Management Portal showing resource utilization of the database in the polyglot scenarion][PolyglotDatabaseUtilization]

Similarly, the DTU utilization of the log database only reaches 66.7%:

![The database monitor in the Azure Management Portal showing resource utilization of the log database in the polyglot scenarion][LogDatabaseUtilization]

The databases are now no longer the limiting factor in the performance of the system, and the throughput might now be restricted by other factors such as web server capacity.

----------

**Note:** If you are still hitting the DTU limits for an Azure SQL database server then you may need to scale up to a higher Service Tier or Performance Level. Currently the Premium/P3 level is the highest level available, supporting up to 800 DTUs. If you anticipate exceeding this throughput, then you should consider scaling horizontally and partitioning the load across database servers.

----------

## Related resources

- [Azure Table Storage and Windows Azure SQL Database - Compared and Contrasted][TableStorageVersusDatabase]

- [Data Access for Highly-Scalable Solutions: Using SQL, NoSQL, and Polyglot Persistence][Data-Access-Guide]

- [Azure Cache documentation][Azure-cache]

- [Data Partitioning Guidance][DataPartitioningGuidance]


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[AdventureWorks2012]: http://msftdbprodsamples.codeplex.com/releases/view/37304
[DocumentDB]: http://azure.microsoft.com/services/documentdb/
[Azure-cache]: http://azure.microsoft.com/documentation/services/cache/
[Data-Access-Guide]: https://msdn.microsoft.com/library/dn271399.aspx
[Azure-Table-Storage]: http://azure.microsoft.com/documentation/articles/storage-dotnet-how-to-use-tables/
[DataPartitioningGuidance]: https://msdn.microsoft.com/library/dn589795.aspx
[TableStorageVersusDatabase]: https://msdn.microsoft.com/library/azure/jj553018.aspx
[MonolithicScenarioLoadTest]: Figures/MonolithicScenarioLoadTest.jpg
[MonolithicDatabaseUtilization]: Figures/MonolithicDatabaseUtilization.jpg
[MonolithicDataAccessStats]: Figures/MonolithicDataAccessStats.jpg
[PolyglotScenarioLoadTest]: Figures/PolyglotScenarioLoadTest.jpg
[PolyglotDatabaseUtilization]: Figures/PolyglotDatabaseUtilization.jpg
[LogDatabaseUtilization]: Figures/LogDatabaseUtilization.jpg

