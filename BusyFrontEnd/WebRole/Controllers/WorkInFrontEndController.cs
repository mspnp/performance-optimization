using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebRole.Controllers
{
    public class WorkInFrontEndController : ApiController
    {
        public void Get(double number)
        {
            new Thread(() =>
            {
                //Simulate processing
                Thread.SpinWait(Int32.MaxValue / 100);
            }).Start();
            
        }
    }
}
