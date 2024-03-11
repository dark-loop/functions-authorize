// <copyright file="FunctionsAuthorizationMetadataMiddleware.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Extensions;
using DarkLoop.Azure.Functions.Authorization.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization.Metadata
{
    /// <summary>
    /// Classifies functions based on their extension type.
    /// </summary>
    internal class FunctionsAuthorizationMetadataMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly FunctionsAuthorizationOptions _options;
        private readonly ConcurrentDictionary<string, bool> _trackedHttp = new();

        public FunctionsAuthorizationMetadataMiddleware(
            IOptions<FunctionsAuthorizationOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (!_trackedHttp.GetOrAdd(context.FunctionId, static (_, c) => c.IsHttpTrigger(), context))
            {
                await next(context);
                return;
            }

            if(!_options.IsFunctionRegistered(context.FunctionDefinition.Name))
            {
                RegisterHttpTriggerAuthorization(context);
            }

            context.Features.Set<IFunctionsAuthorizationFeature>(
                new FunctionsAuthorizationFeature(context.FunctionDefinition.Name));

            await next(context);
        }

        private void RegisterHttpTriggerAuthorization(FunctionContext context)
        {
            var functionName = context.FunctionDefinition.Name;
            var declaringTypeName = context.FunctionDefinition.EntryPoint.LastIndexOf('.') switch
            {
                -1 => string.Empty,
                var index => context.FunctionDefinition.EntryPoint[..index]
            };

            var methodName = context.FunctionDefinition.EntryPoint[(declaringTypeName.Length + 1)..];
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var method = assemblies.Select(a => a.GetType(declaringTypeName, throwOnError: false))
                .FirstOrDefault(t => t is not null)?
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) ??
                throw new MethodAccessException(
                    $"Method instance for function '{context.FunctionDefinition.Name}' " +
                    $"cannot be found or cannot be accessed due to its protection level.");

            var declaringType = method.DeclaringType!;

            _options.RegisterFunctionAuthorizationAttributesMetadata<AuthorizeAttribute>(functionName, declaringType, method);
        }
    }
}
