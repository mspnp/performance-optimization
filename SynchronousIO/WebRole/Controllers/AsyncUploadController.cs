using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WebRole.Controllers
{
    public class AsyncUploadController : ApiController
    {
        public Task Get()
        {
            return UploadFileAsync();
        }

        public async Task UploadFileAsync()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("uploadedfiles");
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = File.OpenRead(HttpContext.Current.Server.MapPath(@"../FileToUpload.txt")))
            {
                await blockBlob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
            }
        }
    }
}
