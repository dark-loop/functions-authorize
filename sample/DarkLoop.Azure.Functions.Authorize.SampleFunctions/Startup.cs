using DarkLoop.Azure.Functions.Authorize.SampleFunctions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(Startup))]

namespace DarkLoop.Azure.Functions.Authorize.SampleFunctions
{
    class Startup : FunctionsStartup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder
                .AddAuthentication(options=>
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
                            var response = x.Response;
                            response.ContentType = "text/plain";
                            response.ContentLength = 5;
                            await response.WriteAsync("No go");
                            await response.Body.FlushAsync();
                        },
                        OnChallenge = async x =>
                        {
                            // un-commenting the following lines would override what the internals do to send an unauthorized response
                            //var response = x.Response;
                            //response.ContentType = "text/plain";
                            //response.ContentLength = 5;
                            //response.StatusCode = 401;
                            //await response.WriteAsync("No go");
                            //await response.Body.FlushAsync();
                        }
                    };
                });

            builder.AddAuthorization();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder.AddUserSecrets<Startup>();

            Configuration = builder.ConfigurationBuilder.Build();

            base.ConfigureAppConfiguration(builder);
        }
    }
}
