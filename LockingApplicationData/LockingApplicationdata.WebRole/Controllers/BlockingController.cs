
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LockingApplicationdata.WebRole.Controllers
{
    public class BlockingController : ApiController
    {
        private static IDictionary<string, string> dictionary = new Dictionary<string, string>();

        private static object _lock = new object();

        [HttpGet]
        [Route("api/blocking/lock")]
        public HttpResponseMessage ReadWrite()
        {
            lock(_lock)
            {
                AddOrUpdate("key1", "value1");
                
                var read1 = dictionary["key1"];
                var read2 = dictionary["key1"];
                var read3 = dictionary["key1"];
                var read4 = dictionary["key1"];

                AddOrUpdate("key2", "value2");

                var read5 = dictionary["key2"];
                var read6 = dictionary["key2"];
                var read7 = dictionary["key2"];
                var read8 = dictionary["key2"];

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        private void AddOrUpdate(string key, string value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(new KeyValuePair<string, string>(key, value));
            }
            else
            {
                dictionary[key] = value;
            }
        }
    }
}
