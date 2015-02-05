using System;
using System.Net.Http;
using System.Threading;
using WebRole.Models;

namespace WebRole
{
    public class ProductRepository : IProductRepository, IDisposable
    {
        private bool disposed = false;
        private HttpClient httpClient;

        public ProductRepository()
        {
            //Simulate delay due to setup and configuration of ProductRepository
            Thread.Sleep(500);

            httpClient = new HttpClient();
        }

        public Product GetProductById(string productId)
        {
            //Opportunity to look for product in cache.

            //var result = httpClient.GetStringAsync("http://www.microsoft.com");
            
            //opportunity to save result to cache

            return new Product(){Name = "Bicycle"};
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                httpClient.Dispose();
            }

            disposed = true;
        }
    }
}