# Running Load Tests

We used [Visual Studio Online][VsoLoadTesting] to perform our load tests.
The specifics of each load test differed depending on the particular
anti-pattern that we were analyzing.

In general, we used a [step load pattern][LoadPattern] because we are interested
in understanding how these particular anti-patterns affect characteristics such
as elasticity, throughput, and latency.

We are not publishing the load tests themselves, only because the are tied to
the specific deployments that we used. However, the load tests should be easy to
reconstruct based on the information included here and with each anti-pattern.

## Deploying to Azure

The current set of anti-patterns utilizes the following Azure services:

- [Cloud Services][]
- [Web Apps][]
- [SQL Azure][]
- [Redis Cache][]
- [Service Bus][]

In order to deploy the code in this project, you will need to provision the
corresponding resources.

### Regarding SQL

Unless specified otherwise, for each deployment we used:

- [Adventure Works for Azure SQL Database][AW2012] database.
- [Premium P3 instances][SQL Tiers]

You could deploy and test against different tiers.

The `Max Pool Size` in our connection strings was set to an arbitrarily high
value (4000) in order to prevent the connection pool from being a constraining
resource. However, this is not a default best practice.

### Regarding Compute

Samples that are deployed as _Cloud Services_ include an `AzureCloudService`
projects. The [VM size][Cloud Services Sizes] of the deployment varied with
each anti-pattern.

If no `AzureCloudService` project is present in the sample, it is intended to
be deployed as a _Web App_.

[LoadPatterns]: https://msdn.microsoft.com/en-us/library/dd997551.aspx
[AW2012]: http://msftdbprodsamples.codeplex.com/releases/view/37304
[SQL Azure]: http://azure.microsoft.com/en-us/pricing/details/sql-database/
[Cloud Services]: http://azure.microsoft.com/en-us/documentation/services/cloud-services/
[Web Apps]: http://azure.microsoft.com/en-us/services/app-service/web/
[Redis Cache]: http://azure.microsoft.com/en-us/services/cache/
[Service Bus]: http://azure.microsoft.com/en-us/documentation/services/service-bus/
[Cloud Services Sizes]: https://msdn.microsoft.com/library/azure/dn197896.aspx
[SQL Tiers]: https://msdn.microsoft.com/library/azure/dn741336.aspx?f=255&MSPPError=-2147217396
[VsoLoadTesting]: https://www.visualstudio.com/get-started/test/load-test-your-app-vs
