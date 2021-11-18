using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorize
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Obsolete("This class is dependent on Azure Functions preview features.")]
    public class FunctionAuthorizeAttribute : FunctionInvocationFilterAttribute, IFunctionInvocationFilter, IAuthorizeData
    {
        public FunctionAuthorizeAttribute() { }

        public FunctionAuthorizeAttribute(string policy)
        {
            this.Policy = policy;
        }

        public string? Policy { get; set; }

        public string? Roles { get; set; }

        public string? AuthenticationSchemes { get; set; }

        async Task IFunctionInvocationFilter.OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if (!this.IsProcessed(executingContext))
            {
                var httpContext = this.GetHttpContext(executingContext);
                if (httpContext != null)
                {
                    await AuthorizeRequestAsync(executingContext, httpContext);
                }
            }
        }

        private bool IsProcessed(FunctionExecutingContext context)
        {
            const string valueKey = "__AuthZProcessed";

            if (!context.Properties.TryGetValue(valueKey, out var value))
            {
                context.Properties[valueKey] = true;
                return false;
            }

            return (bool)value;
        }

        private HttpContext? GetHttpContext(FunctionExecutingContext context)
        {
            var requestOrMessage = context.Arguments.Values.FirstOrDefault(x => x is HttpRequest || x is HttpRequestMessage);

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

        private async Task AuthorizeRequestAsync(FunctionExecutingContext functionContext, HttpContext httpContext)
        {
            var handler = httpContext.RequestServices.GetRequiredService<IFunctionsHttpAuthorizationHandler>();
            await handler.OnAuthorizingFunctionInstance(functionContext, httpContext);
        }
    }
}
