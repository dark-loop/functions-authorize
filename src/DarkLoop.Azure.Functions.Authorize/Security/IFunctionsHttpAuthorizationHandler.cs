using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal interface IFunctionsHttpAuthorizationHandler
    {
        [Obsolete("This functionality is dependent on Azure Functions preview features.")]
        Task OnAuthorizingFunctionInstance(FunctionExecutingContext functionContext, HttpContext httpContext);
    }
}
