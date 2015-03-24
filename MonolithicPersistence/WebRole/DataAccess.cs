// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using WebRole.Models;

namespace WebRole
{
    public static class DataAccess
    {
        static Random rand = new Random();
        public static async Task InsertPurchaseOrderHeaderAsync(string cnStr)
        {
            const string queryString =
                "INSERT INTO Purchasing.PurchaseOrderHeader " +
                "(RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate) " +
                "VALUES " +
                "(@RevisionNumber, @Status, @EmployeeID, @VendorID, @ShipMethodID, @OrderDate, @ShipDate, @SubTotal, @TaxAmt, @Freight, @ModifiedDate)";

            var dt = DateTime.UtcNow;

            using (var cn = new SqlConnection(cnStr))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@RevisionNumber", 1);
                    cmd.Parameters.AddWithValue("@Status", 4);
                    cmd.Parameters.AddWithValue("@EmployeeID", 258);
                    cmd.Parameters.AddWithValue("@VendorID", 1580);
                    cmd.Parameters.AddWithValue("@ShipMethodID", 3);
                    cmd.Parameters.AddWithValue("@OrderDate", dt);
                    cmd.Parameters.AddWithValue("@ShipDate", dt);
                    cmd.Parameters.AddWithValue("@SubTotal", 123.40M);
                    cmd.Parameters.AddWithValue("@TaxAmt", 12.34M);
                    cmd.Parameters.AddWithValue("@Freight", 5.76M);
                    cmd.Parameters.AddWithValue("@ModifiedDate", dt);

                    await cn.OpenAsync().ConfigureAwait(false);
                    try
                    {
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                        int i = 0;

                    }
                }
            }
        }

        public static async Task InsertPurchaseOrderDetailAsync(string cnStr)
        {
            int purchaseOrderID = rand.Next(1, 1000);
            string queryString =
                "UPDATE Purchasing.PurchaseOrderHeader SET RevisionNumber = 1 WHERE purchaseOrderID = " + purchaseOrderID +
                "INSERT INTO Purchasing.PurchaseOrderDetail " +
                "(PurchaseOrderID,DueDate,OrderQty,ProductID,UnitPrice,ReceivedQty,RejectedQty,ModifiedDate) " +
                "VALUES " +
                "(@PurchaseOrderID,@DueDate,@OrderQty,@ProductID,@UnitPrice,@ReceivedQty,@RejectedQty,@ModifiedDate)";

            var dt = DateTime.UtcNow;

            using (var cn = new SqlConnection(cnStr))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderID);
                    cmd.Parameters.AddWithValue("@DueDate", dt);
                    cmd.Parameters.AddWithValue("@OrderQty", 1);
                    cmd.Parameters.AddWithValue("@ProductID", 405);
                    cmd.Parameters.AddWithValue("@UnitPrice", 0);
                    cmd.Parameters.AddWithValue("@ReceivedQty", rand.Next(1, 5));
                    cmd.Parameters.AddWithValue("@RejectedQty", rand.Next(1, 5));
                    cmd.Parameters.AddWithValue("@ModifiedDate", dt);
                    await cn.OpenAsync().ConfigureAwait(false);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task<string> SelectProductDescriptionAsync(string cnStr)
        {
            const string selectRandomId = "SELECT TOP 1 ProductDescriptionID FROM Production.ProductDescription ORDER BY NEWID()";
            const string selectDescription = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";
            var id = 3;
            using (SqlConnection cn = new SqlConnection(cnStr))
            {
                await cn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand(selectRandomId, cn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            id = reader.GetFieldValue<int>(0);
                        }
                    }
                }

                using (var cmd = new SqlCommand(selectDescription, cn))
                {
                    cmd.Parameters.AddWithValue("@inputId", id);
                    return await cmd.ExecuteScalarAsync().ConfigureAwait(false) as string;
                }
            }
        }

        public static async Task<string> SelectProductCategoryAsync(string cnStr)
        {
            const string QueryProductCategoryID = "SELECT TOP 1 ProductCategoryID FROM Production.ProductCategory ORDER BY NEWID()";
            const string QueryProductCategory = "SELECT Name FROM Production.ProductCategory WHERE ProductCategoryID=@inputId";
            var id = 3;
            using (SqlConnection cn = new SqlConnection(cnStr))
            {
                await cn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand(QueryProductCategoryID, cn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            id = reader.GetFieldValue<int>(0);
                        }
                    }
                }

                using (var cmd = new SqlCommand(QueryProductCategory, cn))
                {
                    cmd.Parameters.AddWithValue("@inputId", id);
                    return await cmd.ExecuteScalarAsync().ConfigureAwait(false) as string;
                }
            }
        }

        public static async Task InsertSalesOrderHeaderAsync(string cnStr)
        {

            const string queryString =
                "INSERT INTO SalesLT.SalesOrderHeader " +
                "(RevisionNumber,OrderDate,DueDate,ShipDate,Status,OnlineOrderFlag,PurchaseOrderNumber," +
                "AccountNumber,CustomerID,ShipToAddressID,BillToAddressID,ShipMethod,SubTotal,CreditCardApprovalCode," +
                "TaxAmt,Freight,Comment,rowguid,ModifiedDate) " +
                "VALUES " +
                "(@RevisionNumber,@OrderDate, @DueDate,@ShipDate,@Status,@OnlineOrderFlag,@PurchaseOrderNumber, " +
                "@AccountNumber,@CustomerID,@ShipToAddressID, @BillToAddressID,@ShipMethod, @CreditCardApprovalCode,@SubTotal," +
                "@TaxAmt,@Freight,@Comment, @rowguid,@ModifiedDate)";

            var dt = DateTime.UtcNow;

            using (var cn = new SqlConnection(cnStr))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@RevisionNumber", 2);
                    cmd.Parameters.AddWithValue("@OrderDate", dt);
                    cmd.Parameters.AddWithValue("@DueDate", dt);
                    cmd.Parameters.AddWithValue("@ShipDate", dt);
                    cmd.Parameters.AddWithValue("@Status", 5);
                    cmd.Parameters.AddWithValue("@OnlineOrderFlag", 0);
                    cmd.Parameters.AddWithValue("@PurchaseOrderNumber", "PO348186287");
                    cmd.Parameters.AddWithValue("@AccountNumber", "10-4020-000609");
                    cmd.Parameters.AddWithValue("@CustomerID", 29847);
                    cmd.Parameters.AddWithValue("@ShipToAddressID", 1092);
                    cmd.Parameters.AddWithValue("@BillToAddressID", 1092);
                    cmd.Parameters.AddWithValue("@ShipMethod", "CARGO TRANSPORT 5");
                    cmd.Parameters.AddWithValue("@CreditCardApprovalCode", rand.Next().ToString());
                    cmd.Parameters.AddWithValue("@SubTotal", rand.Next());
                    cmd.Parameters.AddWithValue("@TaxAmt", rand.Next());
                    cmd.Parameters.AddWithValue("@Freight", rand.Next());
                    cmd.Parameters.AddWithValue("@Comment", rand.Next().ToString());
                    cmd.Parameters.AddWithValue("@rowguid", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@ModifiedDate", dt);

                    await cn.OpenAsync().ConfigureAwait(false);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

            }
        }

        public static async Task LogAsync(string cnStr, string LogTableName)
        {
            string queryString = "INSERT INTO dbo." + LogTableName + "(LogId, Message, LogTime) VALUES(@LogId, @Message, @LogTime)";

            var logMessage = new LogMessage();

            using (var cn = new SqlConnection(cnStr))
            {
                using (var cmd = new SqlCommand(queryString, cn))
                {
                    cmd.Parameters.AddWithValue("@LogId", logMessage.LogId);
                    cmd.Parameters.AddWithValue("@Message", logMessage.Message);
                    cmd.Parameters.AddWithValue("@LogTime", logMessage.LogTime);

                    await cn.OpenAsync().ConfigureAwait(false);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }


    }
}