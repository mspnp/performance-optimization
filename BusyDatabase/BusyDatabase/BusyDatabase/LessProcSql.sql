SELECT
soh.[SalesOrderNumber]  AS [OrderNumber],
soh.[Status]            AS [Status],
soh.[OrderDate]         AS [OrderDate],
soh.[DueDate]           AS [DueDate],
soh.[ShipDate]          AS [ShipDate],
soh.[SubTotal]          AS [SubTotal],
soh.[TaxAmt]            AS [TaxAmt],
soh.[TotalDue]          AS [TotalDue],
c.[CompanyName]         AS [CompanyName],
sod.[OrderQty]          AS [Quantity],
sod.[UnitPrice]         AS [UnitPrice],
sod.[LineTotal]         AS [LineTotal],
sod.[ProductID]         AS [ProductId],
a.[City]                AS [City],
a.[CountryRegion]       AS [CountryRegion],
a.[PostalCode]          AS [PostalCode],
a.[StateProvince]       AS [StateProvince]
FROM [SalesLT].[SalesOrderHeader] soh
INNER JOIN [SalesLT].[Customer] c ON soh.[CustomerID] = c.[CustomerID]
INNER JOIN [SalesLT].[SalesOrderDetail] sod ON soh.[SalesOrderID] = sod.[SalesOrderID]
INNER JOIN [SalesLT].[CustomerAddress] p  ON c.[CustomerID] = p.[CustomerID]
INNER JOIN [SalesLT].[Address] a  ON a.[AddressID] = p.[AddressID] 
WHERE soh.[SalesOrderId] IN (
	SELECT TOP 20 SalesOrderId 
	FROM [SalesLT].[SalesOrderHeader] soh 
	WHERE soh.CustomerID = @CustomerID
	ORDER BY soh.[TotalDue] DESC)
ORDER BY soh.[TotalDue] DESC, sod.[SalesOrderDetailID]