// <copyright file="Startup.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorize.SampleFunctions.V4;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace DarkLoop.Azure.Functions.Authorize.SampleFunctions.V4
{
    class Startup : FunctionsStartup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddFunctionsAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"{Configuration["AzureAD:Instance"]}{Configuration["AzureAD:TenantId"]}";
                    options.Audience = Configuration["AzureAD:ApiIdUrl"];
                    options.Challenge = $"Bearer realm=\"\", authorization_uri=\"{Configuration["AzureAD:Instance"]}{Configuration["AzureAD:TenantId"]}/oauth2/authorize\", client_id=\"{Configuration["AzureAD:ApiClientId"]}\"";

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = async x =>
                        {
                            var body = "Unauthorized request";
                            var response = x.Response;
                            response.StatusCode = 401;
                            response.ContentType = "text/plain";
                            response.ContentLength = body.Length;
                            await response.WriteAsync(body);
                            await response.Body.FlushAsync();
                        },
                        OnChallenge = async x =>
                        {
                            // un-commenting the following lines would override what the internals do to send an unauthorized response
                            var response = x.Response;
                            response.StatusCode = 401;
                            response.ContentType = "text/plain";
                            response.ContentLength = 5;
                            await response.WriteAsync("No go");
                            await response.Body.FlushAsync();
                        }
                    };
                }, true);

            builder.Services.AddFunctionsAuthorization();

            // If you want to disable authorization for all functions
            // decorated with FunctionAuthorizeAttribute you can add the following configuration.
            // If you bind it to configuration, you can modify the setting remotely using
            // Azure App Configuration or other configuration providers without the need to restart app.
            if (builder.IsLocalAuthorizationContext())
            {
                builder.Services.Configure<FunctionsAuthorizationOptions>(Configuration.GetSection("AuthOptions"));
            }
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder.AddUserSecrets<Startup>(false, reloadOnChange: true);

            Configuration = builder.ConfigurationBuilder.Build();

            base.ConfigureAppConfiguration(builder);
        }
    }
}
