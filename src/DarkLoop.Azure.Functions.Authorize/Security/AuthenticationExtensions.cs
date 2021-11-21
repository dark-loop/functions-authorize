using System;
using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Configures authentication for the Azure Functions app. It will setup Functions built-in authentication.
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> for the current application.</param>
        /// <returns>A <see cref="FunctionsAuthenticationBuilder"/> instance to configure authentication schemes.</returns>
        /// <exception cref="ArgumentNullException">When builder is null.</exception>
        public static FunctionsAuthenticationBuilder AddAuthentication(this IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddAuthentication(delegate { });
        }

        /// <summary>
        /// Configures authentication for the Azure Functions app. It will setup Functions built-in authentication.
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> for the current application.</param>
        /// <param name="configure">The <see cref="AuthenticationOptions"/> configuration logic.</param>
        /// <returns>A <see cref="FunctionsAuthenticationBuilder"/> instance to configure authentication schemes.</returns>
        /// <exception cref="ArgumentNullException">When builder is null.</exception>
        public static FunctionsAuthenticationBuilder AddAuthentication(
            this IFunctionsHostBuilder builder, Action<AuthenticationOptions>? configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure != null)
            {
                builder.Services.AddSingleton<IConfigureOptions<AuthenticationOptions>>(provider =>
                    new ConfigureOptions<AuthenticationOptions>(options =>
                    {
                        configure(options);
                    }));
            }

            builder.Services
                .AddAuthentication();
         
            var authBuilder = new FunctionsAuthenticationBuilder(builder);

            if (AuthHelper.EnableAuth)
            {
                EnabledAuthHelper.AddArmToken(authBuilder);
                EnabledAuthHelper.AddScriptAuthLevel(authBuilder);
                AuthHelper.AddScriptJwtBearer(authBuilder);
            }
            else
            {
                AuthHelper.AddScriptJwtBearer(authBuilder);
                DisabledAuthHelper.AddScriptAuthLevel(authBuilder);
                DisabledAuthHelper.AddArmToken(authBuilder);
            }
            
            return authBuilder;
        }
    }
}
