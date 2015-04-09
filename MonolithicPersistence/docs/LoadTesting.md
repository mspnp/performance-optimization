# Load Testing MonolithicPersistence

This document summarizes the configuration we used to perform load-testing for the MonolithicPersistence anti-pattern. You should also read about our [general approach][general approach] to deployment and load testing.

## Deployment

 Option             | Value  
------------------- | -------------
Compute             | Cloud Service
VM Size             | Large
Instance Count      | 5
SQL Tier            | S1
Max Pool Size       | 4000

## Test Configuration

The load test project included two webtests, each invoking an HTTP `POST` operation.

The URLs used were:

- http://yourservice.cloudapp.net/api/mono
- http://yourservice.cloudapp.net/api/poly

Replace *yourservice* with the name of your cloud service.

The project also included two load tests, one for each web test. Both load tests were
run against a single deployment but at different times, using the following parameters:

Parameter           | Value
------------------- | ------------:
Initial User Count  | 1
Maximum User Count  | 1000
Step Duration       | 60s
Step Ramp Time      | 0s
Step User Count     | 100
Test Duration       | 15 minutes
Test Warm Up        | 30 seconds

The load test for the http://yourservice.cloudapp.net/api/mono web test generated the following results:

![Load-test results][LoadTest1]

The load test for the http://yourservice.cloudapp.net/api/poly web test generated the following results:

![Load-test results][LoadTest2]

[general approach]: /LoadTesting.md

[LoadTest1]: Figures/MonolithicScenarioLoadTest.jpg
[LoadTest2]: Figures/PolyglotScenarioLoadTest.jpg
