// <copyright file="FunctionsAuthorizationHandler.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Net;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Internal;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <inheritdoc cref="IFunctionsAuthorizationExecutor"/>
    internal sealed class FunctionsAuthorizationExecutor : IFunctionsAuthorizationExecutor
    {
        private readonly IFunctionsAuthorizationProvider _authorizationProvider;
        private readonly IFunctionsAuthorizationResultHandler _authorizationHandler;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly IPolicyEvaluator _policyEvaluator;
        private readonly IOptionsMonitor<FunctionsAuthorizationOptions> _configOptions;
        private readonly ILogger<FunctionsAuthorizationExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationExecutor"/> class.
        /// </summary>
        /// <param name="authorizationProvider">Functions authorization filters provider.</param>
        /// <param name="authorizationHandler">An authorization handler.</param>
        /// <param name="policyProvider">ASP.NET Core's policy provider.</param>
        /// <param name="policyEvaluator">The policy evaluator.</param>
        /// <param name="configOptions">The options object to control behavior base on settings that might be modified in config source.</param>
        /// <param name="logger">The logger for the type.</param>
        public FunctionsAuthorizationExecutor(
            IFunctionsAuthorizationProvider authorizationProvider,
            IFunctionsAuthorizationResultHandler authorizationHandler,
            IAuthorizationPolicyProvider policyProvider,
            IPolicyEvaluator policyEvaluator,
            IOptionsMonitor<FunctionsAuthorizationOptions> configOptions,
            ILogger<FunctionsAuthorizationExecutor> logger)
        {
            Check.NotNull(authorizationProvider, nameof(authorizationProvider));
            Check.NotNull(authorizationHandler, nameof(authorizationHandler));
            Check.NotNull(policyProvider, nameof(policyProvider));
            Check.NotNull(policyEvaluator, nameof(policyEvaluator));
            Check.NotNull(configOptions, nameof(configOptions));
            Check.NotNull(logger, nameof(logger));

            _authorizationProvider = authorizationProvider;
            _authorizationHandler = authorizationHandler;
            _policyProvider = policyProvider;
            _policyEvaluator = policyEvaluator;
            _configOptions = configOptions;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task ExecuteAuthorizationAsync(FunctionExecutingContext context, HttpContext httpContext)
        {
            Check.NotNull(context, nameof(context));

            if (this._configOptions.CurrentValue.AuthorizationDisabled)
            {
                _logger.LogWarning(Strings.DisabledAuthorization, httpContext.Request.GetDisplayUrl());

                return;
            }

            var filter = await _authorizationProvider.GetAuthorizationAsync(context.FunctionName, _policyProvider);

            if (filter.Policy is null)
            {
                return;
            }

            var authenticateResult = await _policyEvaluator.AuthenticateAsync(filter.Policy, httpContext);

            // still authenticating in case token is sent to set context user but skipping authorization
            if (filter.AllowAnonymous)
            {
                return;
            }

            var authorizeResult = await _policyEvaluator.AuthorizeAsync(filter.Policy, authenticateResult, httpContext, httpContext);
            var authContext = new FunctionAuthorizationContextInternal(
                    context.FunctionName, httpContext, filter.Policy, authorizeResult);

            var completed = false;

            // need to know if the response body was completed by handling failure
            httpContext.Response.OnCompleted(async () => completed = await Task.FromResult(true));
            await _authorizationHandler.HandleResultAsync(authContext, httpContext);

            // As this is only executed through an invocation filter,
            // we need to throw an exception to stop the function execution.
            // By now the caller has already received an unauthorized or forbidden response.
            if (!authorizeResult.Succeeded)
            {
                // in case the response was not completed by the handler, we need to complete it before throwing the exception
                // throwing the exception without completing will send a 500 to user
                if (!completed)
                {
                    await httpContext.Response.CompleteAsync();
                }

                if (authorizeResult.Challenged)
                {
                    BombFunctionInstanceAsync(HttpStatusCode.Unauthorized);
                }
                else if (authorizeResult.Forbidden)
                {
                    BombFunctionInstanceAsync(HttpStatusCode.Forbidden);
                }
            }
        }

        /// <summary>
        /// Writes a failure message to response if nothing was written by framework handling functionality.
        /// </summary>
        /// <param name="status">The status to report back to caller.</param>
        /// <returns></returns>
        /// <exception cref="FunctionAuthorizationException"></exception>
        private void BombFunctionInstanceAsync(HttpStatusCode status)
        {
            _logger.LogDebug("Short-circuiting function execution due to authorization failure.");
            throw new FunctionAuthorizationException(status);
        }
    }
}
