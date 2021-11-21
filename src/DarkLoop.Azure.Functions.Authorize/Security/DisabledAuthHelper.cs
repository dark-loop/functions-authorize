using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class DisabledAuthHelper : AuthHelper
    {
        private static Assembly FuncAssembly = Assembly.Load("func");
        private static MethodInfo AddSchemeMethod =
            typeof(AuthenticationBuilder).GetMethods().SingleOrDefault(x =>
                x.Name == "AddScheme" &&
                x.IsGenericMethod &&
                x.GetParameters().Length == 2);
        private static Type AuthLOptions = 
            ScriptWebHostAssembly.GetType("Microsoft.Azure.WebJobs.Script.WebHost.Authentication.AuthenticationLevelOptions");
        private static Type ArmTOptions =
            ScriptWebHostAssembly.GetType("Microsoft.Azure.WebJobs.Script.WebHost.Security.Authentication.ArmAuthenticationOptions");
        private static Type CliAuthHandler =
            FuncAssembly.GetType("Azure.Functions.Cli.Actions.HostActions.WebHost.Security.CliAuthenticationHandler`1");

        private static Func<AuthenticationBuilder, AuthenticationBuilder> __authLDisabled = BuildAuthLFunc();
        private static Func<AuthenticationBuilder, AuthenticationBuilder> __armTokenDisabled = BuildArmTFunc();

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildAuthLFunc()
        {
            var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
            var scheme = Expression.Constant(Constants.WebJobsAuthScheme);
            var action = Expression.Lambda(Expression.Empty(), Expression.Parameter(AuthLOptions, "options"));
            var genMethod = AddSchemeMethod.MakeGenericMethod(AuthLOptions, CliAuthHandler.MakeGenericType(AuthLOptions));
            var method = Expression.Call(builder, genMethod, scheme, action);
            var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);
            return lambda.Compile();
        }

        private static Func<AuthenticationBuilder, AuthenticationBuilder> BuildArmTFunc()
        {
            var builder = Expression.Parameter(typeof(AuthenticationBuilder), "builder");
            var scheme = Expression.Constant(Constants.ArmTokenAuthScheme);
            var action = Expression.Lambda(Expression.Empty(), Expression.Parameter(ArmTOptions, "options"));
            var genMethod = AddSchemeMethod.MakeGenericMethod(ArmTOptions, CliAuthHandler.MakeGenericType(ArmTOptions));
            var method = Expression.Call(builder, genMethod, scheme, action);
            var lambda = Expression.Lambda<Func<AuthenticationBuilder, AuthenticationBuilder>>(method, builder);
            return lambda.Compile();
        }

        internal static FunctionsAuthenticationBuilder AddScriptAuthLevel(FunctionsAuthenticationBuilder builder)
        {
            __authLDisabled(builder);
            return builder;
        }

        internal static FunctionsAuthenticationBuilder AddArmToken(FunctionsAuthenticationBuilder builder)
        {
            __armTokenDisabled(builder);
            return builder;
        }
    }
}
