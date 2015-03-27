--DECLARE @TerritoryId INT
--SET @TerritoryId = 1

SELECT * FROM (
-- root node <Orders />
SELECT 
1						AS Tag,
NULL					AS Parent,
NULL					AS [Orders!1],
NULL					AS [Order!2!OrderNumber],
NULL					AS [Order!2!Status],
NULL					AS [Order!2!ShipDate],
NULL					AS [Order!2!OrderDateYear],
NULL					AS [Order!2!OrderDateMonth],
NULL					AS [Order!2!DueDate],
NULL					AS [Order!2!SubTotal],
NULL					AS [Order!2!TaxAmt],
NULL					AS [Order!2!TotalDue],
NULL					AS [Order!2!ReviewRequired],
NULL					AS [Customer!3!AccountNumber],
NULL					AS [Customer!3!FullName],
NULL					AS [OrderLineItems!4],
NULL					AS [LineItem!5!Quantity],
NULL					AS [LineItem!5!UnitPrice],
NULL					AS [LineItem!5!LineTotal],
NULL					AS [LineItem!5!ProductId],
NULL					AS [LineItem!5!InventoryCheckRequired]

UNION ALL
-- <Order />
SELECT 
2						AS Tag,
1						AS Parent,
NULL					AS [Orders!1],
soh.[SalesOrderNumber]	AS [Order!2!OrderNumber],
soh.[Status]			AS [Order!2!Status],
soh.[ShipDate]			AS [Order!2!ShipDate],
YEAR(soh.[OrderDate])	AS [Order!2!OrderDateYear],
MONTH(soh.[OrderDate])	AS [Order!2!OrderDateMonth],
soh.[DueDate]			AS [Order!2!DueDate],
FORMAT(ROUND(soh.[SubTotal],2),'C')	
						AS [Order!2!SubTotal],
FORMAT(ROUND(soh.[TaxAmt],2),'C')	
						AS [Order!2!TaxAmt],
FORMAT(ROUND(soh.[TotalDue],2),'C')	
						AS [Order!2!TotalDue],
CASE WHEN soh.[TotalDue] > 5000 THEN 'Y' ELSE 'N' END 
						AS [Order!2!ReviewRequired],
NULL					AS [Customer!3!AccountNumber],
NULL					AS [Customer!3!FullName],
NULL					AS [OrderLineItems!4],
NULL					AS [LineItem!5!Quantity],
NULL					AS [LineItem!5!UnitPrice],
NULL					AS [LineItem!5!LineTotal],
NULL					AS [LineItem!5!ProductId],
NULL					AS [LineItem!5!InventoryCheckRequired]
FROM [Sales].[SalesOrderHeader] soh
WHERE soh.[TerritoryId] = @TerritoryId

UNION ALL

-- <Customer />
SELECT
3						AS Tag,
2						AS Parent,
NULL					AS [Orders!1],
soh.[SalesOrderNumber]	AS [Order!2!OrderNumber],
NULL					AS [Order!2!Status],
NULL					AS [Order!2!ShipDate],
NULL					AS [Order!2!OrderDateYear],
NULL					AS [Order!2!OrderDateMonth],
NULL					AS [Order!2!DueDate],
NULL					AS [Order!2!SubTotal],
NULL					AS [Order!2!TaxAmt],
NULL					AS [Order!2!TotalDue],
NULL					AS [Order!2!ReviewRequired],
c.[AccountNumber]		AS [Customer!3!AccountNumber],
UPPER(LTRIM(RTRIM(REPLACE(
	CONCAT( p.[Title], ' ', p.[FirstName], ' ', p.[MiddleName], ' ', p.[LastName], ' ', p.[Suffix]),
	'  ', ' '))))
						AS [Customer!3!FullName],
NULL					AS [OrderLineItems!4],
NULL					AS [LineItem!5!Quantity],
NULL					AS [LineItem!5!UnitPrice],
NULL					AS [LineItem!5!LineTotal],
NULL					AS [LineItem!5!ProductId],
NULL					AS [LineItem!5!InventoryCheckRequired]
FROM [Sales].[Customer] c
INNER JOIN [Sales].[SalesOrderHeader] soh ON soh.[CustomerID] = c.[CustomerID]
LEFT OUTER JOIN [Person].[Person] p ON p.[BusinessEntityID] = c.[PersonID]
WHERE soh.[TerritoryId] = @TerritoryId

UNION ALL

-- <OrderLineItems />
SELECT 
4						AS Tag,
2						AS Parent,
NULL					AS [Orders!1],
soh.[SalesOrderNumber]	AS [Order!2!OrderNumber],
NULL					AS [Order!2!Status],
NULL					AS [Order!2!ShipDate],
NULL					AS [Order!2!OrderDateYear],
NULL					AS [Order!2!OrderDateMonth],
NULL					AS [Order!2!DueDate],
NULL					AS [Order!2!SubTotal],
NULL					AS [Order!2!TaxAmt],
NULL					AS [Order!2!TotalDue],
NULL					AS [Order!2!ReviewRequired],
NULL					AS [Customer!3!AccountNumber],
NULL					AS [Customer!3!FullName],
NULL					AS [OrderLineItems!4],
NULL					AS [LineItem!5!Quantity],
NULL					AS [LineItem!5!UnitPrice],
NULL					AS [LineItem!5!LineTotal],
NULL					AS [LineItem!5!ProductId],
NULL					AS [LineItem!5!InventoryCheckRequired]
FROM [Sales].[SalesOrderHeader] soh
WHERE soh.[TerritoryId] = @TerritoryId

UNION ALL

-- <LineItem />
SELECT 
5						AS Tag,
4						AS Parent,
NULL					AS [Orders!1],
soh.[SalesOrderNumber]	AS [Order!2!OrderNumber],
NULL					AS [Order!2!Status],
NULL					AS [Order!2!ShipDate],
NULL					AS [Order!2!OrderDateYear],
NULL					AS [Order!2!OrderDateMonth],
NULL					AS [Order!2!DueDate],
NULL					AS [Order!2!SubTotal],
NULL					AS [Order!2!TaxAmt],
NULL					AS [Order!2!TotalDue],
NULL					AS [Order!2!ReviewRequired],
NULL					AS [Customer!3!AccountNumber],
NULL					AS [Customer!3!FullName],
NULL					AS [OrderLineItems!4],
sod.[OrderQty]			AS [LineItem!5!Quantity],
FORMAT(sod.[UnitPrice],'C')
						AS [LineItem!5!UnitPrice],
FORMAT(ROUND(sod.[LineTotal],2),'C')	
						AS [LineItem!5!LineTotal],
sod.[ProductID]			AS [LineItem!5!ProductId],
CASE WHEN (sod.[ProductID] > 710) AND (sod.[ProductID] < 720) AND (sod.[OrderQty] > 5) THEN 'Y' ELSE 'N' END 
						AS [LineItem!5!InventoryCheckRequired]

FROM [Sales].[SalesOrderDetail] sod
INNER JOIN [Sales].[SalesOrderHeader] soh ON sod.[SalesOrderID] = soh.[SalesOrderID]
WHERE soh.[TerritoryId] = @TerritoryId
) AS x
ORDER BY x.[Order!2!OrderNumber], x.[LineItem!5!ProductId]
FOR XML EXPLICIT