using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DarkLoop.Azure.Functions.Authorize;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Darkloop.Azure.Functions.Authorize.SampleFunctions.V4
{
    public static class TestFunction
    {
        [FunctionName("TestFunction")]
        [FunctionAuthorize]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var provider = req.HttpContext.RequestServices;
            var schProvider = provider.GetService<IAuthenticationSchemeProvider>();

            var sb = new StringBuilder();
            foreach (var scheme in await schProvider.GetAllSchemesAsync())
                sb.AppendLine($"{scheme.Name} -> {scheme.HandlerType}");

            sb.AppendLine();
            sb.AppendLine(Assembly.GetEntryAssembly().FullName);

            return new OkObjectResult(sb.ToString());
        }
    }
}
