// <copyright file="FunctionExecutingContextExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DarkLoop.Azure.Functions.Authorization
{
    internal static class FunctionExecutingContextExtensions
    {
        internal static HttpContext? GetHttpContext(this FunctionExecutingContext functionContext)
        {
            var requestOrMessage = functionContext.Arguments.Values.FirstOrDefault(x => x is HttpRequest || x is HttpRequestMessage);

            if (requestOrMessage is HttpRequest request)
            {
                return request.HttpContext;
            }
            else if (requestOrMessage is HttpRequestMessage message)
            {
                return message.Properties[nameof(HttpContext)] as HttpContext;
            }
            else
            {
                return null;
            }
        }
    }
}
