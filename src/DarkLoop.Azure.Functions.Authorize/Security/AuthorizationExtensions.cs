using System;
using System.Collections.Generic;
using System.Text;
using DarkLoop.Azure.Functions.Authorize.Bindings;
using DarkLoop.Azure.Functions.Authorize.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationExtensions
    {
        public static IFunctionsHostBuilder AddAuthorization(this IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddAuthorization();
            return builder;
        }

        public static IFunctionsHostBuilder AddAuthorization(this IFunctionsHostBuilder builder, Action<AuthorizationOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.Services.Configure(configure);
            return builder.AddAuthorization();
        }
    }
}
