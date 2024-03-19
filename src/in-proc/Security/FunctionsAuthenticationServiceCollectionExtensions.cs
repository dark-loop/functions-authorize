// <copyright file="AuthenticationExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Internal;
using DarkLoop.Azure.Functions.Authorization.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding authentication services to the DI container.
    /// </summary>
    public static class FunctionsAuthenticationServiceCollectionExtensions
    {
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
        /// <param name="services">The current service collection.</param>
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
            var authBuilder = new FunctionsAuthenticationBuilder(services);

            if (HostUtils.IsLocalDevelopment)
            {
                if (!string.IsNullOrWhiteSpace(defaultScheme))
                {
                    services.AddAuthentication(defaultScheme!);
                }
                else
                {
                    services.AddAuthentication();
                }

                LocalHostUtils.AddScriptJwtBearer(authBuilder);
                LocalHostUtils.AddScriptAuthLevel(authBuilder);
                LocalHostUtils.AddArmToken(authBuilder);
            }
            else
            {
                HostUtils.AddFunctionsBuiltInAuthentication(services);
            }

            if (configure is not null)
            {
                services.Configure(configure);
            }

            return authBuilder;
        }
    }
}
