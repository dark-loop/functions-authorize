using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class AuthHelper
    {
        protected static Assembly ScriptWebHostAssembly = Assembly.Load("Microsoft.Azure.WebJobs.Script.WebHost");
        protected static Type WebHostSvcCollectionExtType =
            ScriptWebHostAssembly.GetType("Microsoft.Azure.WebJobs.Script.WebHost.WebHostServiceCollectionExtensions");
        private static Type JwtSecurityExtsType = 
            ScriptWebHostAssembly.GetType("Microsoft.Extensions.DependencyInjection.ScriptJwtBearerExtensions");
        private static Func<AuthenticationBuilder, AuthenticationBuilder> __func = BuildFunc();
        private static Func<IServiceCollection, IServiceCollection> __authorizationFunc = BuildAuthorizationFunc();

        internal static bool EnableAuth { get; private set; }

        internal static bool IsLocalDevelopment => !EnableAuth;

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

        private static Func<IServiceCollection, IServiceCollection> BuildAuthorizationFunc()
        {
            var services = Expression.Parameter(typeof(IServiceCollection), "services");
            var method = Expression.Call(WebHostSvcCollectionExtType, "AddWebJobsScriptHostAuthorization", Type.EmptyTypes, services);
            var lambda = Expression.Lambda<Func<IServiceCollection, IServiceCollection>>(method, services);

            return lambda.Compile();
        }

        internal static IServiceCollection AddFunctionsBuiltInAuthorization(IServiceCollection services)
        {
            return __authorizationFunc(services);
        }

        internal static FunctionsAuthenticationBuilder AddScriptJwtBearer(FunctionsAuthenticationBuilder builder)
        {
            __func(builder);
            return builder;
        }
    }
}