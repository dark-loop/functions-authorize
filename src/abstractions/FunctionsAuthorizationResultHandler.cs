// <copyright file="FunctionsAuthorizationResultHandler.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <inheritdoc cref="IFunctionsAuthorizationResultHandler"/>
    internal sealed class FunctionsAuthorizationResultHandler : IFunctionsAuthorizationResultHandler
    {
        private readonly IOptionsMonitor<FunctionsAuthorizationOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationResultHandler"/> class.
        /// </summary>
        /// <param name="monitoredOptions">The framework options.</param>
        public FunctionsAuthorizationResultHandler(
            IOptionsMonitor<FunctionsAuthorizationOptions> monitoredOptions)
        {
            _options = monitoredOptions;
        }

        private FunctionsAuthorizationOptions Options => _options.CurrentValue;

        /// <inheritdoc />
        public async Task HandleResultAsync<TContext>(
            FunctionAuthorizationContext<TContext> context,
            HttpContext httpContext,
            Func<TContext, Task>? onSuccess = null)
            where TContext : class
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(httpContext, nameof(httpContext));
            
            if (context.Result.Succeeded)
            {
                if (onSuccess is not null)
                {
                    await onSuccess(context.UnderlyingContext);
                }
                
                return;
            }

            if (context.Result.Challenged)
            {
                if (context.Policy.AuthenticationSchemes.Count > 0)
                {
                    foreach (var scheme in context.Policy.AuthenticationSchemes)
                    {
                        await httpContext.ChallengeAsync(scheme);
                    }
                }
                else
                {
                    await httpContext.ChallengeAsync();
                }
            }
            else if (context.Result.Forbidden)
            {
                if (context.Policy.AuthenticationSchemes.Count > 0)
                {
                    foreach (var scheme in context.Policy.AuthenticationSchemes)
                    {
                        await httpContext.ForbidAsync(scheme);
                    }
                }
                else
                {
                    await httpContext.ForbidAsync();
                }
            }

            await HandleFailureAsync(httpContext, context.Result);
        }

        // Writing default results for forbidden and challenged
        private async Task HandleFailureAsync(HttpContext context, PolicyAuthorizationResult result)
        {
            if (Options.WriteHttpStatusToResponse && !context.Response.HasStarted)
            {
                var httpResult = default(IActionResult);

                if (result.Forbidden)
                {
                    httpResult = new AuthorizationFailureResult(HttpStatusCode.Forbidden);
                }
                else if (result.Challenged)
                {
                    httpResult = new AuthorizationFailureResult(HttpStatusCode.Unauthorized);
                }

                await httpResult!.ExecuteResultAsync(new ActionContext(context, context.GetRouteData(), new ActionDescriptor()));
            }
        }

        private class AuthorizationFailureResult : ObjectResult
        {
            public AuthorizationFailureResult(HttpStatusCode statusCode) : base(statusCode.ToString())
            {
            }
        }
    }
}
