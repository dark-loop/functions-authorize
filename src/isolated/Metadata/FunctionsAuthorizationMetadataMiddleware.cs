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
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization.Metadata
{
    /// <summary>
    /// Classifies functions based on their extension type.
    /// </summary>
    internal sealed class FunctionsAuthorizationMetadataMiddleware : IFunctionsWorkerMiddleware
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

            if (!_options.IsFunctionRegistered(context.FunctionDefinition.Name))
            {
                await RegisterHttpTriggerAuthorizationAsync(context);
            }

            context.Features.Set<IFunctionsAuthorizationFeature>(
                new FunctionsAuthorizationFeature(context.FunctionDefinition.Name));

            await next(context);
        }

        private async Task RegisterHttpTriggerAuthorizationAsync(FunctionContext context)
        {
            // Middleware can be hit concurrently, we need to ensure this functionality
            // is thread-safe on a per function basis.
            // Ensuring key is interned before entering monitor since key is compared as object
            var monitorKey = string.Intern($"famm:{context.FunctionId}");
            await KeyedMonitor.EnterAsync(monitorKey, unblockOnFirstExit: true);

            try
            {
                if (_options.IsFunctionRegistered(context.FunctionDefinition.Name))
                {
                    return;
                }

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
            finally
            {
                KeyedMonitor.Exit(monitorKey);
            }
        }
    }
}
