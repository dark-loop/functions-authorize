using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace SampleIsolatedFunctions.V4
{
    [FunctionAuthorize]
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger("get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var provider = req.HttpContext.RequestServices;
            var schProvider = provider.GetService<IAuthenticationSchemeProvider>();

            var sb = new StringBuilder();

            if (schProvider is not null)
            {
                foreach (var scheme in await schProvider.GetAllSchemesAsync())
                    sb.AppendLine($"{scheme.Name} -> {scheme.HandlerType}");
            }

            sb.AppendLine();
            sb.AppendLine(Assembly.GetEntryAssembly()!.FullName);

            return new OkObjectResult(sb.ToString());

        }
    }
}
