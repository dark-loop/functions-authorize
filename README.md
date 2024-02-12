# functions-authorize
Bringing AuthorizeAttribute Behavior to Azure Functions v3 and v4 (In-Process)

It hooks into .NET Core dependency injection container to enable authentication and authorization in the same way  ASP.NET Core does.

## License
This projects is open source and may be redistributed under the terms of the [Apache 2.0](http://opensource.org/licenses/Apache-2.0) license.

## Using the package
### Installing the package
`dotnet add package DarkLoop.Azure.Functions.Authorize`

### Setting up authentication
The goal is to utilize the same authentication framework provided for ASP.NET Core
```c#
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using MyFunctionAppNamespace;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyFunctionAppNamespace
{
  class Startup : FunctionsStartup
  {
    public void Configure(IFunctionsHostBuilder builder)
    {
      builder
        .AddAuthentication(options =>
        {
          options.DefaultAuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddOpenIdConnect(options =>
        {
          options.ClientId = "<my-client-id>";
          // ... more options here
        })
        .AddJwtBearer(options =>
        {
          options.Audience = "<my-audience>";
          // ... more options here
        });

      builder
        .AddAuthorization(options =>
        {
          options.AddPolicy("OnlyAdmins", policyBuilder =>
          {
            // configure my policy requirements
          });
        });
    }
  }
}
```

No need to register the middleware the way we do for ASP.NET Core applications.

### Using the attribute
And now lets use `FunctionAuthorizeAttribute` the same way we use `AuthorizeAttribute` in our ASP.NET Core applications.
```C#
public class Functions
{
  [FunctionAuthorize]
  [FunctionName("get-record")]
  public async Task<IActionResult> GetRecord(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
    ILogger log)
  {
    var user = req.HttpContext.User;
    var record = GetUserData(user.Identity.Name);
    return new OkObjectResult(record);
  }

  [FunctionAuthorize(Policy = "OnlyAdmins")]
  [FunctionName("get-all-records")]
  public async Task<IActionResult>(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
    ILogger log)
  {
    var records = GetAllData();
    return new OkObjectResult(records);
  }
}
```
##

### Releases
[![Nuget](https://img.shields.io/nuget/v/DarkLoop.Azure.Functions.Authorize.svg)](https://www.nuget.org/packages/DarkLoop.Azure.Functions.Authorize)

### Builds
![master build status](https://dev.azure.com/darkloop/DarkLoop%20Core%20Library/_apis/build/status/Open%20Source/Functions%20Authorize%20-%20Pack?branchName=master)

## Change log
Adding change log starting with version 3.1.3

### 3.1.3
- #### Support for disabling `FunctionAuthorize` effect at the application level.
  Adding support for disabling the effect of `[FunctionAuthorize]` attribute at the application level.  
  This is useful when wanting to disable authorization for a specific environment, such as local development.

  When configuring services, you can now configure `FunctionsAuthorizationOptions`.
  ```c#
  builder.Services.Configure<FunctionsAuthorizationOptions>(options => 
      options.DisableAuthorization = Configuration.GetValue<bool>("AuthOptions:DisableAuthorization"));
  ```

  Optionally you can bind it to configuration to rely on providers like User Secrets or Azure App Configuration to disable and re-enable without having to restart your application:
  ```c#
  builder.Services.Configure<FunctionsAuthorizationOptions>(
      Configuration.GetSection("FunctionsAuthorization"));
  ```

  For function apps targeting .NET 7 or greater, you can also use `AuthorizationBuilder` to set this value:
  ```c#
  builder.Services
      .AddAuthorizationBuilder()
      .DisableAuthorization(Configuration.GetValue<bool>("AuthOptions:DisableAuthorization"));
  ```

  It's always recommended to encapsulate this logic within checks for environments to ensure that if the configuration setting is unintentionally moved to a non-desired environment, it would not affect security of our HTTP triggered functions. This change adds a helper method to identify if you are running the function app in the local environment:
  ```c#
  if (builder.IsLocalAuthorizationContext())
  {
      builder.Services.Configure<FunctionsAuthorizationOptions>(
          options => options.AuthorizationDisabled = true);
  }
  ```

  If you want to output warnings emitted by the library remember to set the log level to `Warning` or lower for `Darkloop` category in your `host.json` file:

  ```json
  {
    "logging": {
      "logLevel": {
        "DarkLoop": "Warning"
      }
    }
  }
  ```
  
  Thanks to [BenjaminWang1031](https://github.com/BenjaminWang1031) for the suggestion to add this functionality.

- #### Remove Functions bult-in JwtBearer configuration by default
  Azure Functions recently [added configuration](https://github.com/Azure/azure-functions-host/pull/9678) for issuer and audience validation for the default authentication flows, not the one supported by this package through `FunctionAuthorizeAttribute`, which interferes with token validation when using our own Bearer scheme token configuration.
  In prior versions, this package has functionality to clear Functions built-in configuration, but it was not enabled by default when using `AddJwtBearer(Action<JwtBearerOptions> configure, bool removeBuiltInConfig = false)`. Since the use of this package is commonly used for custom JWT token, the default value of `removeBuiltInConfig` is now `true`.