// <copyright file="FunctionsAuthenticationMiddleware.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Internal;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization
{
    internal sealed class FunctionsAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        #region Private Fields

        private readonly IAuthenticationHandlerProvider _authenticationHandlerProvider;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly ILogger<FunctionsAuthenticationMiddleware> _logger;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthenticationMiddleware"/> class.
        /// </summary>
        /// <param name="authenticationHandlerProvider">authentication handler provider.</param>
        /// <param name="authenticationSchemeProvider">authentication scheme provider.</param>
        /// <param name="logger">A logger object for diagnostics.</param>
        public FunctionsAuthenticationMiddleware(
            IAuthenticationHandlerProvider authenticationHandlerProvider,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            ILogger<FunctionsAuthenticationMiddleware> logger)
        {
            Check.NotNull(authenticationHandlerProvider, nameof(authenticationHandlerProvider));
            Check.NotNull(authenticationSchemeProvider, nameof(authenticationSchemeProvider));
            Check.NotNull(logger, nameof(logger));

            _authenticationHandlerProvider = authenticationHandlerProvider;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _logger = logger;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <inheritdoc />
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpContext = context.GetHttpContext() ??
                              throw new NotSupportedException(IsolatedMessages.NotSupportedIsolatedMode);

            foreach (var scheme in await _authenticationSchemeProvider.GetRequestHandlerSchemesAsync())
            {
                var handler =
                    await _authenticationHandlerProvider.GetHandlerAsync(httpContext, scheme.Name) as
                        IAuthenticationRequestHandler;
                if (handler != null && await handler.HandleRequestAsync())
                {
                    return;
                }
            }

            var defaultAuthenticate = await _authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Principal != null)
                {
                    httpContext.User = result.Principal;
                }

                if (result?.Succeeded ?? false)
                {
                    var authFeatures = httpContext.Features.SetAuthenticationFeatures(result);
                    context.Features.Set<IHttpAuthenticationFeature>(authFeatures);
                    context.Features.Set<IAuthenticateResultFeature>(authFeatures);
                }
                else
                {
                    var allSchemes = (await _authenticationSchemeProvider.GetAllSchemesAsync()).ToList();
                    _logger.LogDebug(
                        IsolatedMessages.AuthenticationFailed,
                        allSchemes.Count > 0
                            ? " for " + string.Join(", ", allSchemes)
                            : string.Empty);
                }
            }

            await next(context);
        }

        #endregion Public Methods
    }
}