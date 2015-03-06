﻿using System;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Common.Logic;

namespace ImageProcessing.Controllers
{
    [RoutePrefix("backgroundimageprocessing")]
    public class BackgroundProcessingController : ApiController
    {
        private const string ServiceBusConnectionString = "Microsoft.ServiceBus.ConnectionString";
        private const string AppSettingKeyServiceBusQueueName = "Microsoft.ServiceBus.QueueName";

        private readonly QueueClient QueueClient;
        private readonly string QueueName;

        public BackgroundProcessingController()
        {
            var serviceBusConnectionString = CloudConfigurationManager.GetSetting(ServiceBusConnectionString);
            QueueName = CloudConfigurationManager.GetSetting(AppSettingKeyServiceBusQueueName);
            QueueClient = ServiceBusQueueHandler.GetQueueClientAsync(serviceBusConnectionString, QueueName).Result;
        }

        [Route("images")]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            // Logic to upload an image
            Thread.Sleep(2000);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("processimage")]
        [HttpPost]
        public HttpResponseMessage ProcessImage()
        {
            ServiceBusQueueHandler.AddWorkLoadToQueueAsync(QueueClient, QueueName, "Image information as a string");
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        [Route("images/{imageID}/iscomplete")]
        [HttpGet]
        public bool IsImageProcessingComplete(int imageID)
        {
            // Poll to see whether processing of the specified image has finished
            Thread.Sleep(1000);
            return new Random().Next(100) % 5 == 0 ? true : false;
        }

        [Route("images/{imageID}")]
        [HttpGet]
        public HttpResponseMessage Get(int imageID)
        {
            // Logic to retrieve the processed image
            Thread.Sleep(2000);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}