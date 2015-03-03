using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logic;

namespace WebRole.Controllers
{
    public class WorkInFrontEndController : ApiController
    {
        public void Post(double number)
        {
            new Thread(() =>
            {
                var result = Calculator.RunLongComputation(number);
                Trace.WriteLine("Result: {0}", result.ToString());
            }).Start();
            
        }
    }
}
