using System;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace ImageProcessing.Controllers
{
    [RoutePrefix("frontendimageprocessing")]
    public class FrontEndImageProcessingController : ApiController
    {
        [Route("images")]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            // Logic to upload an image
            Task.Delay(2000);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("processimage")]
        [HttpPost]
        public HttpResponseMessage ProcessImage()
        {
            new Thread(() =>
            {
                // Image processing logic
                Thread.SpinWait(Int32.MaxValue);
                Thread.SpinWait(Int32.MaxValue);
                Thread.SpinWait(Int32.MaxValue);
            }).Start();

            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        [Route("images/{imageID}/iscomplete")]
        [HttpGet]
        public bool IsImageProcessingComplete(int imageID)
        {
            // Poll to see whether processing of the specified image has finished
            Task.Delay(1000);
            return new Random().Next(100) % 5 == 0 ? true : false;
        }

        [Route("images/{imageID}")]
        [HttpGet]
        public HttpResponseMessage Get(int imageID)
        {
            // Logic to retrieve the processed image
            Task.Delay(2000);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
