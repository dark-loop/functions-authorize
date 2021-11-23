using System;
using System.Collections.Generic;
using System.Text;
using DarkLoop.Azure.Functions.Authorize.Bindings;
using DarkLoop.Azure.Functions.Authorize.Filters;
using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Adds Functions built-in authorization.
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> containing the services collection to configure.</param>
        public static IFunctionsHostBuilder AddAuthorization(this IFunctionsHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddFunctionsAuthorization(delegate { });
            
            return builder;
        }

        /// <summary>
        /// Adds Functions built-in auhotrization.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static IServiceCollection AddFunctionsAuthorization(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            return services.AddFunctionsAuthorization(delegate { });
        }

        /// <summary>
        /// Adds Functions built-in authorization handlers and allows for further configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> containing the services collection to configure.</param>
        /// <param name="configure">The method to configure the authorization options.</param>
        public static IFunctionsHostBuilder AddAuthorization(this IFunctionsHostBuilder builder, Action<AuthorizationOptions> configure)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddFunctionsAuthorization(configure);

            return builder;
        }

        /// <summary>
        /// Adds Function built-in authorization handlers and allows for further configuration.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">The method to configure the authorization options.</param>
        public static IServiceCollection AddFunctionsAuthorization(
            this IServiceCollection services, Action<AuthorizationOptions> configure)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            AuthHelper.AddFunctionsBuiltInAuthorization(services);
            services.Configure(configure);

            return services;
        }
    }
}
