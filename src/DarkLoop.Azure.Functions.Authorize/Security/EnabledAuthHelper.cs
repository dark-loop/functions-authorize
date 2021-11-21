using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class EnabledAuthHelper : AuthHelper
    {
        private static Type ArmExtensionsType =
            ScriptWebHostAssembly.GetType("Microsoft.Extensions.DependencyInjection.ArmAuthenticationExtensions");
        private static Type AuthLExtensionsType =
            ScriptWebHostAssembly.GetType("Microsoft.Extensions.DependencyInjection.AuthLevelExtensions");
        private static Func<AuthenticationBuilder, AuthenticationBuilder> __armFunc = BuildArmFunc();
        private static Func<AuthenticationBuilder, AuthenticationBuilder> __authLFunc = BuildAuthLFunc();

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildArmFunc()
        {
            var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
            var method = Expression.Call(ArmExtensionsType, "AddArmToken", Type.EmptyTypes, builder);
            var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);
            return lambda.Compile();
        }

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildAuthLFunc()
        {
            var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
            var method = Expression.Call(AuthLExtensionsType, "AddScriptAuthLevel", Type.EmptyTypes, builder);
            var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);
            return lambda.Compile();
        }

        internal static FunctionsAuthenticationBuilder AddArmToken(FunctionsAuthenticationBuilder builder)
        {
            __armFunc(builder);
            return builder;
        }

        internal static FunctionsAuthenticationBuilder AddScriptAuthLevel(FunctionsAuthenticationBuilder builder)
        {
            __authLFunc(builder);
            return builder;
        }
    }
}
