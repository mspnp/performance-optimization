using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BusyDatabase.WebApi.Controllers
{
    public static class HttpResponseHelper
    {
        public static HttpResponseMessage CreateMessageFrom(string result)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var mediaType = new MediaTypeHeaderValue("application/xml");
            mediaType.Parameters.Add(new NameValueHeaderValue("charset", "utf-8"));
            response.Content = new StringContent(result);
            response.Content.Headers.ContentType = mediaType;
            return response;
        }
    }
}