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
        /// Adds Functions built-in authentication.
        /// </summary>
        public static FunctionsAuthenticationBuilder AddFunctionsAuthentication(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            return services.AddFunctionsAuthentication(delegate { });
        }

        /// <summary>
        /// Configures authentication for the Azure Functions app. It will setup Functions built-in authentication.
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> for the current application.</param>
        /// <returns>A <see cref="FunctionsAuthenticationBuilder"/> instance to configure authentication schemes.</returns>
        public static FunctionsAuthenticationBuilder AddAuthentication(this IFunctionsHostBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            return builder.Services.AddFunctionsAuthentication(delegate { });
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
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            return builder.Services.AddFunctionsAuthentication(configure);
        }

        /// <summary>
        /// Configures authentication for the Azure Functions app. It will setup Functions built-in authentication.
        /// </summary>
        /// <param name="configure">The <see cref="AuthenticationOptions"/> configuration logic.</param>
        public static FunctionsAuthenticationBuilder AddFunctionsAuthentication(
            this IServiceCollection services, Action<AuthenticationOptions>? configure)
        {
            var authBuilder = new FunctionsAuthenticationBuilder(services);

            if (AuthHelper.EnableAuth)
            {
                EnabledAuthHelper.AddBuiltInFunctionsAuthentication(services);
            }
            else
            {
                services.AddAuthentication();
                AuthHelper.AddScriptJwtBearer(authBuilder);
                DisabledAuthHelper.AddScriptAuthLevel(authBuilder);
                DisabledAuthHelper.AddArmToken(authBuilder);
            }
            
            if (configure != null)
            {
                services.AddSingleton<IConfigureOptions<AuthenticationOptions>>(provider =>
                    new ConfigureOptions<AuthenticationOptions>(options =>
                    {
                        configure(options);
                    }));
            }

            return authBuilder;
        }
    }
}
