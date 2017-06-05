# Optimizing Performance for Cloud Applications

![](pnp-logo.png)

This repo contains sample code for a set of performance antipatterns.

Documentation and guidance for these patterns can be found on the [Azure Architecture Center](https://docs.microsoft.com/azure/architecture/antipatterns/). For each antipattern, the documentation describes why the antipattern typically occurs, symptoms of the antipattern, and techniques for resolving the problem. The corresponding sample code shows (1) the problem and (2) a recommended way to fix the problem.


| Antipattern | Description | Load test |
|-------------|-------------|-----------|
| [Busy Database][BusyDatabase] | Offloading too much processing to a data store. | [Load testing Busy Database][BusyDatabase-LoadTesting] |
| [Busy Front End][BusyFrontEnd] | Moving resource-intensive tasks onto background threads. | [Load testing Busy Front End][BusyFrontEnd-LoadTesting] |
| [Chatty I/O][ChattyIO] | Continually sending many small network requests. | [Load testing Chatty I/O][ChattyIO-LoadTesting] |
| [Extraneous Fetching][ExtraneousFetching] | Retrieving more data than is needed, resulting in unnecessary I/O. | [Load testing Extraneous Fetching][ExtraneousFetching-LoadTesting] |
| [Improper Instantiation][ImproperInstantiation] | Repeatedly creating and destroying objects that are designed to be shared and reused. | [Load testing Improper Instantiation][ImproperInstantiation-LoadTesting] |
| [Monolithic Persistence][MonolithicPersistence] | Using the same data store for data with very different usage patterns. | [Load testing Monolithic Persistence][MonolithicPersistence-LoadTesting] |
| [No Caching][NoCaching] | Failing to cache data. | [Load testing No Caching][NoCaching-LoadTesting] |
| [Synchronous I/O][SynchronousIO] | Blocking the calling thread while I/O completes. | [Load testing Synchronous I/O][SynchronousIO-LoadTesting] |

[BusyDatabase]: https://docs.microsoft.com/azure/architecture/antipatterns/busy-database/
[BusyFrontEnd]: https://docs.microsoft.com/azure/architecture/antipatterns/busy-front-end/
[ChattyIO]: https://docs.microsoft.com/azure/architecture/antipatterns/chatty-io/
[ExtraneousFetching]: https://docs.microsoft.com/azure/architecture/antipatterns/extraneous-fetching/
[ImproperInstantiation]: https://docs.microsoft.com/azure/architecture/antipatterns/improper-instantiation/
[MonolithicPersistence]: https://docs.microsoft.com/azure/architecture/antipatterns/monolithic-persistence/
[NoCaching]: https://docs.microsoft.com/azure/architecture/antipatterns/no-caching/
[SynchronousIO]: https://docs.microsoft.com/azure/architecture/antipatterns/synchronous-io/

[BusyDatabase-LoadTesting]: BusyDatabase/docs/LoadTesting.md
[BusyFrontEnd-LoadTesting]: BusyFrontEnd/docs/LoadTesting.md
[ChattyIO-LoadTesting]: ChattyIO/docs/LoadTesting.md
[ExtraneousFetching-LoadTesting]: ExtraneousFetching/docs/LoadTesting.md
[ImproperInstantiation-LoadTesting]: ImproperInstantiation/docs/LoadTesting.md
[MonolithicPersistence-LoadTesting]: MonolithicPersistence/docs/LoadTesting.md
[NoCaching-LoadTesting]: NoCaching/docs/LoadTesting.md
[SynchronousIO-LoadTesting]: SynchronousIO/docs/LoadTesting.md

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

