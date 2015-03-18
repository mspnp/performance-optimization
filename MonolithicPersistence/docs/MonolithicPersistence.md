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

The following code snippets show a pair of web API controllers that part of a web application. The `ProductDescription` controller provides a *Get* HTTP operation that retrieves the description of a product held in the [AdventureWorks2012][AdventureWorks2012] sample Azure SQL database. The `PurchaseOrderHeader` controller contains a *Post* HTTP operation creates new `PurchaseOrderHeader` records in the database:

**C# web API**
```C#
public class ProductDescriptionController : ApiController
{
    private string sqlDBConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");

    public async Task<string> GetAsync(int id)
    {
        string result = "";
        string queryString = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";
        using (SqlConnection cn = new SqlConnection(sqlDBConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand(queryString, cn))
            {
                cmd.Parameters.AddWithValue("@inputId", id);
                await cn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = reader.GetFieldValue<string>(0); ;
                    }
                }
            }
        }
        return result;
    }
}

...

public class PurchaseOrderHeaderController : ApiController
{
    private static string sqlDBConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
    public async Task<IHttpActionResult> PostAsync([FromBody]string value)
    {
        await InsertToPurchaseOrderHeaderTableAsync().ConfigureAwait(false);
        return Ok();
    }

    private static async Task InsertToPurchaseOrderHeaderTableAsync()
    {
        string queryString =
                "INSERT INTO Purchasing.PurchaseOrderHeader(" +
                " RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate)" +
                " VALUES(" +
                    "@RevisionNumber,@Status,@EmployeeID,@VendorID,@ShipMethodID,@OrderDate,@ShipDate,@SubTotal,@TaxAmt,@Freight,@ModifiedDate)";
        var dt = DateTime.UtcNow;
        using (SqlConnection cn = new SqlConnection(sqlDBConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand(queryString, cn))
            {
                cmd.Parameters.Add("@RevisionNumber", SqlDbType.TinyInt).Value = 1;
                cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = 4;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = 258;
                cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = 1580;
                cmd.Parameters.Add("@ShipMethodID", SqlDbType.Int).Value = 3;
                cmd.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = dt;
                cmd.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = dt;
                cmd.Parameters.Add("@SubTotal", SqlDbType.Money).Value = 123.40;
                cmd.Parameters.Add("@TaxAmt", SqlDbType.Money).Value = 12.34;
                cmd.Parameters.Add("@Freight", SqlDbType.Money).Value = 5.76;
                cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = dt;

                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
```

----------

**Note:** This code forms part of the [MonolithicPersistence sample application][fullDemonstrationOfProblem] available with this anti-pattern.

----------

These controllers are reasonable inasmuch as they reference the SQL database for storing and retrieving business data. However, the sample application also includes the following controller which is used for performing logging operations:

**C# web API**
```C#
public class SqldbLogController : ApiController
{
    private static string sqlDBConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnectionString");
    public async Task<IHttpActionResult> PostAsync([FromBody]string value)
    {
        LogMessage logMessage = new LogMessage();
        await LogToSqldbAsync(logMessage).ConfigureAwait(false);
        return Ok();
    }

    private static async Task LogToSqldbAsync(LogMessage logMessage)
    {
        string queryString = "INSERT INTO dbo.SqldbLog(Message, LogId, LogTime) VALUES(@Message, @LogId, @LogTime)";
        using (SqlConnection cn = new SqlConnection(sqlDBConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand(queryString, cn))
            {
                cmd.Parameters.Add("@LogId", SqlDbType.NChar, 32).Value = logMessage.LogId;
                cmd.Parameters.Add("@Message", SqlDbType.NText).Value = logMessage.Message;
                cmd.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = logMessage.LogTime;
                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
```

This controller stores log information in the `SqldbLog` table in a SQL database. If the application is configured to use the same database for holding business data and operational data (log information), then this database is being used for two quite different purposes simultaneously. The rate at which log records are generated is likely to impact the performance of the business operations that use the same database.

## How to detect the problem

Using a single data store for all operational and business data can result in the data store becoming a point of contention; a large number of very different requests could be competing for the same data storage resources. In the case of the sample application, as more and more users access the web application, the system slows down markedly and eventually fails as the system runs out of SQL Server connections and throttles applications attempting to read or write the database.

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

As an example, the following graph shows the load-test results for **FINISH THIS OFF WHEN LOAD TESTS ARE AVAILABLE***

![Load-test performance results for the SQL-based controller][]

### Identifying periods of poor performance

If you are monitoring the production system, you might notice patterns in the way in which the system performs. For example, response times might drop off significantly at the same time each day, or there could be a critical workload at which throughput suddenly plummets. You should focus on the telemetry data for these events.

**REWRITE THIS. Note that performance patterns might not be periodic. For example, the following graph shows a sudden deterioration in the throughput of `Post` operations in the controller although the user load was not significant. However, at this point there was a lot of log activity; the logs had grown to a large size and were being purged by an operator.**

![Monitoring charts from APM tool showing events described in previous paragraph][]

### Identifying data stores accessed during periods of poor performance

The instrumentation added to the system should provide an indication of which data stores are being accessed during the periods of poor performance. In the case of the sample application, the data indicated that poor performance coincided with two events:

1. A significant volume of users accessing the system simultaneously.

2. An operator performing log maintenance operations while the system was in use.

In both cases, the telemetry data indicated that the SQL database was the only data store being utilized.

![Chart from APM tool showing the low-level telemetry of the database][]

### Capturing low-level telemetry for data stores

**REWRITE THIS The data stores themselves should also be instrumented to capture the low-level details of the activity that occurs. In the sample application, under normal operations the data access statistics showed a mixture of read operations performed against the `ProductDescription` table in the AdventureWorks2012 database, a similar number of write operations performed against the `PurchaseOrderHeader` table, but a much large volume (approximately 5 times the number) of write requests sent to the `Traces` table. **

![Chart showing the data access stats from Azure SQL database][]

At the times when an operator was clearing the log, the volume of write I/O operations performed against the database greatly increased, to the point at which other requests were starved of access and began timing out.

![Chart showing write stats from Azure SQL database][]

### Examining contended resources

At this point you can conduct a review of the source code focussing on the points at which the contended resources are accessed by the application. While reviewing the code, look for situations such as:

- Data that is logically separate being written to the same store.

- Information being held in a data store that is not best suited for the operations being performed.

- Data which has significantly different usage patterns that share the same store, such as data that is written frequently but read relatively infrequently and vice versa.

- OTHERS?

## How to correct the problem

Data which has different usage patterns or that is logically distinct can be partitioned into separate data stores. The data storage mechanism selected should be appropriate to the pattern of use for each data set. Additionally, you may be able to partition a single data store and structure the data to avoid hot-spots that are subject to high-levels of contention, or replicate data that is read often but written infrequently to spread the load across data stores.

In the sample application, changing the logging mechanism to use an [Azure Event Hub][EventHub] helps to reduce pressure on the Azure SQL database. In the `EventhubLog` controller shown in the code below, the `Post` operation performs the same work that of the corresponding operation in the `SqldbLog` controller. Internally, the new controller writes data to an event hub rather than the SQL database:

**C# web API**
```C#
public class EventhubLogController : ApiController
{
    static string eventHubName = CloudConfigurationManager.GetSetting("EventHubName");
    static string eventHubNamespace = CloudConfigurationManager.GetSetting("EventHubNamespace");
    static string devicesSharedAccessPolicyName = CloudConfigurationManager.GetSetting("LogPolicyName");
    static string devicesSharedAccessPolicyKey = CloudConfigurationManager.GetSetting("LogPolicyKey");
    static string eventHubConnectionString = string.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2};TransportType=Amqp",
        eventHubNamespace, devicesSharedAccessPolicyName, devicesSharedAccessPolicyKey);
    static EventHubClient client = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);

    public async Task<IHttpActionResult> PostAsync([FromBody]string value)
    {
        LogMessage logMessage = new LogMessage();
        await LogToEventhubAsync(logMessage).ConfigureAwait(false);
        return Ok();
    }

    private static async Task LogToEventhubAsync(LogMessage logMessage)
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings();
        var serializedString = JsonConvert.SerializeObject(logMessage);
        var bytes = Encoding.UTF8.GetBytes(serializedString);
        using (EventData data = new EventData(bytes))
        {
            await client.SendAsync(data).ConfigureAwait(false);
        }
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

**REWRITE THIS For comparison purposes, the following graph shows the results of perform the same load-tests as before but running them against the `EventhubLog` controller.**

![Load-test performance results using the EventhubLog controller][]

NEED TO WRITE ABOUT WHAT THESE RESULTS SHOW!

## Related resources

- [Azure Table Storage and Windows Azure SQL Database - Compared and Contrasted][TableStorageVersusDatabase]

- [Data Access for Highly-Scalable Solutions: Using SQL, NoSQL, and Polyglot Persistence][Data-Access-Guide]

- [Azure Cache documentation][Azure-cache]

- [Azure Event Hub documentation][EventHub]


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[AdventureWorks2012]: http://msftdbprodsamples.codeplex.com/releases/view/37304
[SLAB]: https://msdn.microsoft.com/library/dn775006.aspx

[DocumentDB]: http://azure.microsoft.com/services/documentdb/
[Azure-cache]: http://azure.microsoft.com/documentation/services/cache/
[Data-Access-Guide]: https://msdn.microsoft.com/library/dn271399.aspx
[Azure-Table-Storage]: http://azure.microsoft.com/documentation/articles/storage-dotnet-how-to-use-tables/
[TableStorageVersusDatabase]: https://msdn.microsoft.com/library/azure/jj553018.aspx
[EventHub]: http://azure.microsoft.com/services/event-hubs/

