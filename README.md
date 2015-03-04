# Build and Run Monolithic Anti Pattern Code Sample

## Step 1: Create Azure Storage Accounts and update the connections string to the MonoglotPersistence solution

- Create two Azure storage accounts.

- Start Visual Studio and open MonoglotPersistence.sln

- Open Web.config file

- Update the connectionString for azureStorageConnectionString1

- Update the connectionString for azureStorageConnectionString2

## Step 2: Set up AdventureWorks DB and update the connections sting to the MonoglotPersistence solution

- Install the AdventureWorks Database on Azure DB, Add the IP Rules so that you can access the DB from your dev box.

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

- Press F5 to start debugging session

-
