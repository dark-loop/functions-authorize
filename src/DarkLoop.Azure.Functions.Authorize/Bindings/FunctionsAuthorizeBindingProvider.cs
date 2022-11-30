using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;

namespace DarkLoop.Azure.Functions.Authorize.Bindings
{
    internal class FunctionsAuthorizeBindingProvider : IBindingProvider
    {
        private readonly IFunctionsAuthorizationFilterIndex _filtersIndex;

        public FunctionsAuthorizeBindingProvider(IFunctionsAuthorizationFilterIndex filterIndex)
        {
            _filtersIndex = filterIndex;
        }

        public async Task<IBinding?> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var paramType = context.Parameter.ParameterType;
            if (paramType == typeof(HttpRequest) || paramType == typeof(HttpRequestMessage))
            {
                await this.ProcessAuthorizationAsync(context.Parameter);
            }

            return null;
        }

        private Task ProcessAuthorizationAsync(ParameterInfo info)
        {
            var method = info.Member as MethodInfo;

            if (method == null) throw new InvalidOperationException($"Unable to bind authorization context for {info.Name}.");

            var cls = method.DeclaringType;

            var allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>();

            if (allowAnonymous != null) return Task.CompletedTask;

            var classAuthAttrs = cls.GetCustomAttributes<FunctionAuthorizeAttribute>();
            var methodAuthAttrs = method.GetCustomAttributes<FunctionAuthorizeAttribute>();
            var nameAttr = method.GetCustomAttribute<FunctionNameAttribute>();

            var allAuthAttributes = classAuthAttrs.Concat(methodAuthAttrs).ToList();

            if (allAuthAttributes.Count > 0)
            {
                _filtersIndex.AddAuthorizationFilter(method, nameAttr, allAuthAttributes);
            }

            return Task.CompletedTask;
        }
    }
}
