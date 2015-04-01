// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Web.Hosting;
using System.Web.Http;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;

namespace SynchronousIO.WebRole.Controllers
{
    public class SyncUploadController : ApiController
    {
        [HttpGet]
        public void UploadFile()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("uploadedfiles");

            container.CreateIfNotExists();

            var blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = File.OpenRead(HostingEnvironment.MapPath("~/FileToUpload.txt")))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }
    }
}
