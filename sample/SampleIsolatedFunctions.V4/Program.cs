// <copyright file="Program.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using Common.Tests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

// IMPORTANT: because local.settings.json is not included in the repository, you must create it manually
// If you don't create it. the isolated function will not run. Ensure that the file has the following content:
//
// {
//   "IsEncrypted": false,
//   "Values": {
//     "AzureWebJobsStorage": "UseDevelopmentStorage=true",
//     "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
//   }
// }

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseFunctionsAuthorization();
    })
    .ConfigureServices(services =>
    {
        services
            .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            });

        services
            .AddFunctionsAuthorization(options =>
            {
                // Add your policies here
            });
    })
    .Build();

host.Run();
