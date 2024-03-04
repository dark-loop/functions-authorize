// <copyright file="FunctionsAuthorizationHandler.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Net;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization.Policy;
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
        private readonly IPolicyEvaluator _policyEvaluator;
        private readonly IOptionsMonitor<FunctionsAuthorizationOptions> _configOptions;
        private readonly FunctionsAuthorizationOptions _options;
        private readonly ILogger<FunctionsAuthorizationExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationExecutor"/> class.
        /// </summary>
        /// <param name="authorizationProvider">Functions authorization filters provider</param>
        /// <param name="authorizationHandler">An authorization handler</param>
        /// <param name="policyEvaluator">The policy evaluator</param>
        public FunctionsAuthorizationExecutor(
            IFunctionsAuthorizationProvider authorizationProvider,
            IFunctionsAuthorizationResultHandler authorizationHandler,
            IPolicyEvaluator policyEvaluator,
            IOptionsMonitor<FunctionsAuthorizationOptions> configOptions,
            IOptions<FunctionsAuthorizationOptions> options,
            ILogger<FunctionsAuthorizationExecutor> logger)
        {
            Check.NotNull(authorizationProvider, nameof(authorizationProvider));
            Check.NotNull(authorizationHandler, nameof(authorizationHandler));
            Check.NotNull(policyEvaluator, nameof(policyEvaluator));
            Check.NotNull(configOptions, nameof(configOptions));
            Check.NotNull(logger, nameof(logger));

            _authorizationProvider = authorizationProvider;
            _authorizationHandler = authorizationHandler;
            _policyEvaluator = policyEvaluator;
            _configOptions = configOptions;
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task ExecuteAuthorizationAsync(FunctionExecutingContext context)
        {
            Check.NotNull(context, nameof(context));

            var httpContext = context.GetHttpContext()!;

            if (this._configOptions.CurrentValue.AuthorizationDisabled)
            {
                _logger.LogWarning(
                    $"Authorization through FunctionAuthorizeAttribute is disabled at the application level. Skipping authorization for {httpContext.Request.GetDisplayUrl()}.");

                return;
            }

            var filter = await _authorizationProvider.GetAuthorizationAsync(context.FunctionName);

            if (httpContext.Items.ContainsKey(Constants.AuthInvokedKey) ||
                filter?.Policy is null)
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

            await _authorizationHandler.HandleResultAsync(authContext, httpContext);

            if (!authorizeResult.Succeeded)
            {
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
        private static void BombFunctionInstanceAsync(HttpStatusCode status)
        {
            throw new FunctionAuthorizationException(status);
        }
    }
}
