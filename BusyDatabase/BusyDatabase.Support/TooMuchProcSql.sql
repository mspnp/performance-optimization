--DECLARE @OrderId INT
--SET @OrderId = 47001



-- root node
SELECT 
1						AS Tag,
NULL					AS Parent,
soh.[SalesOrderNumber]	AS [Order!1!OrderNumber],
soh.[Status]			AS [Order!1!Status],
soh.[ShipDate]			AS [Order!1!ShipDate],
YEAR(soh.[OrderDate])	AS [Order!1!OrderDateYear],
MONTH(soh.[OrderDate])	AS [Order!1!OrderDateMonth],
soh.[DueDate]			AS [Order!1!DueDate],
ROUND(soh.[SubTotal],2)	AS [Order!1!SubTotal],
ROUND(soh.[TaxAmt],2)	AS [Order!1!TaxAmt],
ROUND(soh.[TotalDue],2)	AS [Order!1!TotalDue],
CASE WHEN soh.[TotalDue] > 5000 THEN 'Y' ELSE 'N' END 
						AS [Order!1!ReviewRequired],
NULL					AS [Customer!2!AccountNumber],
NULL					AS [Customer!2!FullName],
NULL					AS [OrderLineItems!3],
NULL					AS [LineItem!4!Quantity],
NULL					AS [LineItem!4!UnitPrice],
NULL					AS [LineItem!4!LineTotal],
NULL					AS [LineItem!4!ProductId],
NULL					AS [LineItem!4!InventoryCheckRequired]
FROM [Sales].[SalesOrderHeader] soh
WHERE soh.SalesOrderID = @OrderId

UNION ALL

-- customer info
SELECT
2						AS Tag,
1						AS Parent,
NULL					AS [Order!1!OrderNumber],
NULL					AS [Order!1!Status],
NULL					AS [Order!1!ShipDate],
NULL					AS [Order!1!OrderDateYear],
NULL					AS [Order!1!OrderDateMonth],
NULL					AS [Order!1!DueDate],
NULL					AS [Order!1!SubTotal],
NULL					AS [Order!1!TaxAmt],
NULL					AS [Order!1!TotalDue],
NULL					AS [Order!1!ReviewRequired],
c.[AccountNumber]		AS [Customer!2!AccountNumber],
UPPER(LTRIM(RTRIM(REPLACE(
	CONCAT( p.[Title], ' ', p.[FirstName], ' ', p.[MiddleName], ' ', p.[LastName], ' ', p.[Suffix]),
	'  ', ' '))))
						AS [Customer!2!FullName],
NULL					AS [OrderLineItems!3],
NULL					AS [LineItem!4!Quantity],
NULL					AS [LineItem!4!UnitPrice],
NULL					AS [LineItem!4!LineTotal],
NULL					AS [LineItem!4!ProductId],
NULL					AS [LineItem!4!InventoryCheckRequired]
FROM [Sales].[Customer] c
INNER JOIN [Sales].[SalesOrderHeader] soh ON soh.[CustomerID] = c.[CustomerID]
LEFT OUTER JOIN [Person].[Person] p ON p.[BusinessEntityID] = c.[PersonID]
WHERE soh.SalesOrderID = @OrderId

UNION ALL

-- container tag for line items
SELECT 
3						AS Tag,
1						AS Parent,
NULL					AS [Order!1!OrderNumber],
NULL					AS [Order!1!Status],
NULL					AS [Order!1!ShipDate],
NULL					AS [Order!1!OrderDateYear],
NULL					AS [Order!1!OrderDateMonth],
NULL					AS [Order!1!DueDate],
NULL					AS [Order!1!SubTotal],
NULL					AS [Order!1!TaxAmt],
NULL					AS [Order!1!TotalDue],
NULL					AS [Order!1!ReviewRequired],
NULL					AS [Customer!2!AccountNumber],
NULL					AS [Customer!2!FullName],
NULL					AS [OrderLineItems!3],
NULL					AS [LineItem!4!Quantity],
NULL					AS [LineItem!4!UnitPrice],
NULL					AS [LineItem!4!LineTotal],
NULL					AS [LineItem!4!ProductId],
NULL					AS [LineItem!4!InventoryCheckRequired]

UNION ALL

-- line items
SELECT 
4						AS Tag,
3						AS Parent,
NULL					AS [Order!1!OrderNumber],
NULL					AS [Order!1!Status],
NULL					AS [Order!1!ShipDate],
NULL					AS [Order!1!OrderDateYear],
NULL					AS [Order!1!OrderDateMonth],
NULL					AS [Order!1!DueDate],
NULL					AS [Order!1!SubTotal],
NULL					AS [Order!1!TaxAmt],
NULL					AS [Order!1!TotalDue],
NULL					AS [Order!1!ReviewRequired],
NULL					AS [Customer!2!AccountNumber],
NULL					AS [Customer!2!FullName],
NULL					AS [OrderLineItems!3],
sod.[OrderQty]			AS [LineItem!4!Quantity],
sod.[UnitPrice]			AS [LineItem!4!UnitPrice],
ROUND(sod.[LineTotal],2)AS [LineItem!4!LineTotal],
sod.[ProductID]			AS [LineItem!4!ProductId],
CASE WHEN (sod.[ProductID] > 710) AND (sod.[ProductID] < 720) AND (sod.[OrderQty] > 5) THEN 'Y' ELSE 'N' END 
						AS [LineItem!4!InventoryCheckRequired]

FROM [Sales].[SalesOrderDetail] sod
WHERE sod.SalesOrderID = @OrderId

FOR XML EXPLICIT