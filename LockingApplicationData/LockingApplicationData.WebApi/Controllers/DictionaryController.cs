using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace LockingApplicationData.WebApi.Controllers
{
    public class DictionaryController : ApiController
    {
        private static IDictionary<string, string> dictionary = new Dictionary<string, string>();

        private static object _lock = new object();


        [HttpGet]
        [Route("dictionary/get_value")]
        public HttpResponseMessage GetValue(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "key should be not null nor empty.");
            }

            lock(_lock)
            {
                string value;

                if(!dictionary.TryGetValue(key, out value))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("the key {0} is not present in the dictionary.", key));
                }

                return Request.CreateResponse(HttpStatusCode.OK, value);
            }
        }

        [HttpPost]
        [Route("dictionary/set_value")]
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

            lock(_lock)
            {
                if(!dictionary.ContainsKey(key))
                {
                    dictionary.Add(new KeyValuePair<string, string>(key, value));
                }
                else
                {
                    dictionary[key] = value;
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}