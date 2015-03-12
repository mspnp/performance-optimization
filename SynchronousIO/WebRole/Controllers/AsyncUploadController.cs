using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace WebRole.Controllers
{
    public class AsyncUploadController : ApiController
    {
        [HttpGet]
        public async Task UploadFileAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("uploadedfiles");
            
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var blockBlob = container.GetBlockBlobReference("myblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = File.OpenRead(HostingEnvironment.MapPath("~/FileToUpload.txt")))
            {
                await blockBlob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
            }
        }
    }
}
