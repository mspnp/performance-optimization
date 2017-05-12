using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LockingApplicationdata.WebRole.Controllers
{
    public class NonBlockingController : ApiController
    {
        private static ConcurrentDictionary<string, string> concurrentDictionary = new ConcurrentDictionary<string, string>();

        [HttpGet]
        [Route("api/nonblocking")]
        public HttpResponseMessage ReadWrite()
        {
            throw new NotImplementedException();
        }
    }
}
