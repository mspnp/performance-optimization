This web api sample solution is to demonstrated a anti-pattern where there is a over utilization of
SQL resources in azure due to unnecessary string manipulations which can be done at the controller 
API level.

Considerations
1. In global.asax we load the query into the dictionary to avoid the cost of reloading per request.
there are two files in BusyDatabaseSupport: LessProcSql3.txt and TooMuchProcSql3.txt
which we execute in LessProcSqlController/GeNameConcat and TooMuchProcController/GetNameConcat
2. The delta in DTU transactional Units cost is 7% and 58% between less 
and more cost on SQL, which is very expensive
3. One issue to observe is tha one web api operation might result in more that one DTU
unit in SQL azure. Fewer expensive web api operations might result on DTU % starvation.
4. The main metrics to observe is DTU% in the portal and throughput and latency that will
be obsevable in app.dynamics
5. To fix this anti-pattern is to move all unnecessary processing away from SQL azure
into the web.api layer, which will results in less DTU consumption on SQL azure and more scaling
opportunity for the applications.

 