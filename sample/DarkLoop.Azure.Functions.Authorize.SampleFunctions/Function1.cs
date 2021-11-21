using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DarkLoop.Azure.Functions.Authorize.SampleFunctions
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

