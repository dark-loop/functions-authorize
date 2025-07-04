// <copyright file="FunctionsAuthenticationMiddleware.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Internal;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization;

internal sealed class FunctionsAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<FunctionsAuthenticationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionsAuthenticationMiddleware"/> class.
    /// </summary>
    /// <param name="logger">A logger object for diagnostics.</param>
    public FunctionsAuthenticationMiddleware(
        ILogger<FunctionsAuthenticationMiddleware> logger)
    {
        Check.NotNull(logger, nameof(logger));

        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext() ?? throw new NotSupportedException(IsolatedMessages.NotSupportedIsolatedMode);

        var schemes = context.InstanceServices.GetRequiredService<IAuthenticationSchemeProvider>();
        var handlers = context.InstanceServices.GetRequiredService<IAuthenticationHandlerProvider>();
        foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
        {
            var handler = await handlers.GetHandlerAsync(httpContext, scheme.Name) as IAuthenticationRequestHandler;
            if (handler != null && await handler.HandleRequestAsync())
            {
                return;
            }
        }

        var defaultAuthenticate = await schemes.GetDefaultAuthenticateSchemeAsync();
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
                var allSchemes = (await schemes.GetAllSchemesAsync()).ToList();
                _logger.LogDebug(
                    IsolatedMessages.AuthenticationFailed,
                    allSchemes.Count > 0
                    ? " for " + string.Join(", ", allSchemes)
                    : string.Empty);
            }
        }

        await next(context);
    }

}