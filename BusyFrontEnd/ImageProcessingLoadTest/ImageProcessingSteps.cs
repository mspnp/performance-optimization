namespace ImageProcessingLoadTest
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading;
    using System.Web;
    using System.Collections.Specialized;


    [TestClass]
    public class ImageProcessingSteps
    {
        private static HttpClient httpClient = null;
        private static string uriRoot = null;

        static ImageProcessingSteps()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["endpointbaseaddress"]);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            uriRoot = ConfigurationManager.AppSettings["uriroot"];
        }

        [TestMethod]
        public async Task ClientSteps()
        {
            var uploadResponse = await httpClient.PostAsync(uriRoot + "/images", null);
            Thread.Sleep(2000);
            var processResponse = await httpClient.PostAsync(uriRoot + "/processimage", null);

            HttpResponseMessage isReadyResponse = null;
            string response;
            do
            {
                Thread.Sleep(3000);
                isReadyResponse = await httpClient.GetAsync(uriRoot + "/images/99/iscomplete");
                response = await isReadyResponse.Content.ReadAsStringAsync();
            }
            while (response != "true");

            var downloadResponse = await httpClient.GetAsync(uriRoot + "/images/99");
            Thread.Sleep(2000);
        }

        [TestMethod]
        public async Task UploadImage()
        {
            var uploadResponse = await httpClient.PostAsync(uriRoot + "/images", null);
        }

        [TestMethod]
        public async Task ProcessImage()
        {
            var processResponse = await httpClient.PostAsync(uriRoot + "/processimage", null);
        }

        [TestMethod]
        public async Task WaitForImageProcessing()
        {
            var isReadyResponse = await httpClient.GetAsync(uriRoot + "/images/99/iscomplete");
        }

        [TestMethod]
        public async Task DownloadImage()
        {
            var downloadResponse = await httpClient.GetAsync(uriRoot + "/images/99");
        }
    }
}
