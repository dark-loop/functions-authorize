// <copyright file="Startup.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Common.Tests;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorize.SampleFunctions.V4;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                    // this line is here to bypass the token validation
                    // and test the functionality of this library.
                    // you can create a dummy token by executing the GetTestToken function in HelperFunctions.cs
                    // THE FOLLOWING LINE SHOULD BE REMOVED IN A REAL-WORLD SCENARIO
                    options.SecurityTokenValidators.Add(new TestTokenValidator());

                    // this is what you should look for in a real-world scenario
                    // comment the lines if you cloned this repository and want to test the library
                    options.Authority = "https://login.microsoftonline.com/<your-tenant>";
                    options.Audience = "<your-audience>";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };
                }, true);

            builder.Services.AddFunctionsAuthorization(options =>
            {
                // Add your policies here
            });

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
