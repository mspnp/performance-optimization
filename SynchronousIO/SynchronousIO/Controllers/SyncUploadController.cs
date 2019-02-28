// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Web.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SynchronousIO.WebRole.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncUploadController : ControllerBase
    {
        private readonly IHostingEnvironment environment;
        private readonly IConfiguration configuration;

        public SyncUploadController(IHostingEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }
        
        [HttpGet]
        public void UploadFile()
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("uploadedfiles");

            container.CreateIfNotExists();

            var blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var stream = CreateFile.Get())
            {
                blockBlob.UploadFromStream(stream);
            }
        }
    }
}
