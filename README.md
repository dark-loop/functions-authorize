# functions-authorize
Extension bringing AuthorizeAttribute Behavior to Azure Functions In-Proc and Isolated mode. For the latter is only available with ASPNET Core integration.

It hooks into .NET Core dependency injection container to enable authentication and authorization in the same way  ASP.NET Core does.

## Getting Started
- [Azure Functions V3+ In-Proc mode](./src/in-proc/README.md)
- [Azure Functions V4 Isolated mode with ASPNET Core integration](./src/isolated/README.md)

## License
This projects is open source and may be redistributed under the terms of the [Apache 2.0](http://opensource.org/licenses/Apache-2.0) license.

## Package Status
### Releases
[![Nuget](https://img.shields.io/nuget/v/DarkLoop.Azure.Functions.Authorization.Abstractions.svg)](https://www.nuget.org/packages/DarkLoop.Azure.Functions.Authorization.Abstractions)

### Builds
![master build status](https://dev.azure.com/darkloop/DarkLoop%20Core%20Library/_apis/build/status/Open%20Source/Functions%20Authorize%20-%20Pack?branchName=master)

## Change log
Adding change log starting with version 3.1.3

### 4.0.1
Deprecating `DarkLoop.Azure.Functions.Authorize` package in favor of `DarkLoop.Azure.Functions.Authorization.InProcess` package.<br/>
The functionality remains the same, it's just a way to keep package naming in sync.

### 4.0.0
Starting from 4.0.0, support for Azure Functions V4 Isolated mode with ASPNET Core integration is added.
The package is now split into two separate packages, one for each mode. 

The package for Azure Functions V3+ In-Proc mode is now called `DarkLoop.Azure.Functions.Authorization.InProcess` and the package for Azure Functions V4 Isolated mode with ASPNET Core integration is called `DarkLoop.Azure.Functions.Authorize.Isolated`.

- #### .NET 6 support
  Starting with version 4.0.0, the package is now targeting .NET 6.0. This means that the package is no longer compatible with .NET 5 or lower. If you are using .NET 5 or lower, you should use version 3.1.3 of the package.
  
- #### DarkLoop.Azure.Functions.Authorize v4.0.0
  This package is published but is now deprecated in favor of `DarkLoop.Azure.Functions.Authorization.InProcess`. All it's functionality remains the same. It's just a way to keep package naming in sync.

- #### Introducing IFunctionsAuthorizationProvider interface
  The `IFunctionsAuthorizationProvider` interface is introduced to allow for custom authorization filter provisioning to the framework.
  By default the framework relies on decorating the function or type with `[FunctionAuthorize]`. You could skip this decoration and provide the middleware with an authorization filter sourced from your own mechanism, for example a database.
  At this moment this can be done only with Isolated mode even when the interface is defined in the shared package.<br/>
  Support for In-Process will be added in a future version, once source generators are introduced, as the in-process framework relies on Invocation Filters to enable authorization.
  Replacing the service in the application services would break the authorization for in-process mode at this point.

### 3.1.3
3.1.3 and lower versions only support Azure Functions V3 In-Proc mode. Starting from 4.0.0, support for Azure Functions V4 Isolated mode with ASPNET Core integration is added.
- #### Support for disabling `FunctionAuthorize` effect at the application level.
  Adding support for disabling the effect of `[FunctionAuthorize]` attribute at the application level.  
  This is useful when wanting to disable authorization for a specific environment, such as local development.

  When configuring services, you can now configure `FunctionsAuthorizationOptions`.
  ```csharp
  builder.Services.Configure<FunctionsAuthorizationOptions>(options => 
      options.DisableAuthorization = Configuration.GetValue<bool>("AuthOptions:DisableAuthorization"));
  ```

  Optionally you can bind it to configuration to rely on providers like User Secrets or Azure App Configuration to disable and re-enable without having to restart your application:
  ```csharp
  builder.Services.Configure<FunctionsAuthorizationOptions>(
      Configuration.GetSection("FunctionsAuthorization"));
  ```

  For function apps targeting .NET 7 or greater, you can also use `AuthorizationBuilder` to set this value:
  ```csharp
  builder.Services
      .AddAuthorizationBuilder()
      .DisableAuthorization(Configuration.GetValue<bool>("AuthOptions:DisableAuthorization"));
  ```

  It's always recommended to encapsulate this logic within checks for environments to ensure that if the configuration setting is unintentionally moved to a non-desired environment, it would not affect security of our HTTP triggered functions. This change adds a helper method to identify if you are running the function app in the local environment:
  ```csharp
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