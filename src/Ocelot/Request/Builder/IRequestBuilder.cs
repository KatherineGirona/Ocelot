﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocelot.Responses;

namespace Ocelot.Request.Builder
{
    public interface IRequestBuilder
    {
        Task<Response<Request>> Build(string httpMethod,
            string downstreamUrl,
            Stream content,
            IHeaderDictionary headers,
            IRequestCookieCollection cookies,
            QueryString queryString,
            string contentType, 
            RequestId.RequestId requestId);
    }
}
