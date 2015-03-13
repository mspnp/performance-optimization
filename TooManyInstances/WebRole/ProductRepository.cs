using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebRole.Models;

namespace WebRole
{
    /// <summary>
    /// This ProductRepository represents a class that is meant to be long lived. 
    /// The same instance is meant to be share by the classes that depend on it.
    /// </summary>
    public class ProductRepository : IProductRepository, IDisposable
    {
        private bool _disposed;
        private readonly HttpClient _httpClient;

        public ProductRepository()
        {
            //Simulate delay due to setup and configuration of ProductRepository
            Thread.Sleep(100);

            //HttpClient is thread-safe in that you can submit many requests concurrently.
            //HttpClient instances should be shared, otherwise you could run out of TCP sockets.
            _httpClient = new HttpClient();
        }

        public async Task<Product> GetProductByIdAsync(string productId)
        {
            //Opportunity to look for product in cache.

            var hostName = HttpContext.Current.Request.Url.Host;
            var result = await _httpClient.GetStringAsync(string.Format("http://{0}:8080", hostName)).ConfigureAwait(false);
            
            //opportunity to save result to cache

            return new Product { Name = result };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _httpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
