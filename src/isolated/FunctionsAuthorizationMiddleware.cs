// <copyright file="AuthorizationMiddleware.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Internal;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <inheritdoc cref="IFunctionsWorkerMiddleware"/>
    internal sealed class FunctionsAuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IFunctionsAuthorizationProvider _authorizationProvider;
        private readonly IFunctionsAuthorizationResultHandler _authorizationResultHandler;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly IPolicyEvaluator _policyEvaluator;
        private readonly IOptionsMonitor<FunctionsAuthorizationOptions> _configOptions;
        private readonly ILogger<FunctionsAuthorizationMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationMiddleware"/> class.
        /// </summary>
        /// <param name="authorizationProvider">Functions authorization provider to retrieve filters.</param>
        /// <param name="authorizationHandler">Authorization handler.</param>
        /// <param name="policyProvider">ASP.NET Core's authorization policy provider.</param>
        /// <param name="policyEvaluator">ASP.NET Core's policy evaluator.</param>
        /// <param name="configOptions">Functions authorization configure options.</param>
        /// <param name="logger">A logger object for diagnostics.</param>
        public FunctionsAuthorizationMiddleware(
            IFunctionsAuthorizationProvider authorizationProvider,
            IFunctionsAuthorizationResultHandler authorizationHandler,
            IAuthorizationPolicyProvider policyProvider,
            IPolicyEvaluator policyEvaluator,
            IOptionsMonitor<FunctionsAuthorizationOptions> configOptions,
            ILogger<FunctionsAuthorizationMiddleware> logger)
        {
            Check.NotNull(authorizationProvider, nameof(authorizationProvider));
            Check.NotNull(authorizationHandler, nameof(authorizationHandler));
            Check.NotNull(policyProvider, nameof(policyProvider));
            Check.NotNull(policyEvaluator, nameof(policyEvaluator));
            Check.NotNull(configOptions, nameof(configOptions));
            Check.NotNull(logger, nameof(logger));

            _authorizationProvider = authorizationProvider;
            _authorizationResultHandler = authorizationHandler;
            _policyProvider = policyProvider;
            _policyEvaluator = policyEvaluator;
            _configOptions = configOptions;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpContext = context.GetHttpContext() ?? throw new NotSupportedException(IsolatedMessages.NotSupportedIsolatedMode);

            if (this._configOptions.CurrentValue.AuthorizationDisabled)
            {
                var displayUrl = httpContext.Request.GetDisplayUrl();

                _logger.LogWarning(IsolatedMessages.FunctionAuthIsDisabled, displayUrl);

                await next(context);
                return;
            }

            var filter = await _authorizationProvider.GetAuthorizationAsync(context.FunctionDefinition.Name, _policyProvider);

            if (filter.Policy is null)
            {
                await next(context);
                return;
            }

            var authenticateResult = await _policyEvaluator.AuthenticateAsync(filter.Policy, httpContext);

            if (filter.AllowAnonymous)
            {
                await next(context);
                return;
            }

            if (authenticateResult is not null && !authenticateResult.Succeeded)
            {
                _logger.LogDebug(
                    IsolatedMessages.AuthenticationFailed,
                    filter.Policy.AuthenticationSchemes.Count > 0
                        ? " for " + string.Join(", ", filter.Policy.AuthenticationSchemes)
                        : string.Empty);
            }

            var authorizeResult = await _policyEvaluator.AuthorizeAsync(filter.Policy, authenticateResult!, httpContext, httpContext);
            var authContext = new FunctionAuthorizationContext<FunctionContext>(
                context.FunctionDefinition.Name, context, filter.Policy, authorizeResult);

            await _authorizationResultHandler.HandleResultAsync(authContext, httpContext, async (ctx) => await next(ctx));
        }
    }
}
