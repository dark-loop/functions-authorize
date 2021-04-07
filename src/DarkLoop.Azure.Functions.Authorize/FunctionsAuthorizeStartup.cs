using DarkLoop.Azure.Functions.Authorize;
using DarkLoop.Azure.Functions.Authorize.Bindings;
using DarkLoop.Azure.Functions.Authorize.Filters;
using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionsAuthorizeStartup))]
namespace DarkLoop.Azure.Functions.Authorize
{
    class FunctionsAuthorizeStartup : FunctionsStartup, IWebJobsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IBindingProvider, FunctionsAuthorizeBindingProvider>();
            builder.Services.AddSingleton<IFunctionsAuthorizationFilterIndex, FunctionAuthorizationFilterIndex>();
            builder.Services.AddSingleton<IFunctionsHttpAuthorizationHandler, FunctionsHttpAuthorizationHandler>();
        }

        void IWebJobsStartup.Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<FunctionsAuthExtension>();
            base.Configure(builder);
        }
    }
}
