# Busy Database

The primary purpose of a database is to act as a repository of information. Technically, a database is simply a collection of data files, but most modern database systems implement a server-based approach that abstracts the details of how these files are structured. A database server also handles aspects such as concurrency and locking to prevent data being corrupted by concurrent read and write operations, and managing security to control access to data.

As well as the logic associated with storing and fetching data in a controlled and safe manner, many database systems include the ability to run code within the server. Examples include stored procedures and triggers. The intent is that it is often more efficient to perform this processing close to the data rather than transmitting the data to a client application for processing. It is possible for a single data update operation to run a number of database triggers and stored procedures, which might in turn invoke further triggers and stored procedures as the results of a single change causes inserts, updates, and deletes to other tables. Consider cascading deletes in a SQL database as an example; removing a one row in one table might trigger updates and deletes of many other related rows in other tables.

However, you should use the increased functionality available with database servers with care to prevent a database server from becoming overloaded. A common occurrence is to perform data processing on the database server in the belief that this is the most efficient way of implementing these tasks. However, offloading processing to a database server can cause it to spend a significant proportion of the time running code rather than responding to requests to store and retrieve data. In turn, this can impact the performance of all client applications using the database.

This anti-pattern typically occurs because:

- The database is viewed as a service rather than a repository. As such, an application might expect the database server to manipulate and format data (such as converting to XML), or perform complex calculations rather than simply returning raw information.

- The client application uses data binding against the database and expects queries to return results that can be displayed directly. This might involve combining fields as they are retrieved from the database (for example, using SQL statements such as ` SELECT firstname + ' ' + middlename + ' ' + lastname FROM ...`), or formatting dates, times, currency, and numeric values according to locale.

----------

**Note:** This practice is not recommended as it closely couples the user interface with the database. Instead, retrieve the raw data into objects that act as view-models and bind the user interface to these view-models. For more information, see the [Model View ViewModel (MVVM)][MVVM] pattern.

----------

- It is viewed as a strategy to combat issues with network bandwidth as described by the [Retrieving Too Much Data][retrievingtoomuchdata] anti-pattern.

- An application uses stored procedures in a database to encapsulate business logic because they are easier to maintain and quicker to deploy than doing rolling updates to code in web applications. The process of updating a stored procedure is also less disruptive to end-users of an application (no reinstallation required).

- There is the perception that a database running on powerful hardware is more efficient at performing computations over data than a client application.

The [sample application][fullDemonstrationOfProblem] available with this anti-pattern illustrates a conceptual example that uses Azure SQL Database to perform a set of string manipulations on some test data by using Transact-SQL (the code replaces all occurrences of "ca" in the data retrieved with "cat", and then replaces all commas with spaces):

**Transact-SQL:**
```SQL
declare @Items varchar(8000);
declare @ItemList varchar(8000);
DECLARE @DelimIndex     INT;
DECLARE @Item   VARCHAR(8000);
SELECT @ItemList = ... /* Retrieve the data to be processed from a table in the database */
declare @Delimiter varchar(1);
SET @Delimiter = ',';
SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0);
SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex));
SET @Items = @Item;
WHILE (@DelimIndex != 0)
BEGIN
    SET @ItemList = SUBSTRING(@ItemList, @DelimIndex+1, LEN(@ItemList)-@DelimIndex)
	SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0)
	SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex))
	Set @Item =Replace('ca','ca','cat')
	SET @Items=CONCAT(@Items,' ',@Item)	
END; -- End WHILE
SET @Item = LTRIM(SUBSTRING(@ItemList, 0, @DelimIndex))
SET @Items=CONCAT(@Items,' ',@Item);	
select  @Items as FormattedList;
```

The code that invokes this Transact-SQL block is located in the `GetNameConcat` method in the `TooMuchProcSql` web API controller in the sample application.

## How to detect the problem

Symptoms of a busy database in an application include a disproportionate degradation in throughput and response time in business operations that access the database.  

You can perform the following steps to help identify this problem:

1. Use performance monitoring to identify how much time the system spends performing database activity.

2. Examine the work performed by the database occurring during these periods.

3. If the database activity reveals significant processing but little data traffic, review the source code and determine whether the processing can better be performed elsewhere.

The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you examine applications and services systematically.

----------

### Monitoring the volume of database activity

You can use a application performance monitor to track the database activity of the system in production. If the volume of database activity is low or response times are relatively fast, then a busy database is unlikely to be a performance problem.

If you suspect that particular operations might be cause undue database activity, then you can perform load-testing in a controlled environment. Each test should run a mixture of the suspect operations using a variable user-load to see how the system responds. You should also monitor the test system and examine the telemetry generated while the load test is in operation and observe how the database is used.

The following graph shows the results of performing a load-test against the sample application using a step-load of up to 1000 concurrent users. The volume of tests that the system can handle quickly reaches a limit and stays at that level, while the response time steadily increases (not that the scale measuring the number of tests and the response time is logarithmic):

![Load-test results for performing processing in the database][ProcessingInDatabaseLoadTest]

Examining the performance of the SQL Database by using the database monitor in the Azure Management console and capturing the CPU utilization together with the number of database throughput units (DTU) provides a measure of how much processing the database was performing. In the graph below, CPU and DTU utilization both reached 100% during the test (the test ran from 9:05 to 9:30, including some warm-up time)

![Azure SQL Database monitor showing the performance of the database while performing processing][ProcessingInDatabaseMonitor]

### Examining the work performed by the database

It could be that the tasks performed by the database are genuine data access operations, so it is important to understand the SQL statements being run while the database is busy. You can capture this information by using the monitoring information to correlate with the operational requests (user activity) being performed and then retrieve the low-level details of the SQL operations being made by these requests.

**NOTES AND IMAGE SHOWING SQL OPERATIONS FOR A WEB TRANSACTION CAPTURED BY USING NEW RELIC**

### Reviewing the source code

If you identify database operations that perform processing rather than data access operations, review the code that invokes these operations. It might be preferable to implement the processing as application code rather than offloading it to the database server. 

## How to correct the problem

Relocate processing from the database server to the client application where appropriate. This will involve refactoring the application code, and it may still be necessary to retrieve some information from the database to implement an operation. Ideally, you should limit the database to performing data access operations, and possibly to summarizing information where appropriate if the database server supports the necessary aggregation functions.

In the sample application, the Transact-SQL code can be replaced with the following statements that simply retrieve the data to be processed from the database:

**Transact-SQL:**
```SQL
declare @ItemList varchar(8000);
SELECT @ItemList = ... /* Retrieve the data to be processed from a table in the 
```

The processing is performed by the client application using the `Replace` method of the `string` class, as follows:

**C#**
```C#
using (SqlCommand command = new SqlCommand(commandString, connection))
{
    command.CommandType = CommandType.Text;
    using (SqlDataReader reader = await command.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            var field = await reader.GetFieldValueAsync<string>(0);       
            field=field.Replace("ca","cat").Replace(',',' ');
        }
    }
    ...
}
```


----------

**Note:** This code is available in the `LessProcSql` controller in the [solution code][fullDemonstrationOfSolution] provided with this anti-pattern.

----------

You should consider the following points:

- **TBD**. 


## Consequences of the solution

Relocating processing to the client application leaves the database free to focus on supporting data access operations. The following graph shows the results of repeating the load-test from earlier against the updated code Note that the throughput is significantly higher (nearly 10000 requests per second versus 800 earlier), and the response time is correspondingly much lower (below 0.1 seconds compared to 1 second for the previous test):

![Load-test results for performing processing in the database][ProcessingInClientApplicationLoadTest]

The database monitor in the Azure Management console shows the following CPU and DTU utilization. Notice that this time both figures only reached 40% despite the increased throughput and performance of the application:

![Azure SQL Database monitor showing the performance of the database while performing processing in the client application][ProcessingInClientApplicationMonitor]

## Related resources

- **REFS - TBD**

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123

[retrievingtoomuchdata]: http://LINK_TO_TOO_MUCH_DATA_ANTIPATTERN
[MVVM]: http://blogs.msdn.com/b/msgulfcommunity/archive/2013/03/13/understanding_2d00_the_2d00_basics_2d00_of_2d00_mvvm_2d00_design_2d00_pattern.aspx

[ProcessingInDatabaseLoadTest]: Figures/ProcessingInDatabaseLoadTest.jpg
[ProcessingInClientApplicationLoadTest]: Figures/ProcessingInClientApplicationLoadTest.jpg
[ProcessingInDatabaseMonitor]: Figures/ProcessingInDatabaseMonitor.jpg
[ProcessingInClientApplicationMonitor]: Figures/ProcessingInClientApplicationMonitor.jpg

