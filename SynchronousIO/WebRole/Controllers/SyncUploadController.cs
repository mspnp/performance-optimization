using System.IO;
using System.Web.Hosting;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace WebRole.Controllers
{
    public class SyncUploadController : ApiController
    {
        public void Get()
        {
            UploadFile();
        }

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
