// <copyright file="FunctionsAuthenticationBuilderExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Linq;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="FunctionsAuthenticationBuilder"/>
    /// </summary>
    public static class FunctionsAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the JWT Bearer scheme to the authentication configuration. JWT is added by default to Azure Functions 
        /// and all HTTP functions are applied the Admin level after a token is validated.
        /// </summary>
        /// <param name="builder">The current authentication builder.</param>
        /// <param name="removeBuiltInConfig">A value indicating whether remove the built-in configuration for JWT.
        /// Bearer scheme is still in place, but Admin level is not set for incoming requests.
        /// <para>When setting this value to <c>true</c> (default) all existing configuration will be removed.</para></param>
        /// <returns>A instance of the <see cref="FunctionsAuthenticationBuilder"/></returns>
        public static FunctionsAuthenticationBuilder AddJwtBearer(
            this FunctionsAuthenticationBuilder builder, bool removeBuiltInConfig = true)
        {
            return builder.AddJwtBearer(delegate { }, removeBuiltInConfig);
        }

        /// <summary>
        /// Adds the JWT Bearer scheme to the authentication configuration. JWT is added by default to Azure Functions 
        /// and all HTTP functions are applied the Admin level after a token is validated.
        /// </summary>
        /// <param name="builder">The current authentication builder.</param>
        /// <param name="configureOptions">An action configuring the JWT options for authentication. 
        /// <para>When <paramref name="removeBuiltInConfig"/> is set to <see langword="false" />, it enhances the built-in configuration for the scheme</para></param>
        /// <param name="removeBuiltInConfig">A value indicating whether remove the built-in configuration for JWT.
        /// Bearer scheme is still in place, but Admin level is not set incoming requests.
        /// <para>When setting this value to <see langword="true"/> (default) all existing configuration will be removed.</para></param>
        /// <returns>A instance of the <see cref="FunctionsAuthenticationBuilder"/></returns>
        public static FunctionsAuthenticationBuilder AddJwtBearer(
            this FunctionsAuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions, bool removeBuiltInConfig = true)
        {
            if (removeBuiltInConfig)
            {
                var descriptors = builder.Services
                    .Where(s => s.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    var instance = descriptor?.ImplementationInstance as ConfigureNamedOptions<JwtBearerOptions>;

                    if (instance?.Name == "Bearer")
                    {
                        builder.Services.Remove(descriptor!);
                    }
                }
            }

            builder.Services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure(configureOptions);

            return builder;
        }
    }
}
