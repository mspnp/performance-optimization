using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace LockingApplicationData.WebApi.Controllers
{
    public class ContentionController : ApiController
    {
        private static IDictionary<string, string> dictionary = new Dictionary<string, string> 
        {
            {"key1", "Some Value"}
        };

        private static ConcurrentDictionary<string, string> concurrentDictionary = new ConcurrentDictionary<string, string>
        (
            new Dictionary<string, string>
            {
                {"key1", "Some Value"}
            }
        );

        private static object _lock = new object();

        [HttpGet]
        [Route("contention/dictionary_no_locking")]
        public string GetValueFromDictionaryNoLocking()
        {
            return dictionary["key1"];
        }

        [HttpGet]
        [Route("contention/dictionary_locking")]
        public string GetValueFromDictionaryLocking()
        {
            lock(_lock)
            {
                return dictionary["key1"];
            }
        }

        [HttpGet]
        [Route("contention/concurrent_dictionary")]
        public string GetValueFromConcurrentDictionaty()
        {
            return dictionary["key1"];
        }
    }
}