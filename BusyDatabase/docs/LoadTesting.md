# Load Testing BusyDatabase

## Deployment

 Option             | Value  
------------------- | -------------
Compute             | Cloud Service
VM Size             | Large
Instances Count     | 1
SQL Tier            | P3
`Max Pool Size`     | 4000

## Test Configuration

The load test project included two webtests, each invoking a `GET` using the
`Generate Random Integer` plugin.

The urls were similar to

- http://something.cloudapp.net/toomuchprocsql/get/{{orderid}}
- http://something.cloudapp.net/lessprocsql/get/{{orderid}}

The project include two load tests, one for each web test. Both load tests were
ran against a single deployment but at different times.

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
