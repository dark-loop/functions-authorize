// <copyright file="FunctionsAuthorizationServiceCollectionExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection
{
    // Adding this functionality to maintain compatibility with the original library
    public static class FunctionsAuthorizationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Functions built-in authorization.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static IServiceCollection AddFunctionsAuthorization(this IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            return services.AddAuthorizationCore();
        }

        /// <summary>
        /// Adds Functions built-in authorization handlers and allows for further configuration.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">The method to configure the authorization options.</param>
        public static IServiceCollection AddFunctionsAuthorization(
            this IServiceCollection services, Action<AuthorizationOptions> configure)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(configure, nameof(configure));

            return services.AddAuthorizationCore(configure);
        }

        /// <summary>
        /// Adds Functions built-in authentication.
        /// </summary>
        public static FunctionsAuthenticationBuilder AddFunctionsAuthentication(
            this IServiceCollection services, string? defaultScheme = null)
        {
            Check.NotNull(services, nameof(services));

            return services.AddFunctionsAuthentication(defaultScheme, null);
        }

        /// <summary>
        /// Configures authentication for the Azure Functions app. It will setup Functions built-in authentication.
        /// </summary>
        /// <param name="configure">The <see cref="AuthenticationOptions"/> configuration logic.</param>
        public static FunctionsAuthenticationBuilder AddFunctionsAuthentication(
            this IServiceCollection services, Action<AuthenticationOptions>? configure)
        {
            Check.NotNull(services, nameof(services));

            return services.AddFunctionsAuthentication(null, configure);
        }

        private static FunctionsAuthenticationBuilder AddFunctionsAuthentication(
            this IServiceCollection services, string? defaultScheme, Action<AuthenticationOptions>? configure)
        {
            var builder = new FunctionsAuthenticationBuilder(services);

            if (!string.IsNullOrWhiteSpace(defaultScheme))
            {
                services.AddAuthentication(defaultScheme);
            }
            else if (configure is not null)
            {
                services.AddAuthentication(configure);
            }
            else
            {
                services.AddAuthentication();
            }

            return builder;

        }
    }
}
