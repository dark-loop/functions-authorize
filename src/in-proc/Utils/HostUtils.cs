// <copyright file="HostUtils.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Linq.Expressions;
using System.Reflection;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorization.Utils
{
    internal class HostUtils
    {
        protected static readonly Assembly WebJobsHostAssembly;

        // These are a series of publicly available types that are used to interact with the Azure Functions runtime.
        // We use reflection to access these types to not create a hard dependency on the Azure Functions WebHost.
        private static readonly Type? __webHostSvcCollectionExtType;
        internal static readonly Type? FunctionExecutionFeatureType;

        private static readonly Func<IServiceCollection, IServiceCollection>? __authorizationFunc;
        private static readonly Func<IServiceCollection, IServiceCollection>? __authenticationFunc;

        static HostUtils()
        {
            WebJobsHostAssembly = Assembly.Load(Strings.WJH_Assembly);

            if (WebJobsHostAssembly is null)
            {
                throw new InvalidOperationException($"{Assembly.GetExecutingAssembly()} cannot be used outside of an Azure Functions environment.");
            }

            __webHostSvcCollectionExtType = WebJobsHostAssembly.GetType(Strings.WJH_WebJovsSvcsExtensions);
            FunctionExecutionFeatureType = WebJobsHostAssembly.GetType(Strings.WJH_FuncExecFeature);

            var entryAssembly = Assembly.GetEntryAssembly();
            var entryFullName = entryAssembly!.FullName;
            var entryName = entryFullName!.Substring(0, entryFullName.IndexOf(','));
            IsLocalDevelopment = !entryName.Equals(Strings.WJH_Assembly, StringComparison.OrdinalIgnoreCase);

            __authenticationFunc = BuildBuiltInAuthenticationFunc();
            __authorizationFunc = BuildBuiltInAuthorizationFunc();
        }

        internal static bool IsLocalDevelopment { get; }

        internal static IServiceCollection AddFunctionsBuiltInAuthentication(IServiceCollection services)
        {
            return __authenticationFunc?.Invoke(services) ?? services;
        }

        internal static IServiceCollection AddFunctionsBuiltInAuthorization(IServiceCollection services)
        {
            return __authorizationFunc?.Invoke(services) ?? services;
        }

        private static Func<IServiceCollection, IServiceCollection> BuildBuiltInAuthenticationFunc()
        {
            if (__webHostSvcCollectionExtType is not null)
            {
                var services = Expression.Parameter(typeof(IServiceCollection), "services");
                var method = Expression.Call(__webHostSvcCollectionExtType, Strings.WJH_AddAuthentication, Type.EmptyTypes, services);
                var lambda = Expression.Lambda<Func<IServiceCollection, IServiceCollection>>(method, services);

                return lambda.Compile();
            }

            return builder => builder;
        }

        private static Func<IServiceCollection, IServiceCollection> BuildBuiltInAuthorizationFunc()
        {
            if (__webHostSvcCollectionExtType is not null)
            {
                var services = Expression.Parameter(typeof(IServiceCollection), "services");
                var method = Expression.Call(__webHostSvcCollectionExtType, Strings.WJH_AddAuthorization, Type.EmptyTypes, services);
                var lambda = Expression.Lambda<Func<IServiceCollection, IServiceCollection>>(method, services);

                return lambda.Compile();
            }

            return services => services;
        }
    }
}
