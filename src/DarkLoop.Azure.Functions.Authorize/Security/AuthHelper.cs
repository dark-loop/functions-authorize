using Microsoft.AspNetCore.Authentication;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class AuthHelper
    {
        protected static Assembly ScriptWebHostAssembly = Assembly.Load("Microsoft.Azure.WebJobs.Script.WebHost");
        private static Type JwtSecurityExtsType = 
            ScriptWebHostAssembly.GetType("Microsoft.Extensions.DependencyInjection.ScriptJwtBearerExtensions");
        private static Func<AuthenticationBuilder, AuthenticationBuilder> __func = BuildFunc();

        internal static bool EnableAuth { get; private set; }

        static AuthHelper()
        {
            var entry = Assembly.GetEntryAssembly();
            var fullName = entry.FullName;
            var name = fullName.Substring(0, fullName.IndexOf(','));
            EnableAuth = name.Equals("Microsoft.Azure.WebJobs.Script.WebHost", StringComparison.OrdinalIgnoreCase);
        }

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildFunc()
        {
            var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
            var method = Expression.Call(JwtSecurityExtsType, "AddScriptJwtBearer", Type.EmptyTypes, builder);
            var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);

            return lambda.Compile();
        }

        internal static FunctionsAuthenticationBuilder AddScriptJwtBearer(FunctionsAuthenticationBuilder builder)
        {
            __func(builder);
            return builder;
        }
    }
}