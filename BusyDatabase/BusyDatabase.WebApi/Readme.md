This Web API sample solution is to demonstrate an anti-pattern where there is a over utilization of SQL resources in Azure due to unnecessary string manipulations which can be done at the controller API level.

## Considerations

1. In `global.asax` we load the query into the dictionary to avoid the cost of reloading per request. There are two files in `BusyDatabase.Support` that we execute in `LessProcSqlController/GetNameConcat` and `TooMuchProcController/GetNameConcat`:
	* `LessProcSql3.txt`
	* `TooMuchProcSql3.txt`

2. The delta in DTU transactional Units cost is 7% and 58% between less and more cost on SQL, which is very expensive.

3. One issue to observe is tha one web api operation might result in more that one DTU unit in Azure SQL. Fewer expensive web api operations might result on DTU % starvation.

4. The main metrics to observe is DTU% in the portal and throughput and latency that will be observable in other tooling.

5. To fix this anti-pattern is to move all unnecessary processing away from SQL azure into the web.api layer, which will results in less DTU consumption on SQL azure and more scaling opportunity for the applications.
