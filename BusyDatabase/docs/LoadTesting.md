# Load Testing BusyDatabase

Read about our [general approach][] to deployment and load testing.

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
Test Duration       | 30 minutes
Test Warm Up        | 30 seconds

[general approach]: /LoadTesting.md
