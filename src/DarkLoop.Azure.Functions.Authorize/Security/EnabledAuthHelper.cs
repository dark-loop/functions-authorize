using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class EnabledAuthHelper : AuthHelper
    {
        private static Func<IServiceCollection, IServiceCollection> __authNFunc = BuildAuthenticationFunc();
        
        private static Func<IServiceCollection, IServiceCollection> BuildAuthenticationFunc()
        {
            var services = Expression.Parameter(typeof(IServiceCollection), "services");
            var method = Expression.Call(WebHostSvcCollectionExtType, "AddWebJobsScriptHostAuthentication", Type.EmptyTypes, services);
            var lambda = Expression.Lambda<Func<IServiceCollection, IServiceCollection>>(method, services);

            return lambda.Compile();
        }

        internal static void AddBuiltInFunctionsAuthentication(IServiceCollection services)
        {
            __authNFunc(services);
        }
    }
}
