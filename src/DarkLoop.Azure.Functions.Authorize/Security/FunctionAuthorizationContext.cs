using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class FunctionAuthorizationContext
    {
        public FunctionAuthorizationContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public HttpContext HttpContext { get; }

        public IActionResult Result { get; internal set; }
    }
}
