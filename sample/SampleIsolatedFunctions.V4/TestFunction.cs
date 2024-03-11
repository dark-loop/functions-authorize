// <copyright file="TestFunction.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Text;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleIsolatedFunctions.V4
{
    [FunctionAuthorize(AuthenticationSchemes = "Bearer")]
    public class TestFunction
    {
        private readonly ILogger<TestFunction> _logger;

        public TestFunction(ILogger<TestFunction> logger)
        {
            _logger = logger;
        }

        [Function("TestFunction")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Run([HttpTrigger("get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var provider = req.HttpContext.RequestServices;
            var schProvider = provider.GetService<IAuthenticationSchemeProvider>();

            var sb = new StringBuilder();
            sb.AppendLine("Authentication schemes:");

            if (schProvider is not null)
            {
                foreach (var scheme in await schProvider.GetAllSchemesAsync())
                    sb.AppendLine($"  {scheme.Name} -> {scheme.HandlerType}");
            }

            sb.AppendLine();
            sb.AppendLine($"User:");
            sb.AppendLine($"  Name  -> {req.HttpContext.User.Identity!.Name}");
            sb.AppendLine($"  Email -> {req.HttpContext.User.FindFirst("email")?.Value}");

            return new OkObjectResult(sb.ToString());
        }
    }
}
