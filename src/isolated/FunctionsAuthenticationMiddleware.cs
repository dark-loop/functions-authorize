// <copyright file="FunctionsAuthenticationMiddleware.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Features;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Authentication middleware to handle remote authentication flows.
    /// </summary>
    internal class FunctionsAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthenticationMiddleware"/> class.
        /// </summary>
        /// <param name="schemeProvider">ASPNET Core's authentication schemes provider.</param>
        public FunctionsAuthenticationMiddleware(IAuthenticationSchemeProvider schemeProvider)
        {
            _schemeProvider = schemeProvider;
        }

        /// <inheritdoc/>
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(next, nameof(next));

            var httpContext = context.GetHttpContext() ?? throw new InvalidOperationException("HttpContext has not been initialized for pipeline.");

            var feature = new FunctionsAuthenticationFeature();

            context.Features.Set<IFunctionsAuthenticationFeature>(feature);

            var handlers = context.InstanceServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await _schemeProvider.GetRequestHandlerSchemesAsync())
            {
                var handler = handlers.GetHandlerAsync(httpContext, scheme.Name) as IAuthenticationRequestHandler;
                if (handler is not null && await handler.HandleRequestAsync())
                {
                    return;
                }
            }

            var defaultAuthenticate = await _schemeProvider.GetDefaultAuthenticateSchemeAsync();
            if(defaultAuthenticate is not null)
            {
                var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Principal is not null)
                {
                    httpContext.User = result.Principal;
                }

                if (result?.Succeeded ?? false)
                {
                    feature.Result = result;
                }
            }

            await next(context);
        }
    }
}
