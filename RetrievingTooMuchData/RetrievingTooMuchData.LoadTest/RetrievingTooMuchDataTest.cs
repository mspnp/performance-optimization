using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Configuration;
using System.Threading.Tasks;

namespace LoadTest
{
    [TestClass]
    public class RetrievingTooMuchDataTest
    {
        private static HttpClient httpClient = null;

        static RetrievingTooMuchDataTest()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(ConfigurationManager.AppSettings["endpointbaseaddress"])
            };
        }

        [TestMethod]
        public async Task UnnecessaryRows_Sales_Total_Aggregate_On_Client_Test()
        {
            var response = await httpClient.GetAsync("unnecessaryrows/sales/total_aggregate_on_client");
        }

        [TestMethod]
        public async Task UnnecessaryRows_Sales_Total_Aggregate_On_Database_Test()
        {
            var response = await httpClient.GetAsync("unnecessaryrows/sales/total_aggregate_on_database");
        }

        [TestMethod]
        public async Task UnnecessaryFields_Products_Project_All_Fields_Test()
        {
            var response = await httpClient.GetAsync("unnecessaryfields/products/project_all_fields");
        }

        [TestMethod]
        public async Task UnnecessaryFields_Products_Project_Only_Required_Fields_Test()
        {
            var response = await httpClient.GetAsync("unnecessaryfields/products/project_only_required_fields");
        }
    }
}
