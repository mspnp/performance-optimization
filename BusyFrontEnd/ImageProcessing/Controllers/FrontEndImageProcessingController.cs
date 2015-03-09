using System;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Microsoft.ApplicationInsights;

namespace ImageProcessing.Controllers
{
    [RoutePrefix("frontendimageprocessing")]
    public class FrontEndImageProcessingController : ApiController
    {
        private TelemetryClient telemetry = new TelemetryClient();
        
        [Route("images")]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            try
            {                
                // Logic to upload an image
                Thread.SpinWait(Int32.MaxValue / 1000);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("processimage")]
        [HttpPost]
        public HttpResponseMessage ProcessImage()
        {
            new Thread(() =>
            {
                try
                {
                    // Image processing logic
                    Thread.SpinWait(Int32.MaxValue);
                    Thread.SpinWait(Int32.MaxValue);

                }
                catch (Exception ex)
                {
                    telemetry.TrackException(ex);
                }
            }).Start();

            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        [Route("images/{imageID}/iscomplete")]
        [HttpGet]
        public bool IsImageProcessingComplete(int imageID)
        {
            try
            {
                // Poll to see whether processing of the specified image has finished
                Thread.SpinWait(Int32.MaxValue / 2000);
                
                return new Random().Next(100) % 5 == 0 ? true : false;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return true;
            }
        }

        [Route("images/{imageID}")]
        [HttpGet]
        public HttpResponseMessage Get(int imageID)
        {
            try
            {
                // Logic to retrieve the processed image
                Thread.SpinWait(Int32.MaxValue / 1000);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
