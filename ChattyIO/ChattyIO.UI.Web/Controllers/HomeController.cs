namespace ChattyIO.UI.Web.Controllers
{

    using System;
    using System.Threading.Tasks;
 
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Net.Http;
    using System.Net.Http.Headers;

    using ChattyIO.DataAccess;
    using ChattyIO.UI.Web.App_Start;

    public class HomeController : Controller
    {

        private static HttpClient httpClient = null;

        static HomeController()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Config.ApiEndPointBaseAddress);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> ChattyAsync()
        {
            var categoryResponse = await httpClient.GetAsync("chattyproduct/1");
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

            return View("Index");
        }

        public async Task<ActionResult> ChunkyAsync()
        {
            var categoryResponse = await httpClient.GetAsync("chunkyproduct/1");
            var category = await categoryResponse.Content.ReadAsAsync<ProductCategory>();
            return View("Index");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}