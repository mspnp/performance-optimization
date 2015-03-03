using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logic;

namespace WebRole.Controllers
{
    public class WorkInFrontEndController : ApiController
    {
        public void Get(double number)
        {
            new Thread(() =>
            {
                Trace.WriteLine("Number: " + number);
                var result = Calculator.RunLongComputation(number);
                Trace.WriteLine("Result: " + result);
            }).Start();
            
        }
    }
}
