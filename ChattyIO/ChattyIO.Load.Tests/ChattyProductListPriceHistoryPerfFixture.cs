namespace ChattyIO.Load.Tests
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
            
            for (int i = 1; i < 5; i++)
            {
               
                    var subCategoryResponse =
                        await httpClient.GetAsync("chattyproduct/products/" + i.ToString());
                    var subCategory = await subCategoryResponse.Content.ReadAsAsync<ProductSubcategory>();
            }
        }

        [TestMethod]
        public async Task WhenUsingChunkyAPI_ToGet_AllProductListPriceHistory_ForCategories()
        {
            for (int i = 1; i < 5; i++)
            {
                var subCategoryResponse = await httpClient.GetAsync("chunkyproduct/products/" + i.ToString());
                var subCategory = await subCategoryResponse.Content.ReadAsAsync<ProductSubcategory>();
            }
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
