// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SynchronousIO.WebRole.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsyncUploadController : ControllerBase
    {
        private readonly IHostingEnvironment environment;
        private readonly IConfiguration configuration;

        public AsyncUploadController(IHostingEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task UploadFileAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("uploadedfiles");
            
            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var stream = new CreateFile().Get())
            {
                await blockBlob.UploadFromStreamAsync(stream);
            }
        }
    }
}
