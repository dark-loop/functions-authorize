// <copyright file="FunctionsAuthorizationCoreServiceCollectionExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Extension methods for adding the Functions Authorization Core services to the DI container.
    /// </summary>
    internal static class FunctionsAuthorizationCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddFunctionsAuthorizationCore(this IServiceCollection services)
        {
            services
                .TryAddSingleton<IFunctionsAuthorizationProvider, FunctionsAuthorizationProvider>();

            return services
                .AddSingleton<IFunctionsAuthorizationResultHandler, FunctionsAuthorizationResultHandler>()
                .AddSingleton(typeof(IFunctionsAuthorizationFilterCache<>), typeof(FunctionsAuthorizationFilterCache<>));
        }
    }
}
