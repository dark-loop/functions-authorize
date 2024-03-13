# functions-authorization-isolated
Bringing AuthorizeAttribute Behavior to Azure Functions v4 in Isolated mode.

It hooks into .NET Core dependency injection container to enable authentication and authorization in the same way  ASP.NET Core does.

## Using the package
### Installing the package
`dotnet add package DarkLoop.Azure.Functions.Authorization.Isolated`

### Setting up authentication and authorization
The goal is to utilize the same authentication framework provided for ASP.NET Core
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebAppliction(builder =>
    {
        // Explicitly adding the extension middleware because
        // registering middleware when extension is loaded does not
        // place the middleware in the pipeline where required request
        // information is available.
        builder.UseFunctionsAuthorization();
    })
    .ConfigureServices(services =>
    {
        services
            .AddFunctionsAuthenticationation(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://login.microsoftonline.com/your-tenant-id";
                options.Audience = "your-client-id";
                ...
            });

        services.AddFunctionsAuthorization(options =>
        {
            options.AddPolicy("OnlyAdmins", policy => policy.RequireRole("Admin"));
        });

        // Add other services
    })
    .Build();

host.Run();
```

Notice the call to `UseFunctionsAuthorization` in the `ConfigureFunctionsWebAppliction` method. 
This is required to ensure that the middleware is placed in the pipeline where required function information is available.`

### Using the attribute
And now lets use `FunctionAuthorizeAttribute` the same way we use `AuthorizeAttribute` in our ASP.NET Core applications.
```csharp
[FunctionAuthorize]
public class Functions
{
  [FunctionName("get-record")]
  public async Task<IActionResult> GetRecord(
    [HttpTrigger("get")] HttpRequest req, ILogger log)
  {
    var user = req.HttpContext.User;
    var record = GetUserData(user.Identity.Name);
    return new OkObjectResult(record);
  }

  [Authorize(Policy = "OnlyAdmins")]
  [FunctionName("get-all-records")]
  public async Task<IActionResult> GetAllRecords(
    [HttpTrigger("get")] HttpRequest req, ILogger log)
  {
    var records = GetAllData();
    return new OkObjectResult(records);
  }
}
```

Something really nice to notice is that for Functions in Isolated mode, the `HttpTriggerAttribute` default `AuthenticationLevel` is `Anonymous`, playing really well with the attribute.<br/>
Also notice how the second function uses the `AuthorizeAttribute` attribute to apply a policy to the function. `FunctionAuthorizeAttribute` was left as part of the framework only to make it easier to migrate from In-Proc to Isolated, but they can be used interchangeably.
