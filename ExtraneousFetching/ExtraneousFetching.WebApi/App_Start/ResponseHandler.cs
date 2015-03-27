// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RetrievingTooMuchData.WebApi
{
    public class ResponseHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.Content != null)
            {
                await response.Content.LoadIntoBufferAsync();
                var contentLength = response.Content.Headers.ContentLength;

                // TODO: Observe the http response size using the monitoring system of your choice.

            }

            return response;
        }
    }
}