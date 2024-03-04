// <copyright file="FunctionsAuthorizeStartup.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Bindings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionsAuthorizeStartup))]

namespace DarkLoop.Azure.Functions.Authorization
{
    class FunctionsAuthorizeStartup : FunctionsStartup, IWebJobsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddFunctionsAuthorizationCore()
                .AddSingleton<IBindingProvider, FunctionsAuthorizeBindingProvider>()
                .AddSingleton<IFunctionsAuthorizationExecutor, FunctionsAuthorizationExecutor>();
        }

        void IWebJobsStartup.Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<FunctionsAuthExtension>();
            base.Configure(builder);
        }
    }
}
