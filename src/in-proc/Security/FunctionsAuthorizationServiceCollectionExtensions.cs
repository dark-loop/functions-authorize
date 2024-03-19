// <copyright file="AuthorizationExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using DarkLoop.Azure.Functions.Authorization.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> to add Functions built-in authorization.
    /// </summary>
    public static class FunctionsAuthorizationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Functions built-in authorization.
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
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">The method to configure the authorization options.</param>
        public static IServiceCollection AddFunctionsAuthorization(
            this IServiceCollection services, Action<AuthorizationOptions> configure)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            HostUtils.AddFunctionsBuiltInAuthorization(services);
            services.Configure(configure);

            return services;
        }
    }
}
