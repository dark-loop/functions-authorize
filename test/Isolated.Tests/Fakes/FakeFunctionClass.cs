// <copyright file="FunctionsAuthorizationMetadataMiddlewareTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Isolated.Tests.Fakes
{
    [AllowAnonymous]
    public class FakeFunctionClass
    {
        [Function("TestFunction")]
        [FunctionAuthorize]
        public IActionResult TestFunction([HttpTrigger("get")] HttpRequest request)
        {
            return new OkResult();
        }

        [Function("TestFunction2")]
        [FunctionAuthorize]
        internal IActionResult TestFunction2([HttpTrigger("get")] HttpRequest request)
        {
            return new OkResult();
        }
    }
}
