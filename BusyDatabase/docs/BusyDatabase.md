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

As an example, the following code shows **TBD**


----------

**Note:** The `XYZ` controllers are included in the [sample code][fullDemonstrationOfProblem] available with this anti-pattern.

----------

## How to detect the problem

**TBD - COMPLETE THIS CONTENT**


Symptoms of a busy front end in an application include ...

You can perform the following steps to help identify this problem:

1. Identify ....

The following sections apply these steps to the sample application described earlier.

----------

**Note:** If you already have an insight into where problems might lie, you may be able to skip some of these steps. However, you should avoid making unfounded or biased assumptions. Performing a thorough analysis can sometimes lead to the identification of unexpected causes of performance problems. The following sections are formulated to help you examine applications and services systematically.

----------

### Monitoring

SPIEL

![Azure SQL Database monitor showing the performance of the database while performing processing][ProcessingInDatabaseMonitor]

### Performing load-testing

SPIEL

![Load-test results for performing processing in the database][ProcessingInDatabaseLoadTest]


## How to correct the problem

**TBD**

You should consider the following points:

- **TBD**. 


## Consequences of the solution

SPIEL

![Load-test results for performing processing in the database][ProcessingInClientApplicationLoadTest]

SPIEL

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

