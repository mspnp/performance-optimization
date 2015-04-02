# Build and Run Monolithic Anti Pattern Code Sample

## Step 1: Set up AdventureWorks DB and update the Settings in ServiceConfiguration cscfg files

- Create an Azure SQL DB in Windows Azure. Install AdventureWorks Database on it, add the IP rules so that you can access the DB from your development box. This is your production database.

- Create another Azure SQL DB. Create an empty database on it, add the IP rules so that you can access the DB from your development box. This is your Log database.

- Start **Visual Studio** and open **MonolithicPersistence.sln**

- Expand **AzureCloudService->Roles->WebRole** and right click **WebRole** and select **Properties**.

- Click **Settings**.

- Update the **Value** for **ProductionSQlDbCnStr** with the connection string from the AdventureWorks Production database.

- Update the **Value** for **LogSQlDbCnStr** with the connection string from the Log Database.

## Step 2: Test the solution locally

### Test MonoController

You are going to test the following MonoController method:

```C#
public async Task<IHttpActionResult> PostAsync()
{
    await DataAccess.InsertPurchaseOrderHeaderAsync(ProductionDb);

    await DataAccess.LogAsync(ProductionDb, LogTableName);

    return Ok();
}
```

The method **InsertPurchaseOrderHeaderAsync(ProductionDb)** executes the following SQL query:

```sql
INSERT INTO Purchasing.PurchaseOrderHeader(
    RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID,
    OrderDate,
    ShipDate,
    SubTotal,TaxAmt, Freight,
    ModifiedDate)
VALUES(
    1, 4, 258, 1580, 3,
    2015-03-05 14:23:04.743,
    2015-03-05 14:23:04.743,
    123.40, 12.34, 5.76,
    2015-03-05 14:23:04.743)
```
**Note**: date value is obtained at runtime and will be different from the above.

The method **LogAsync(ProductionDb, LogTableName)** executes the following SQL query:

``` sql
INSERT INTO dbo.MonoLog(LogId, Message, LogTime)
VALUES(@LogId, @Message, @LogTime)
```

**Note**: **@LogId, @Message** are random strings and **@LogTime** is obtained at runtime

**Test Steps**

Here are the test steps using Fiddler. You can also use any of your preferred tool to send a post message.

- Start **Visual Studio** and open **MonolithicPersistence.sln**

- Press **F5** to start debugging session, wait for IE to open. make a note of the port number

- Start **Fiddler**

- Click **Composer**

- Change http verbs to **POST**

- Change the url to **http://localhost:yourportnumber/api/mono**

  **Note** replace yourportnumber with the number in the IE address

- Click  **Execute**

- Verify that record is inserted to the database: In Sql Server Object Explorer tree view, expand to AdventureWorks2012 database and start the following query

``` sql
SELECT Count(*) FROM Purchasing.PurchaseOrderHeader

SELECT Count(*) FROM dbo.MonoLog
```
- Make a note of the result for the count.

- In Fiddler, Execute the POST again

- In SQL query, run the query again. Verify the result count is increased by 1.

### Test PolyController

- Follow the same procedure as the previous step using **Poly** instead of **Mono**.

- To verify that the log is inserted to the LogDB, in Sql Server Object Explorer tree view, expand to Log database and run the following query

``` sql
SELECT Count(*) FROM dbo.PolyLog
```


## Step 3: Publish the AzureCloudService to Microsoft Azure

- Start **Visual Studio** and open **MonolithicPersistence.sln**

- Right click on **AzureCloudService** and click **Publish...**.  

- Wait for the publish to complete

- Test the deployment follow the same steps as that in Step 2. You need to change the url to point to your cloud service.

## Step 3: Load Test MonoController:

- Create a web test that consists of a web service requests (http post) with the url **http://mycloudservice.cloudapp.net/api/Mono**

   **Note**: you need to replace mycloudservice with the actual value.

- Run web test and make sure that it passes.

- Create a load test that include the above web test and change the load test settings as follows:

Step Load Pattern       | Value
------------------------| -----------
Pattern                 | Step
Initial User Count      | 1
Maximum User Count      | 1000
Step Duration (seconds) | 60
Step Ramp Time (seconds)| 30
Step User Count         | 100


Run Settings              | Value
------------------------  | -----------
Agent Count (Total Cores) | 20
Run Duration              | 00:15:00
Sample Rate               | 00:00:15
Warm-up Duration          | 00:00:30

- Connect to the production db and run the following sql to reset the PurchaseOrderHeader table and the MonoLog table:

```sql
DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012

DELETE FROM dbo.MonoLog
```

- Start the load test.

## Step 3: Load Test PolyController:
- Create a web test that consists of a web service requests (http post) with the url **http://mycloudservice.cloudapp.net/api/Poly**

  **Note**: you need to replace mycloudservice with the actual value.

- Run web test and make sure that it passes.

- Create a load test that include the above web test and change the load test settings to the same as that of the above load test for the MonoController.

- Connect to the production db and run the following sql to reset the PurchaseOrderHeader table:

```sql
DELETE FROM Purchasing.PurchaseOrderHeader WHERE PurchaseOrderID > 4012
```

- Connect to the Log db and run the following sql to reset the PolyLog table:

```sql
DELETE FROM dbo.PolyLog
```

- Start the load test.
