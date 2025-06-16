// <copyright file="FunctionsAuthorizationWorkerAppBuilderExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Features;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Azure.Functions.Worker
{
  /// <summary>
  /// Extension methods for adding the <see cref="FunctionsAuthorizationMiddleware"/> to the application pipeline.
  /// </summary>
  public static class FunctionsAuthorizationWorkerAppBuilderExtensions
  {
    #region Public Methods

    /// <summary>
    /// Adds both DarkLoop's Functions authentication and authorization middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The current builder.</param>
    public static IFunctionsWorkerApplicationBuilder UseFunctionsAuth(this IFunctionsWorkerApplicationBuilder builder)
    {
      builder.UseWhen<FunctionsAuthenticationMiddleware>(context =>
        context.Features.Get<IFunctionsAuthorizationFeature>() is not null);

      builder.UseWhen<FunctionsAuthorizationMiddleware>(context =>
                context.Features.Get<IFunctionsAuthorizationFeature>() is not null);

      return builder;
    }

    /// <summary>
    /// Adds DarkLoop's Functions authentication middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The current builder.</param>
    public static IFunctionsWorkerApplicationBuilder UseFunctionsAuthentication(this IFunctionsWorkerApplicationBuilder builder)
    {
      return builder.UseWhen<FunctionsAuthenticationMiddleware>(context =>
        context.Features.Get<IFunctionsAuthorizationFeature>() is not null);
    }

    /// <summary>
    /// Adds DarkLoop's Functions authorization middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The current builder.</param>
    public static IFunctionsWorkerApplicationBuilder UseFunctionsAuthorization(this IFunctionsWorkerApplicationBuilder builder)
    {
      return builder.UseWhen<FunctionsAuthorizationMiddleware>(context =>
        context.Features.Get<IFunctionsAuthorizationFeature>() is not null);
    }

    #endregion Public Methods
  }
}