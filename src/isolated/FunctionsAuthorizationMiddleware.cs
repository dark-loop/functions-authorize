// <copyright file="AuthorizationMiddleware.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization
{
    internal class FunctionsAuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IFunctionsAuthorizationProvider _authorizationProvider;
        private readonly IPolicyEvaluator _policyEvaluator;
        private readonly IFunctionsAuthorizationResultHandler _authorizationResultHandler;
        private readonly IOptionsMonitor<FunctionsAuthorizationOptions> _configOptions;
        private readonly ILogger<FunctionsAuthorizationMiddleware> _logger;

        public FunctionsAuthorizationMiddleware(
            IFunctionsAuthorizationProvider authorizationProvider,
            IPolicyEvaluator policyEvaluator,
            IFunctionsAuthorizationResultHandler authorizationHandler,
            IOptionsMonitor<FunctionsAuthorizationOptions> configOptions,
            ILogger<FunctionsAuthorizationMiddleware> logger)
        {
            Check.NotNull(authorizationProvider, nameof(authorizationProvider));
            Check.NotNull(policyEvaluator, nameof(policyEvaluator));
            Check.NotNull(authorizationHandler, nameof(authorizationHandler));
            Check.NotNull(configOptions, nameof(configOptions));
            Check.NotNull(logger, nameof(logger));

            _authorizationProvider = authorizationProvider;
            _policyEvaluator = policyEvaluator;
            _authorizationResultHandler = authorizationHandler;
            _configOptions = configOptions;
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpContext = context.GetHttpContext() ??
                throw new NotSupportedException("DarkLoop Functions authorization is only supported in Isolated mode with ASPNET Core integration.");

            if (this._configOptions.CurrentValue.AuthorizationDisabled)
            {
                _logger.LogWarning(
                    $"Authorization through FunctionAuthorizeAttribute is disabled at the application level. Skipping authorization for {httpContext!.Request.GetDisplayUrl()}.");

                await next(context);
                return;
            }

            var filter = await _authorizationProvider.GetAuthorizationAsync(context.FunctionDefinition.Name);

            if (filter?.Policy is null)
            {
                await next(context);
                return;
            }

            var authenticateResult = await _policyEvaluator.AuthenticateAsync(filter.Policy, httpContext);

            if (!filter.AllowAnonymous)
            {
                var authorizeResult = await _policyEvaluator.AuthorizeAsync(filter.Policy, authenticateResult, httpContext, httpContext);
                var authContext = new FunctionAuthorizationContext<FunctionContext>(
                    context.FunctionDefinition.Name, context, filter.Policy, authorizeResult);

                await _authorizationResultHandler.HandleResultAsync(authContext, httpContext, async (ctx) => await next(ctx));
            }
        }
    }
}
