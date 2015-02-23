using Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Wait for the webserver to be ready when running locally.
            Thread.Sleep(1000);

            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:25921/");

            var sw = new Stopwatch();

            Console.WriteLine("Example 1: Get all products with all fields.");
            sw.Start();
            GetAllProductsWithAllFieldsAsync(client).Wait();
            sw.Stop();
            Console.WriteLine("Example 1: Finished after {0} ms.", sw.ElapsedMilliseconds);
            
            sw.Reset();

            Console.WriteLine("Example 2: Getting all products with required fields only.");
            sw.Start();
            RetrieveAllProductsBySubcategory(client).Wait();
            sw.Stop();
            Console.WriteLine("Example 2: Finished after {0} ms.", sw.ElapsedMilliseconds);

            Console.ReadKey();
        }

        static async Task GetAllProductsWithAllFieldsAsync(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync("products/wrong");
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadAsAsync<List<ProductInfo>>();
        }

        static async Task RetrieveAllProductsBySubcategory(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync("products/right");
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadAsAsync<List<ProductInfo>>();
        }
    }
}
