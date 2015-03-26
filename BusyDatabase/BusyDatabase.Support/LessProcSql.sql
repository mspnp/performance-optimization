--DECLARE @OrderId INT
--SET @OrderId = 47001

SELECT 
soh.[SalesOrderNumber]	AS [OrderNumber],
soh.[Status]			AS [Status],
soh.[OrderDate]			AS [OrderDate],
soh.[DueDate]			AS [DueDate],
soh.[ShipDate]			AS [ShipDate],
soh.[SubTotal]			AS [SubTotal],
soh.[TaxAmt]			AS [TaxAmt],
soh.[TotalDue]			AS [TotalDue],
c.[AccountNumber]		AS [AccountNumber],
p.[Title]				AS [CustomerTitle],
p.[FirstName]			AS [CustomerFirstName],
p.[MiddleName]			AS [CustomerMiddleName],
p.[LastName]			AS [CustomerLastName],
p.[Suffix]				AS [CustomerSuffix],
sod.[OrderQty]			AS [Quantity],
sod.[UnitPrice]			AS [UnitPrice],
sod.[LineTotal]			AS [LineTotal],
sod.[ProductID]			AS [ProductId]
FROM [Sales].[SalesOrderHeader] soh
INNER JOIN [Sales].[Customer] c ON soh.[CustomerID] = c.[CustomerID]
INNER JOIN [Person].[Person] p ON c.[PersonID] = p.[BusinessEntityID] 
LEFT OUTER JOIN [Sales].[SalesOrderDetail] sod ON soh.[SalesOrderID] = sod.[SalesOrderID]
WHERE soh.SalesOrderID = @OrderId
