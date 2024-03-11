// <copyright file="FunctionAuthorizeAttribute.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorize
{
    /// <summary>
    /// Represents authorization logic that needs to be applied to a function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class FunctionAuthorizeAttribute : FunctionInvocationFilterAttribute, IFunctionInvocationFilter, IAuthorizeData
    {
        public FunctionAuthorizeAttribute() { }

        public FunctionAuthorizeAttribute(string policy)
        {
            this.Policy = policy;
        }

        /// <summary>
        /// Gets or sets the name of the authorization policy to apply to function.
        /// </summary>
        public string? Policy { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of roles that are required to execute function.
        /// </summary>
        public string? Roles { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of authentication schemes that are required to apply the authorization logic.
        /// </summary>
        public string? AuthenticationSchemes { get; set; }

        async Task IFunctionInvocationFilter.OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if (!this.IsProcessed(executingContext))
            {
                var httpContext = executingContext.GetHttpContext();
                if (httpContext is not null)
                {
                    var services = httpContext.RequestServices;
                    var authorizationExecutor = services.GetRequiredService<IFunctionsAuthorizationExecutor>();
                    
                    await authorizationExecutor.ExecuteAuthorizationAsync(executingContext, httpContext);
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
    }
}
