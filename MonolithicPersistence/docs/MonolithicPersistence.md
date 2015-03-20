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

The following code snippets show a pair of web API controllers that part of a web application. The `Monolithic` controller provides an HTTP POST operation that performs the following tasks:

- Retrieves the description of a product held in the [AdventureWorks2012][AdventureWorks2012] sample Azure SQL database,

- Creates a new `PurchaseOrderHeader` record in the same database, and

- Writes log records to the same database.

The details of the database operations are implemented by a set of static methods the `DataAccess` class. These methods use the ADO.NET API to interact with the database.

**C# web API**
```C#
public class MonolithicController : ApiController
{
    public async Task<IHttpActionResult> PostAsync([FromBody]int logCount)
    {
        for (int i = 0; i < logCount; i++)
        {
            var logMessage = new LogMessage();
            await DataAccess.LogToSqldbAsync(logMessage);
        }

        await DataAccess.SelectProductDescriptionAsync(321);
        await DataAccess.InsertToPurchaseOrderHeaderTableAsync();

        return Ok();
    }
}
```

----------

**Note:** This code forms part of the [MonolithicPersistence sample application][fullDemonstrationOfProblem] available with this anti-pattern.

----------

The application uses the same database for two distinctly different purposes; to store business data and to log operational data. The rate at which log records are generated is likely to impact the performance of the business operations. Additionally, if a third party utility (such as application process monitor) regularly reads and processes the log data, then the activities of this utility can also affect the performance of business operations.

## How to detect the problem

Using a single data store for all operational and business data can result in the data store becoming a point of contention; a large number of very different requests could be competing for the same data storage resources. If a single business operation performs one business transaction against the database but also records 5 log records (for example, *start operation*, *submit SQL request*, *retrieve response*, *process response*, *return results*) then the additional of a single user to the workload might increase the volume of database traffic by 6. In the sample application, as more and more users access the web application, the system will likely slow down markedly and eventually fail as the system runs out of SQL Server connections and throttles applications attempting to read or write the database.

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

**NOTE: WHAT WAS LOG_COUNT SET TO?**

![Load-test performance results for the SQL-based controller][LoadTestWithSQLLogging]

**START HERE - NOT SURE HOW EFFECTIVE THE CURRENT GRAPH IS.**

### Identifying periods of poor performance

If you are monitoring the production system, you might notice patterns in the way in which the system performs. For example, response times might drop off significantly at the same time each day, or there could be a critical workload at which throughput suddenly plummets. You should focus on the telemetry data for these events.

You should look for matches in increased response times and throughput against any likely causes, such as increased database activity or I/O to shared resources. If any such correlations exist, then the database or shared resource could be acting as a bottleneck.

![Monitoring charts from APM tool showing events described in previous paragraph][]

----------

 **Note:** The cause of performance deterioration could be an external event. In the example application, an operator purging the SQL log could generate a significant load on the database server and cause a slow-down in business operations even if the user-load was relatively low at the time.

----------

### Identifying data stores accessed during periods of poor performance

The instrumentation added to the system should provide an indication of which data stores are being accessed during the periods of poor performance. In the case of the sample application, the data indicated that poor performance coincided with a significant volume of users accessing the system simultaneously. In this time, the telemetry data indicated that the SQL database was the only data store being utilized.

![Chart from APM tool showing the low-level telemetry of the database][]

### Capturing low-level telemetry for data stores

The data stores themselves should also be instrumented to capture the low-level details of the activity that occurs. In the sample application, under normal operations the data access statistics showed a mixture of read operations performed against the `ProductDescription` table in the AdventureWorks2012 database, a similar number of write operations performed against the `PurchaseOrderHeader` table, but a much large volume (approximately 5 times the number) of write requests sent to the `Traces` table. **

![Chart showing the data access stats from Azure SQL database][]


### Examining contended resources

At this point you can conduct a review of the source code focussing on the points at which the contended resources are accessed by the application. While reviewing the code, look for situations such as:

- Data that is logically separate being written to the same store.

- Information being held in a data store that is not best suited for the operations being performed.

- Data which has significantly different usage patterns that share the same store, such as data that is written frequently but read relatively infrequently and vice versa.

- OTHERS?

## How to correct the problem

Data which has different usage patterns or that is logically distinct can be partitioned into separate data stores. The data storage mechanism selected should be appropriate to the pattern of use for each data set. Additionally, you may be able to partition a single data store and structure the data to avoid hot-spots that are subject to high-levels of contention, or replicate data that is read often but written infrequently to spread the load across data stores.

In the sample application, changing the logging mechanism to use an [Azure Event Hub][EventHub] helps to reduce pressure on the Azure SQL database. The code below uses the static `LogToEventHubAsync` method of the `DataAccess` class to write data to an event hub rather than the SQL database:

**C# web API**
```C#
public class PolyglotController : ApiController
{
    public async Task<IHttpActionResult> PostAsync([FromBody]int logCount)
    {
        for (int i = 0; i < logCount; i++)
        {
            var logMessage = new LogMessage();
            await DataAccess.LogToEventhubAsync(logMessage);
        }

        await DataAccess.SelectProductDescriptionAsync(321);
        await DataAccess.InsertToPurchaseOrderHeaderTableAsync();

        return Ok();
    }
}
```

----------

**Note:** This snippet is taken from the [sample code][fullDemonstrationOfSolution] available with this anti-pattern.

----------

You should consider the following points when determining the most appropriate scheme for storing business and operational data:

- **MORE POINTS GO HERE?**

- Partition data by the way in which it is used and where it is accessed. For example, don't store log information and audit records in the same data store. Although both types of data are written sequentially, they have significantly different requirements (log records are dispensable and should not contain confidential information whereas audit records must be permanent and may contain sensitive data).

- Use a storage technology that is most appropriate to the data access pattern. For example, store formatted reports and documents in a document database such as [DocumentDB][DocumentDB], use a specialized solution such as [Azure Redis Cache][Azure-cache] for caching temporary data, or use [Azure Table Storage][Azure-Table-Storage] for holding information written and accessed sequentially such as log files.

## Consequences of the solution

Spreading the load across data stores reduces contention and helps to improve overall the performance of the system under load. You could also take the opportunity to assess the data storage technologies used and rework selected parts of the system to use a more appropriate storage mechanism, although making changes such as this may involve thorough retesting of the system functionality.

For comparison purposes, the following graph shows the results of performing the same load-tests as before but logging records to the event hub.

![Load-test performance results using the EventhubLog controller][LoadTestWithEventHubLogging]

**NEED TO WRITE ABOUT WHAT THESE RESULTS SHOW!**

## Related resources

- [Azure Table Storage and Windows Azure SQL Database - Compared and Contrasted][TableStorageVersusDatabase]

- [Data Access for Highly-Scalable Solutions: Using SQL, NoSQL, and Polyglot Persistence][Data-Access-Guide]

- [Azure Cache documentation][Azure-cache]

- [Azure Event Hub documentation][EventHub]


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[AdventureWorks2012]: http://msftdbprodsamples.codeplex.com/releases/view/37304

[DocumentDB]: http://azure.microsoft.com/services/documentdb/
[Azure-cache]: http://azure.microsoft.com/documentation/services/cache/
[Data-Access-Guide]: https://msdn.microsoft.com/library/dn271399.aspx
[Azure-Table-Storage]: http://azure.microsoft.com/documentation/articles/storage-dotnet-how-to-use-tables/
[TableStorageVersusDatabase]: https://msdn.microsoft.com/library/azure/jj553018.aspx
[EventHub]: http://azure.microsoft.com/services/event-hubs/
[LoadTestWithSQLLogging]: Figures/LoadTestWithSQLLogging.jpg
[LoadTestWithEventHubLogging]: Figures/LoadTestWithEventHubLogging.jpg

