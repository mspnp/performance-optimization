# Load Testing BusyDatabase

## Deployment

Our load tests were ran against an Azure SQL Database premium instance (P3).
We used the [Adventure Works for Azure SQL Database][AW2012].

The ASP.NET application was deployed as a [Cloud Service][AzureCloudService],
using a single [Large instance][AzureCloudServiceSizes].

The `Max Pool Size` in the connection string was set to an arbitrarily high
value (4000) in order to prevent the connection pool from being a constraining
resource.

## Test Configuration

We used [Visual Studio Online][VsoLoadTesting] to perform our tests.
There were two webtests, each invoking a `GET` using the `Generate Random Integer`
plugin.

The urls were similar to

- http://something.cloudapp.net/toomuchprocsql/get/{{orderid}}
- http://something.cloudapp.net/lessprocsql/get/{{orderid}}

We had a load test for each web test. The load tests were ran against the same
deployment but at different times .

Both load tests used a *Step* load pattern with the following parameter values:

Parameter           | Value
------------------- | ------------:
Initial User Count  | 1
Maximum User Count  | 50
Step Duration       | 30s
Step Ramp Time      | 30s
Step User Count     | 1

Each load test ran for 30 minutes, with a 30 second warm-up period.

[AW2012]: http://msftdbprodsamples.codeplex.com/releases/view/37304
[AzureSQL]: http://azure.microsoft.com/en-us/pricing/details/sql-database/
[AzureCloudService]: http://azure.microsoft.com/en-us/documentation/services/cloud-services/
[AzureCloudServiceSizes]: https://msdn.microsoft.com/library/azure/dn197896.aspx
[VsoLoadTesting]: https://www.visualstudio.com/get-started/test/load-test-your-app-vs
