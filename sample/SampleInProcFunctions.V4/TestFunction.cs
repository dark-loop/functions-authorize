// <copyright file="TestFunction.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DarkLoop.Azure.Functions.Authorize.SampleFunctions.V4
{
    public static class TestFunction
    {
        [FunctionName("TestFunction")]
        [FunctionAuthorize]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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
