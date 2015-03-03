using System.Threading.Tasks;
using System.Web.Http;
using Common.Logic;

namespace WebRole.Controllers
{
    public class WorkInBackgroundController : ApiController
    {
        public Task Post(double number)
        {
            return ServiceBusQueueHandler.AddWorkLoadToQueueAsync(
                    WebApiApplication.QueueClient,
                    WebApiApplication.QueueName,
                    number);
        }
    }
}
