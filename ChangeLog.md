# Change log
Change log stars with version 3.1.3

## 4.2.1
- Adding check for `FunctionNameAttribute` when executing binding for InProcess mode. There are situations where other functions can be injected that are not decorated with attribute and also execute parameter binding.

## 4.2.0
- Removing support for STS .NET versions (7.0), as these versions are not supported by Azure Functions runtime.
- Removing ARM authentication support to align with Azure Functions runtime changes.
- Aligning IdentityModel packages versions with versions specified in the *Script.WebHost** project.

## 4.1.3
- Adding support for specifying a scheme in `AddJwtFunctionsBearer` method as an overload. It throws exception if 'Bearer' is used as a scheme name.

## 4.1.2
- **[Bug Fix]** The main change in this version is ensuring metadata collection middleware is thread safe when no metadata has been built for a specific function. Thanks @dstenroejl for reporting [issue](https://github.com/dark-loop/functions-authorize/issues/62).
- Using `FunctionsBearer` scheme in sample application to align with implementation.

## 4.1.1
- After authenticate but before authorize IAuthenticateResultFeature and IHttpAuthenticationFeature are now both set in HttpContext.Features and (for isolated Azure Functions) FunctionContext.Features.
- Adding support for multiple framework versions in same package instead of only targeting .NET 6. This ensures the right version for authentication assemblies to be loaded for the target ASP.NET Core version used in environment.

## 4.1.0
- ### [Breaking] Removing support for `Bearer` scheme and adding `FunctionsBearer`
  Recent security updates in the Azure Functions runtime are clashing with the use of the default, well known `Bearer` scheme.<br/>
  One of the effects of this change is the portal not able to interact with the functions app to retrieve runtime information and in some cases not able to retrieve functions information.
  In the past this was not an issue and application was able to replace the default `Bearer` configuration to enable the functionality provided by this package.<br/>
  Starting from this version, using the default `AddJwtBearer` with no custom name, will produce an error. You will have 2 options: you can switch your app to use `AddJwtFunctionsBearer` method without providing any name which will map your configuration to the `FunctionsBearer` scheme, or you can use `AddJwtBearer("<your-custom-scheme>", ...)` to specify something different.

## 4.0.1
Deprecating `DarkLoop.Azure.Functions.Authorize` package in favor of `DarkLoop.Azure.Functions.Authorization.InProcess` package.<br/>
The functionality remains the same, it's just a way to keep package naming in sync.

## 4.0.0
Starting from 4.0.0, support for Azure Functions V4 Isolated mode with ASPNET Core integration is added.
The package is now split into two separate packages, one for each mode. 

The package for Azure Functions V3+ In-Proc mode is now called `DarkLoop.Azure.Functions.Authorization.InProcess` and the package for Azure Functions V4 Isolated mode with ASPNET Core integration is called `DarkLoop.Azure.Functions.Authorize.Isolated`.

- ### .NET 6 support
  Starting with version 4.0.0, the package is now targeting .NET 6.0. This means that the package is no longer compatible with .NET 5 or lower. If you are using .NET 5 or lower, you should use version 3.1.3 of the package.
  
- ### DarkLoop.Azure.Functions.Authorize v4.0.0
  This package is published but is now deprecated in favor of `DarkLoop.Azure.Functions.Authorization.InProcess`. All it's functionality remains the same. It's just a way to keep package naming in sync.

- ### Introducing IFunctionsAuthorizationProvider interface
  The `IFunctionsAuthorizationProvider` interface is introduced to allow for custom authorization filter provisioning to the framework.
  By default the framework relies on decorating the function or type with `[FunctionAuthorize]`. You could skip this decoration and provide the middleware with an authorization filter sourced from your own mechanism, for example a database.
  At this moment this can be done only with Isolated mode even when the interface is defined in the shared package.<br/>
  Support for In-Process will be added in a future version, once source generators are introduced, as the in-process framework relies on Invocation Filters to enable authorization.
  Replacing the service in the application services would break the authorization for in-process mode at this point.

## 3.1.3
3.1.3 and lower versions only support Azure Functions V3 In-Proc mode. Starting from 4.0.0, support for Azure Functions V4 Isolated mode with ASPNET Core integration is added.
- ### Support for disabling `FunctionAuthorize` effect at the application level.
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
  > This functionality is now deprecated and no longer supported starting in version 4.1.0. It will be removed in future versions.
  > Bearer scheme is now used by the Functions runtime and another one should be used for your functions.
