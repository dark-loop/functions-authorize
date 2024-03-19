// <copyright file="FunctionsAuthorizeBindingProvider.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization.Bindings
{
    internal class FunctionsAuthorizeBindingProvider : IBindingProvider
    {
        private readonly FunctionsAuthorizationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizeBindingProvider"/> class.
        /// </summary>
        /// <param name="options">The options object.</param>
        public FunctionsAuthorizeBindingProvider(
            IOptions<FunctionsAuthorizationOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<IBinding?> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var paramType = context.Parameter.ParameterType;
            if (paramType == typeof(HttpRequest) || paramType == typeof(HttpRequestMessage))
            {
                this.ProcessAuthorization(context.Parameter);
            }

            return Task.FromResult<IBinding?>(null);
        }

        private void ProcessAuthorization(ParameterInfo info)
        {
            var method = info.Member as MethodInfo ??
                throw new InvalidOperationException($"Unable to bind authorization context for {info.Name}.");

            var declaringType = method.DeclaringType!;
            var nameAttr = method.GetCustomAttribute<FunctionNameAttribute>()!;

            _options.RegisterFunctionAuthorizationAttributesMetadata<FunctionAuthorizeAttribute>(
                nameAttr.Name, declaringType, method);
        }
    }
}
