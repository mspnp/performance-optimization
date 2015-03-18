// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ChattyIO.DataAccess.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChattyIO.Tests
{
    [TestClass]
    public class ChattyProductListPriceHistoryPerfFixture
    {
        private static readonly HttpClient HttpClient;
        
        static ChattyProductListPriceHistoryPerfFixture()
        {
            HttpClient = new HttpClient {BaseAddress = new Uri(ConfigurationManager.AppSettings["endpointbaseaddress"])};
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [TestMethod]
        public async Task WhenUsingChattyAPI_ToGet_AllProductListPriceHistory_ForCategories()
        {
            for (int i = 1; i < 5; i++)
            {
                var subCategoryResponse = await HttpClient.GetAsync("chattyproduct/products/" + i);
                var subCategory = await subCategoryResponse.Content.ReadAsAsync<ProductSubcategory>();
            }
        }

        [TestMethod]
        public async Task WhenUsingChunkyAPI_ToGet_AllProductListPriceHistory_ForCategories()
        {
            for (int i = 1; i < 5; i++)
            {
                var subCategoryResponse = await HttpClient.GetAsync("chunkyproduct/products/" + i);
                var subCategory = await subCategoryResponse.Content.ReadAsAsync<ProductSubcategory>();
            }
        }
    }

}
