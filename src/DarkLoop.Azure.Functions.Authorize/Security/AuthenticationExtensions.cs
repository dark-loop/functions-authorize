using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationExtensions
    {
        private const string AuthLevelClaimType = "http://schemas.microsoft.com/2017/07/functions/claims/authlevel";

        public static AuthenticationBuilder AddAuthentication(this IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddAuthentication(null);
        }

        public static AuthenticationBuilder AddAuthentication(
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

            return builder.Services
                .AddAuthentication()
                .AddScriptFunctionsJwtBearer();
        }

        private static AuthenticationBuilder AddScriptFunctionsJwtBearer(this AuthenticationBuilder builder)
        {
            return builder.AddJwtBearer(Constants.WebJobsAuthScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = c =>
                    {
                        options.TokenValidationParameters = CreateTokenValidationParameters();
                        return Task.CompletedTask;
                    },

                    OnTokenValidated = c =>
                    {
                        c.Principal.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(AuthLevelClaimType, AuthorizationLevel.Admin.ToString()) }));
                        c.Success();
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = CreateTokenValidationParameters();
            });

            TokenValidationParameters CreateTokenValidationParameters()
            {
                var defaultKey = "2d3a0617-f369-492c-ab7a-f21ec1631376";
                var result = new TokenValidationParameters();

                if (defaultKey != null)
                {
                    result.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(defaultKey));
                    result.ValidateAudience = true;
                    result.ValidateIssuer = true;
                    result.ValidAudience = string.Format("https://{0}.azurewebsites.net/azurefunctions", "func");
                    result.ValidIssuer = string.Format("https://{0}.scm.azurewebsites.net", "func");
                }

                return result;
            }
        }
    }
}
