# functions-authorize
Extension bringing AuthorizeAttribute Behavior to Azure Functions In-Proc and Isolated mode. For the latter is only available with ASPNET Core integration.

It hooks into .NET Core dependency injection container to enable authentication and authorization in the same way  ASP.NET Core does.

> **Breaking for current package consumers** <br/>
> Starting with version 4.1.0, due to security changes made on the Functions runtime, the Bearer scheme is no longer supported for your app functions.<br/>
> Use `AddJwtFunctionsBearer(Action<JwtBearerOptions>)` instead of `AddJwtBearer(Action<JwtBearerOptions>)` when setting up authentication.
Using `AddJwtBearer` will generate a compilation error when used against `FunctionsAuthenticationBuilder`. 
We are introducing `JwtFunctionsBearerDefaults` to refer to the suggested new custom scheme name.<br/>
No changes should be required if already using a custom scheme name.<br/>
> Refer to respective README documentation for isolated and in-process for more information.

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

## Change Log
You can access the change log [here](https://github.com/dark-loop/functions-authorize/blob/master/ChangeLog.md).