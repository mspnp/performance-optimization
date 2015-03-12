using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace LockingApplicationData.WebApi.Controllers
{
    public class ReadWriteContentionController : ApiController
    {
        private static ConcurrentDictionary<string, string> concurrentDictionary = new ConcurrentDictionary<string, string>();

        [HttpGet]
        [Route("concurrentdictionary/get_value")]
        public HttpResponseMessage GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "key should be not null nor empty.");
            }
            string value;

            if (!concurrentDictionary.TryGetValue(key, out value))
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("the key {0} is not present in the dictionary.", key));
            }

            return Request.CreateResponse(HttpStatusCode.OK, value);
        }

        [HttpPost]
        [Route("concurrentdictionary/set_value")]
        public HttpResponseMessage SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "key should be not null nor empty.");
            }

            if (string.IsNullOrEmpty(value))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "value should be not null nor empty.");
            }

            concurrentDictionary.AddOrUpdate(key, value, (key1, oldValue) => oldValue);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}