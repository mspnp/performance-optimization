using System.Web;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("uploadedfiles");
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(HttpContext.Current.Server.MapPath(@"../FileToUpload.txt")))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }
    }
}
