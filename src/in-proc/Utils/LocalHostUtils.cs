// <copyright file="LocalHostUtils.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Linq.Expressions;
using System.Reflection;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authentication;

namespace DarkLoop.Azure.Functions.Authorization.Utils
{
    internal class LocalHostUtils : HostUtils
    {
        private static readonly Assembly? __funcAssembly;
        private static readonly MethodInfo? __addSchemeMethod;

        // The following types are used to interact with the Azure Functions runtime.
        // We use reflection to access these types to not create a hard dependency on the Azure Functions Hosting.
        private static readonly Type? __jwtSecurityExtensionsType;
        private static readonly Type? __authLevelOptionsType;
        private static readonly Type? __armTokenOptionsType;
        private static readonly Type? __cliAuthHandlerType;
        private static readonly Func<AuthenticationBuilder, AuthenticationBuilder>? __addBuiltInJwt;
        private static readonly Func<AuthenticationBuilder, AuthenticationBuilder>? __addAuthLevel;
        private static readonly Func<AuthenticationBuilder, AuthenticationBuilder>? __addArmToken;

        static LocalHostUtils()
        {
            try
            {
                // this is problematic for testing as the core tools are not loaded and there's not package available
                // enclosing in try/catch to avoid breaking the tests
                __funcAssembly = Assembly.Load("func");
            }
            catch
            {
                // ignored
            }

            if (IsLocalDevelopment)
            {
                Expression addSchemeExpression = (AuthenticationBuilder b) =>
                    b.AddScheme<AuthenticationSchemeOptions, AuthenticationHandler<AuthenticationSchemeOptions>>("scheme", null);

                __addSchemeMethod = (((LambdaExpression)addSchemeExpression).Body as MethodCallExpression)!.Method.GetGenericMethodDefinition();
                __jwtSecurityExtensionsType = WebJobsHostAssembly.GetType(Strings.WJH_JWTExtensions);
                __authLevelOptionsType = WebJobsHostAssembly.GetType(Strings.WJH_AuthLevelOptions);
                __armTokenOptionsType = WebJobsHostAssembly.GetType(Strings.WJH_ArmAuthOptions);
                __cliAuthHandlerType = __funcAssembly?.GetType(Strings.Func_ClieAuthHandler);

                __addBuiltInJwt = BuildAddBuiltInJwtFunc();
                __addAuthLevel = BuildAuthLevelFunc();
                __addArmToken = BuildArmTokenFunc();
            }
        }

        internal static FunctionsAuthenticationBuilder AddScriptJwtBearer(FunctionsAuthenticationBuilder builder)
        {
            __addBuiltInJwt?.Invoke(builder);

            return builder;
        }

        internal static FunctionsAuthenticationBuilder AddScriptAuthLevel(FunctionsAuthenticationBuilder builder)
        {
            __addAuthLevel?.Invoke(builder);

            return builder;
        }

        internal static FunctionsAuthenticationBuilder AddArmToken(FunctionsAuthenticationBuilder builder)
        {
            __addArmToken?.Invoke(builder);

            return builder;
        }

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildAddBuiltInJwtFunc()
        {
            if (__jwtSecurityExtensionsType is not null)
            {
                var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
                var method = Expression.Call(__jwtSecurityExtensionsType, "AddScriptJwtBearer", Type.EmptyTypes, builder);
                var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);

                return lambda.Compile();
            }

            return builder => builder;
        }

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildAuthLevelFunc()
        {
            if (IsLocalDevelopment && __authLevelOptionsType is not null && __cliAuthHandlerType is not null)
            {
                var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
                var scheme = Expression.Constant(Constants.WebJobsAuthScheme);
                var action = Expression.Lambda(Expression.Empty(), Expression.Parameter(__authLevelOptionsType, "options"));
                var genMethod = __addSchemeMethod!.MakeGenericMethod(__authLevelOptionsType, __cliAuthHandlerType.MakeGenericType(__authLevelOptionsType));
                var method = Expression.Call(builder, genMethod, scheme, action);
                var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);

                return lambda.Compile();
            }

            return builder => builder;
        }

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildArmTokenFunc()
        {
            if (IsLocalDevelopment && __armTokenOptionsType is not null && __cliAuthHandlerType is not null)
            {
                var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
                var scheme = Expression.Constant(Constants.ArmTokenAuthScheme);
                var action = Expression.Lambda(Expression.Empty(), Expression.Parameter(__armTokenOptionsType, "options"));
                var genMethod = __addSchemeMethod!.MakeGenericMethod(__armTokenOptionsType, __cliAuthHandlerType.MakeGenericType(__armTokenOptionsType));
                var method = Expression.Call(builder, genMethod, scheme, action);
                var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);

                return lambda.Compile();
            }

            return builder => builder;
        }
    }
}
