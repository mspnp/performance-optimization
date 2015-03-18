# Monoglot Persistence

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

The following code snippets show part of a web application. The `Get` operation retrieves the description of a product held in the [AdventureWorks2012][AdventureWorks2012] sample Azure SQL database. The `Post` operation creates new `PurchaseOrderHeader` records in the database. Both methods are instrumented by using the [Semantic Logging Application Block][SLAB] to capture database timing information as each request runs. This data is saved by using the `MonoglotEventSource` type. The `MonoglotEventSource` sends data to the same Azure SQL database (AdventureWorks2012) used by the `Get` method:

**C#**
```C#
public class MonoglotController : ApiController
{
    private string sqlServerConnectionString = ConfigurationManager.ConnectionStrings["sqlServerConnectionString"].ConnectionString;

    public string Get(int id)
    {
        string result = "";
        try
        {
            // Log the start of the operation
            MonoglotEventSource.Log.Startup();
            MonoglotEventSource.Log.PageStart(id, this.Url.Request.RequestUri.AbsoluteUri.ToString());
            
            string queryString = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";
            using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@inputId", id);

                    // Instrumentation - log the time at which the database read commences
                    MonoglotEventSource.Log.ReadDataStart();
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    cn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = String.Format("Description = {0}", reader[0]);
                    }
                    reader.Close();

                    // Log the time at which the database read completes
                    watch.Stop();
                    MonoglotEventSource.Log.ReadDataFinish(watch.ElapsedMilliseconds);
                }
            }
            // Log the end of the operation
            MonoglotEventSource.Log.PageEnd();
        }
        catch (Exception ex)
        {
            ...
        }
        return result;
    }

    public void Post([FromBody]string value)
    {
        try
        {
            MonoglotEventSource.Log.Startup();
            MonoglotEventSource.Log.PageStart(1, this.Url.Request.RequestUri.AbsoluteUri.ToString());

            string queryString =
                "INSERT INTO Purchasing.PurchaseOrderHeader(" +
                " RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate)" +
                " VALUES(" +
                "@RevisionNumber,@Status,@EmployeeID,@VendorID,@ShipMethodID,@OrderDate,@ShipDate,@SubTotal,@TaxAmt,@Freight,@ModifiedDate)";
            var dt = DateTime.Now;
            using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(queryString, cn))
                {
                    MonoglotEventSource.Log.WriteDataStart();

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

                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    watch.Stop();
                    MonoglotEventSource.Log.WriteDataFinish(watch.ElapsedMilliseconds);
                }
            }

            MonoglotEventSource.Log.PageEnd();
        }
        catch (Exception ex)
        {
            ...
        }
    }
...
}

...

// Class that initializes the event sources used by logging
// Invoked when the web application starts up
public class Logging
{
    private static ObservableEventListener listener0;
    ...
    private static SinkSubscription<SqlDatabaseSink> subscription0;
    ...

    public static void Start()
    {
        // Configure the Monoglot Event Source to send data to the SQL Server database
        string sqlServerConnectionString = ConfigurationManager.ConnectionStrings["sqlServerConnectionString"].ConnectionString;
        listener0 = new ObservableEventListener();
        listener0.EnableEvents(MonoglotEventSource.Log, EventLevel.Informational);
        subscription0 = listener0.LogToSqlDatabase("Monolithic Anti Pattern", sqlServerConnectionString);
        ...
    }
}
```

----------

**Note:** This code forms part of the [CachingDemo sample application][fullDemonstrationOfProblem].

----------

The AdventureWorks2012 database has to handle all data retrieval and storage operations, whether these are business requests (fetching product details) or operational requests (storing log records). The same database is being used for two quite different purposes, and the rate at which log records are generated is likely to impact the performance of business operations that require access to information held in the same database.

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

As an example, the following graph shows the load-test results for the sample application. Users ran a mixture of the `Post` and `Get` operations in the `Monoglot` controller. The workload was increased over time. As the workload hits 100 users, response time increases significantly and throughput drops off:

![Load-test performance results for the Monoglot controller][]

### Identifying periods of poor performance

If you are monitoring the production system, you might notice patterns in the way in which the system performs. For example, response times might drop off significantly at the same time each day, or there could be a critical workload at which throughput suddenly plummets. You should focus on the telemetry data for these events.

Note that performance patterns might not be periodic. For example, the following graph shows a sudden deterioration in the throughput of `Post` operations in the `Monoglot` controller although the user load was not significant. However, at this point there was a lot of log activity; the logs had grown to a large size and were being purged by an operator.

![Monitoring charts from APM tool showing events described in previous paragraph][]

### Identifying data stores accessed during periods of poor performance

The instrumentation added to the system should provide an indication of which data stores are being accessed during the periods of poor performance. In the case of the sample application, the data indicated that poor performance coincided with two events:

1. A significant volume of users accessing the system simultaneously.

2. An operator performing log maintenance operations while the system was in use.

In both cases, the telemetry data indicated that the Azure SQL database was the only data store being utilized.

![Chart from APM tool showing the low-level telemetry of the database][]

### Capturing low-level telemetry for data stores

The data stores themselves should also be instrumented to capture the low-level details of the activity that occurs. In the sample application, under normal operations the data access statistics showed a mixture of read operations performed against the `ProductDescription` table in the AdventureWorks2012 database, a similar number of write operations performed against the `PurchaseOrderHeader` table, but a much large volume (approximately 5 times the number) of write requests sent to the `Traces` table.

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

In the sample application, changing the logging mechanism to use Azure table storage helps to reduce pressure on the Azure SQL database. In the `Polyglot` controller shown in the code below, the `Get` and `Post` operations perform the same work as before except that they use the `PolyglotEventSource` type to record log data. This event source stores information in Azure table storage:

**C#**
```C#
public class PolyglotController : ApiController
{
    ...

    public string Get(int id)
    {
        string result = "";
        try
        {
            PolyglotEventSource.Log.Startup();
            PolyglotEventSource.Log.PageStart(id, this.Url.Request.RequestUri.AbsoluteUri.ToString());
            ...
            using (SqlConnection cn = ...)
            {
                ...
                PolyglotEventSource.Log.ReadDataStart();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                ...
                watch.Stop();
                PolyglotEventSource.Log.ReadDataFinish(watch.ElapsedMilliseconds);
            }
            
            PolyglotEventSource.Log.PageEnd();
        }
        catch (Exception ex)
        {
            ...
        }
        return result;
    }

    public void Post([FromBody]string value)
    {
        try
        {
            PolyglotEventSource.Log.Startup();
            PolyglotEventSource.Log.PageStart(1, this.Url.Request.RequestUri.AbsoluteUri.ToString());
            ...
            PolyglotEventSource.Log.PageEnd();
        }
        catch (Exception ex)
        {
            ...
        }
    }
    ...
}

...

// Class that initializes the event sources used by logging
// Invoked when the web application starts up
public class Logging
{
    ...
    private static ObservableEventListener listener1;
    ...
    private static SinkSubscription<WindowsAzureTableSink> subscription1;
    ...

    public static void Start()
    {
        ...
        // Configure the Polyglot Event Source to send data to Azure table storage
        string azureStorageConnectionString1 = ConfigurationManager.ConnectionStrings["azureStorageConnectionString1"].ConnectionString;
        listener1 = new ObservableEventListener();
        listener1.EnableEvents(PoliglotEventSource.Log, EventLevel.Informational);
        subscription1 = listener1.LogToWindowsAzureTable("Monolithic Anti Pattern", azureStorageConnectionString1);
        ...
    }
}
```

----------

**Note:** This snippet is taken from the [sample code][fullDemonstrationOfSolution] available with this anti-pattern.

----------

You should consider the following points when determining the most appropriate scheme for storing business and operational data:

- MORE POINTS GO HERE

- Partition data by the way in which it is used and where it is accessed. For example, don't store log information and audit records in the same data store. Although both types of data are written sequentially, they have significantly different requirements (log records are dispensable and should not contain confidential information whereas audit records must be permanent and may contain sensitive data).

- Use a storage technology that is most appropriate to the data access pattern. For example, store formatted reports and documents in a document database such as [DocumentDB][DocumentDB], use a specialized solution such as [Azure Redis Cache][Azure-cache] for caching temporary data, or use [Azure Table Storage][Azure-Table-Storage] for holding information written and accessed sequentially such as log files.

## Consequences of the solution

Spreading the load across data stores reduces contention and helps to improve overall the performance of the system under load. You could also take the opportunity to assess the data storage technologies used and rework selected parts of the system to use a more appropriate storage mechanism, although making changes such as this may involve thorough retesting of the system functionality.

For comparison purposes, the following graph shows the results of perform the same load-tests as before but running them against the `Polyglot` controller.

![Load-test performance results for the Polyglot controller][]

NEED TO WRITE ABOUT WHAT THESE RESULTS SHOW!

## Related resources

- [Azure Table Storage and Windows Azure SQL Database - Compared and Contrasted][TableStorageVersusDatabase]

- [Data Access for Highly-Scalable Solutions: Using SQL, NoSQL, and Polyglot Persistence][Data-Access-Guide]

- [Azure Cache documentation][Azure-cache].


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[AdventureWorks2012]: http://msftdbprodsamples.codeplex.com/releases/view/37304
[SLAB]: https://msdn.microsoft.com/library/dn775006.aspx

[DocumentDB]: http://azure.microsoft.com/services/documentdb/
[Azure-cache]: http://azure.microsoft.com/documentation/services/cache/
[Data-Access-Guide]: https://msdn.microsoft.com/library/dn271399.aspx
[Azure-Table-Storage]: http://azure.microsoft.com/documentation/articles/storage-dotnet-how-to-use-tables/
[TableStorageVersusDatabase]: https://msdn.microsoft.com/library/azure/jj553018.aspx

