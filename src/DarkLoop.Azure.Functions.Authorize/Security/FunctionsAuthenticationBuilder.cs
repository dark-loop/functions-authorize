using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    /// <summary>
    /// An <see cref="AuthenticationBuilder"/> that enhances the built-in authentication behavior for Azure Functions.
    /// </summary>
    public class FunctionsAuthenticationBuilder : AuthenticationBuilder
    {
        internal FunctionsAuthenticationBuilder(IServiceCollection services)
            : base(services) { }

        /// <summary>
        /// Adds the JWT Bearer scheme to the authentication configuration. JWT is added by default to Azure Functions 
        /// and all HTTP functions are applied the Admin level after a token is validated.
        /// </summary>
        /// <param name="removeBuiltInConfig">A value indicating whether remove the built-in configuration for JWT.
        /// Bearer scheme is still in place, but Admin level is not set incoming requests.</param>
        /// <returns>A instance of the <see cref="FunctionsAuthenticationBuilder"/></returns>
        public FunctionsAuthenticationBuilder AddJwtBearer(bool removeBuiltInConfig = false)
        {
            return this.AddJwtBearer(delegate { }, removeBuiltInConfig);
        }

        /// <summary>
        /// Adds the JWT Bearer scheme to the authentication configuration. JWT is added by default to Azure Functions 
        /// and all HTTP functions are applied the Admin level after a token is validated.
        /// </summary>
        /// <param name="configureOptions">An action configuring the JWT options for authentication. 
        /// <para>When <see cref="removeBuiltInConfig"/> is set to false, it enhances the built-in configuration for the scheme</para></param>
        /// <param name="removeBuiltInConfig">A value indicating whether remove the built-in configuration for JWT.
        /// Bearer scheme is still in place, but Admin level is not set incoming requests.</param>
        /// <returns>A instance of the <see cref="FunctionsAuthenticationBuilder"/></returns>
        public FunctionsAuthenticationBuilder AddJwtBearer(Action<JwtBearerOptions> configureOptions, bool removeBuiltInConfig = false)
        {
            if(removeBuiltInConfig)
            {
                var descriptor = Services.FirstOrDefault(s => s.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>));
                Services.Remove(descriptor);
            }

            this.Services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure(configureOptions);

            return this;
        }
    }
}
