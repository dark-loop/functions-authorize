# functions-authorize
Bringing AuthorizeAttribute Behavior to Azure Functions v3 and v4 (In-Process)

It hooks into .NET Core dependency injection container to enable authentication and authorization in the same way  ASP.NET Core does.

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