# Build and Run Monolithic Anti Pattern Code Sample

## Step 1: Create Azure Storage Accounts and update the connections string to the MonoglotPersistence solution

- Create two Azure storage accounts.

- Start Visual Studio and open MonoglotPersistence.sln

- Open Web.config file

- Update the connectionString for azureStorageConnectionString1

- Update the connectionString for azureStorageConnectionString2

## Step 2: Set up AdventureWorks DB and update the connections sting to the MonoglotPersistence solution

- Install the AdventureWorks Database on Azure DB, Add the IP Rules so that you can access the DB from your dev box. Make sure that the SQLDB version = Standard, level = S1, and storage = 50 GB

- Start Visual Studio and open MonoglotPersistence.sln

- Open Web.config file

- Update the connectionString for sqlServerConnectionString

## Step 3: Create Traces Table for SLAB in AdventureWorks DB

- Start Visual Studio and open MonoglotPersistence.sln

- Rebuild the solution

- Click View-> Sql Server Object Explorer

- In **SQL Server Object Explorer**, right click on SQL Server and click on **Add SQL Server**

- In the popup screen enter the Server name and credentials for your AdventureWorks DB server and Connect to it.

- In the tree view, expand your SQL server to AdventureWorks2012 database and start a new query

- Start windows explorer and browser to \MonoglotPersistence\packages\EnterpriseLibrary.SemanticLogging.Database.2.0.1406.1\scripts

- Open CreateSemanticLoggingDatabaseObjects.sql and copy its content to the Visual SQL Query Window and Execute the query

- Verify that dbo.Traces table is created.

## Step 4: Test the solution locally

- Start Visual Studio and open MonoglotPersistence.sln

- Press F5 to start debugging session, wait for IE to open.

### Test HTTPGet http://localhost:61912/api/monoglot/321

This request will call MonoglotController Method Get(id) and execute the following SQL querey:

**SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=321**

The Query Result for Description will be returned.

Here are the steps:

- Browse to http://localhost:61912/api/monoglot/321 (Note: port number may be different for you)

- You should get a popup windows:

 *Do you want to open or save 321.json (109 bytes) from localhost?*

- Click on **Open**

- This should open 321.json in Notepad with the following content:

"Description = Same technology as all of our Road series bikes.  Perfect all-around bike for road or racing."

- close the Notepad.

### Test HTTPGet http://localhost:61912/api/poliglot/321

This request will call PoliglotController Method Get(id) and execute the following SQL querey:

**SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=321**

The Query Result for Description will be returned.

Here are the steps:

- browse to http://localhost:61912/api/poliglot/321 (Note: port number may be different for you)

- THE rest steps are the same as that for Monoglot.

### Test HTTPPost http://localhost:61912/api/monoglot (Note: port number may be different for you)

This request will call MonoglotController Method Post(value) and execute the following SQL querey:

**INSERT INTO Purchasing.PurchaseOrderHeader**
**(RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate)**
**VALUES(1,4,258,1580,3,2015-03-05 14:23:04.743,2015-03-05 14:23:04.743,123.40,12.34,5.76,2015-03-05 14:23:04.743)**

(Note: Date values is obtained in runtime and will be different from the above)

The No Result is returned.

Here are the steps:

- Start Fiddler

- Click **Composer**

- change http verbs to **POST**

- change the url to **http://localhost:61912/api/mologlot**

- Click  **Execute**

- Verify the response is  **HTTP/1.1 204 No Content**

- Verify that the record is inserted to the database:

- In Sql Server Object Explorer tree view, expand to AdventureWorks2012 database and start a new query and enter the following in the query window

**SELECT Count(*) FROM Purchasing.PurchaseOrderHeader**

- make a note of the result for the count.

- In Fiddler, Execute the POST again

- In SQL query, run the query again. Verify the result count is increased by 1.


### Test HTTPPost http://localhost:61912/api/poliglot

- follow the same procedure as the previous step using **Poliglot** instead of **Monoglot**.

### Verify that the SLAB Logging is working properly for Monoglot Controller:

Monoglot Controller is logging to the SQLDB traces table.

- Run SQL querey against AdventureWorks2012 Database

**SELECT Count(*) FROM dbo.Traces**

- Make a note of the count

- Test HTTPGet http://localhost:61912/api/monoglot/321

- run SQL query again and verify that the count is increased by 5. (the HTTPGet method is add 5 log entries)

- Test HTTPPost http://localhost:61912/api/monoglot

- run SQL query again and verify that the count is increased by 5. (the HTTPPost method is add 5 log entries)

### Verify that the SLAB Logging is working properly for Poliglot Controller:

Poliglot Controller is logging to the Azure Table.

- In Visual Studio, Open Server Explorer, the Expand Azure -> Storage and expand to your storage account associated with azureStorageConnectionString1

- Make a note of the count of entities in the SLABLogsTable

- Test HTTPGet http://localhost:61912/api/poliglot/321

- refresh the entities count in the SLABLogsTable and verify that the count is increased by 5. (the HTTPGet method is add 5 log entries)

- Test HTTPPost http://localhost:61912/api/poliglot

- refresh the entities count in the SLABLogsTable and verify that the count is increased by 5. (the HTTPGet method is add 5 log entries)


## Step 5: Publish the AzureCloudService to Microsoft Azure

- Start Visual Studio and open MonoglotPersistence.sln

- Right click on **AzureCloudService** and click **Publish...**.  The default setting for the sample is 5 web role instances, medium.

- Wait for the publish to complete

## Step 6: Test the the Published Cloud Service

You may follow the similar steps as that in Step4.

## Step 7: Monoglot BaseLine Load Test:

- Create a web test that consists of two requests: a http get request and a web services request (httppost):

**http://mycloudservice.cloudapp.net/api/Monoglot/321**

**http://mycloudservice.cloudapp.net/api/Monoglot**

- run web test and make sure that it passes.

- create a load test that include the above web test, with 50 users, and set the run time to 10 minutes.

Setting           | Value
------------------| -----------
run time	        | 10
Warm-up Duration  |	2
webrole instances | 5

- run the following sql to reset the table:

  **DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012**

- start the load test.

- Here is sample results of a load test:

Count             | Value
------------------| -----------
User load         |	50
Requests/Sec	    | 268
Errors/Sec	      | 0.00
Avg Response Time	| 0.17
Max Response Time	| 0.49
Total Record Count| 94277
Error Count	      | 0

The following 4 count are the result of the load test.

1. Requests/Sec
1. Errors/Sec
1. Avg Response Time
1. Max Response Time

**Total Record Count** is obtained by running sql query after the load test:

**SELECT Count(*) FROM Purchasing.PurchaseOrderHeader**

**Error Count** is obtained by query Azure table in the second storage account associated with azureStorageConnectionString2.

## Step 8: Run Monoglot BaseLine Load Test while deleting the log entry in dbo.Traces table

- run the following query to find out the total number of trace entries:

**Select count(*) from dbo.traces**

- run the base line load test a few times until traces count is bigger then 2 million.

- Start the base line load test again.
don't forget to reset the PurchaseOrderHeader table by running sql query  **DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012**

- Here is sample results of a load test:

Setting/Count     | base line| deleting log
------------------| ---------| ------------
User load         |	50       | 50
Requests/Sec	    | 268      | 91
Errors/Sec	      | 0.00     | 0.00
Avg Response Time	| 0.17     | 0.54
Max Response Time	| 0.49     | 0.68
Total Record Count| 94277    |35950
Error Count	      | 0        |0

You can see the Request/Sec is much lower than the base, and the response time increase significantly.

## Step 9: Double the load on Monoglot BaseLine Load Test

Increase the user load from 50 to 100
- Start the  load test again.
don't forget to reset the PurchaseOrderHeader table by running sql query  **DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012**

Setting/Count     | base line| mono double
------------------| ---------| ------------
User load         |	50       | 100
Requests/Sec	    | 268      | 327
Errors/Sec	      | 0.00     | 6.68
Avg Response Time	| 0.17     | 0.29
Max Response Time	| 0.49     | 0.92
Total Record Count| 94,277   | 114,817
Error Count	      | 0        | 4591

You can see that when doubling the load, the throughput (Requests/Sec) only increase slightly, but latency (response) is increased greatly and we are starting seeing errors. checking the error message we found that SQLDB is throttling which causes some insert operation to fail.

## Step 10: Poliglot Load Test

- Create a web test that consists of two requests: a http get request and a web services request (httppost):

**http://mycloudservice.cloudapp.net/api/Poliglot/321**

**http://mycloudservice.cloudapp.net/api/Poliglot**

- run web test and make sure that it passes.

- create a load test that include the above web test, with 50 users, and set the run time to 10 minutes.

Setting           | Value
------------------| -----------
run time	        | 10
Warm-up Duration  |	2
webrole instances | 5

- run the following sql to reset the table:

  **DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012**

- start the load test.

- here is the test result comparing to the monoglot base line

Setting/Count     | base line| poliglot
------------------| ---------| ---------
User load         |	50       | 50
Requests/Sec	    | 268      | 376
Errors/Sec	      | 0.00     | 0.00
Avg Response Time	| 0.17     | 0.12
Max Response Time	| 0.49     | 0.19
Total Record Count| 94,277   | 130,553  
Error Count	      | 0        | 0

You can see that poliglot performnce is much better.

## Step 11: Double the load on Poliglot Load Test

Increase the user load from 50 to 100 for poliglot load test
- Start the  load test again.
don't forget to reset the PurchaseOrderHeader table by running sql query  **DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012**

Setting/Count     | base line| poliglot | poli double
------------------| ---------| ---------|------------
User load         |	50       | 50       | 100
Requests/Sec	    | 268      | 376      | 396
Errors/Sec	      | 0.00     | 0.00     | 0
Avg Response Time	| 0.17     | 0.12     | 0.23
Max Response Time	| 0.49     | 0.19     | 0.32
Total Record Count| 94,277   | 130,553  | 139577
Error Count	      | 0        | 0        | 0
