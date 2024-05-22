# functions-authorize
Bringing AuthorizeAttribute Behavior to Azure Functions v3 and v4 (In-Process)

It hooks into .NET Core dependency injection container to enable authentication and authorization in the same way  ASP.NET Core does.

> **Breaking for current package consumers**
> 
> Starting with version 4.1.0, due to security changes made on the Functions runtime, the Bearer scheme is no longer supported for your app functions.
>
> Use `AddJwtFunctionsBearer(Action<JwtBearerOptions>)` instead of `AddJwtBearer(Action<JwtBearerOptions>)` when setting up authentication.
Using `AddJwtBearer` will generate a compilation error when used against `FunctionsAuthenticationBuilder`. 
We are introducing `JwtFunctionsBearerDefaults` to refer to the suggested new custom scheme name.
>
>No changes should be required if already using a custom scheme name.

## Using the package
### Installing the package
`dotnet add package DarkLoop.Azure.Functions.Authorize`

### Setting up authentication
The goal is to utilize the same authentication framework provided for ASP.NET Core
```csharp
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
        // This is important as Bearer scheme is used by the runtime
        // and no longer supported by this framework.
        .AddJwtFunctionsBearer(options =>
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

> Starting with version 4.1.0, the default Bearer scheme is not supported by this framework.
> You can use a custom scheme or make use of `AddJwtFunctionsBearer(Action<JwtBearerOptions>)` as shown above. This one
adds the `"FunctionsBearer"` scheme. Clients still submit token for Authorization header in the format: `Bearer <token>`.


No need to register the middleware the way we do for ASP.NET Core applications.

### Using the attribute
And now lets use `FunctionAuthorizeAttribute` the same way we use `AuthorizeAttribute` in our ASP.NET Core applications.
```csharp
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
  public async Task<IActionResult> GetAllRecords(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
    ILogger log)
  {
    var records = GetAllData();
    return new OkObjectResult(records);
  }
}
```