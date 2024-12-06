module Program

open Common.Tests
open DarkLoop.Azure.Functions.Authorization
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.IdentityModel.Tokens

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

let host = 
    HostBuilder()
        .ConfigureFunctionsWebApplication(fun builder -> builder.UseFunctionsAuthorization() |> ignore )
        .ConfigureServices(fun services ->    
            services
                .AddFunctionsAuthentication(JwtFunctionsBearerDefaults.AuthenticationScheme)
                .AddJwtFunctionsBearer(fun options ->
                    // this line is here to bypass the token validation
                    // and test the functionality of this library.
                    // you can create a dummy token by executing the GetTestToken function in HelperFunctions.cs
                    // THE FOLLOWING LINE SHOULD BE REMOVED IN A REAL-WORLD SCENARIO
                    options.SecurityTokenValidators.Add(TestTokenValidator())

                    // this is what you should look for in a real-world scenario
                    // comment the lines if you cloned this repository and want to test the library
                    options.Authority <- "https://login.microsoftonline.com/<your-tenant>"
                    options.Audience <- "<your-audience>"
                    options.TokenValidationParameters <- TokenValidationParameters
                        (
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true
                        )
                    ()
                ) |> ignore

            services
                .AddFunctionsAuthorization(fun options ->
                    // Add your policies here
                    ()
                ) |> ignore
        )
        .Build()

host.Run()
