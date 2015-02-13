namespace ChattyIO.Tests
{

    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using ChattyIO.DataAccess;
    
    [TestClass]
    public class ChattyProductListPriceHistoryPerfFixture
    {
        private static HttpClient httpClient = null;
        private TestContext testContextInstance;
        static ChattyProductListPriceHistoryPerfFixture()
        {
           
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["endpointbaseaddress"]);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        [TestMethod]
      public async Task WhenUsingChattyAPI_ToGet_AllProductListPriceHistory_ForCategories()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 1; i < 5; i++)
            {
                var categoryResponse = await httpClient.GetAsync("chattyproduct/"+ i.ToString());
                var category = await categoryResponse.Content.ReadAsAsync<ProductCategory>();

                var subCategoriesResponse =
                    await httpClient.GetAsync("chattyproduct/productsubcategories/" + category.ProductCategoryId);
                var subCategories = await subCategoriesResponse.Content.ReadAsAsync<IEnumerable<ProductSubcategory>>();

                foreach (var subCategory in subCategories)
                {
                    var productsResponse =
                        await httpClient.GetAsync("chattyproduct/products/" + subCategory.ProductSubcategoryId);
                    var products = await productsResponse.Content.ReadAsAsync<IEnumerable<Product>>();

                    foreach (var product in products)
                    {
                        var productListPriceResponse =
                            await httpClient.GetAsync("chattyproduct/productlistpricehistory/" + product.ProductId);
                        var productListPrices =
                            await productListPriceResponse.Content.ReadAsAsync<IEnumerable<ProductListPriceHistory>>();
                        productListPrices.ToList().ForEach((plp) => product.ProductListPriceHistory.Add((plp)));
                    }
                }
            }
            stopWatch.Stop();
            Console.WriteLine("Chatty-Time {0}",stopWatch.ElapsedMilliseconds);
        }

        [TestMethod]
        [AspNetDevelopmentServer("ChattyIO", "ChattyIO.Web")]
        public async Task WhenUsingChunkyAPI_ToGet_AllProductListPriceHistory_ForCategories()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 1; i < 5; i++)
            {
                var categoryResponse = await httpClient.GetAsync("chunkyproduct/" + i.ToString());
                var category = await categoryResponse.Content.ReadAsAsync<ProductCategory>();
            }
            stopWatch.Stop();
         Console.WriteLine("Chunky-Time {0}", stopWatch.ElapsedMilliseconds);

        }

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
    }
    
}
