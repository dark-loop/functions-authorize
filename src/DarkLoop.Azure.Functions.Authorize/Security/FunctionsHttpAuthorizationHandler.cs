using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    internal class FunctionsHttpAuthorizationHandler : IFunctionsHttpAuthorizationHandler
    {
        private readonly IFunctionsAuthorizationFilterIndex _filtersIndex;

        public FunctionsHttpAuthorizationHandler(IFunctionsAuthorizationFilterIndex filtersIndex)
        {
            _filtersIndex = filtersIndex ?? throw new ArgumentNullException(nameof(filtersIndex));
        }

        [Obsolete("This functionality is dependent on Azure Functions preview features.")]
        public async Task OnAuthorizingFunctionInstance(FunctionExecutingContext functionContext, HttpContext httpContext)
        {
            if (functionContext is null) throw new ArgumentNullException(nameof(functionContext));
            if (httpContext is null) throw new ArgumentNullException(nameof(httpContext));

            var filter = _filtersIndex.GetAuthorizationFilter(functionContext.FunctionName);

            if (filter is null) return;

            var context = new FunctionAuthorizationContext(httpContext);

            await filter.AuthorizeAsync(context);

            if (context.Result is ChallengeResult challenge)
            {
                if (challenge.AuthenticationSchemes != null && challenge.AuthenticationSchemes.Count > 0)
                {
                    foreach (var scheme in challenge.AuthenticationSchemes)
                    {
                        await httpContext.ChallengeAsync(scheme);
                    }
                }
                else
                {
                    await httpContext.ChallengeAsync();
                }

                await SetResponseAsync("Unauthorized", httpContext.Response);
                
                // need to make sure function stops executing. At this moment this is the only way.
                BombFunctionInstance((int)HttpStatusCode.Unauthorized);
            }

            if (context.Result is ForbidResult forbid)
            {
                if (forbid.AuthenticationSchemes != null && forbid.AuthenticationSchemes.Count > 0)
                {
                    foreach (var scheme in forbid.AuthenticationSchemes)
                    {
                        await httpContext.ForbidAsync(scheme);
                    }
                }
                else
                {
                    await httpContext.ForbidAsync();
                }

                await SetResponseAsync("Forbidden", httpContext.Response);

                // need to make sure function stops executing. At this moment this is the only way.
                BombFunctionInstance((int)HttpStatusCode.Forbidden);
            }
        }

        private async Task SetResponseAsync(string message, HttpResponse response)
        {
            if (response.HasStarted)
                return;

            response.ContentType = "text/plain";
            response.ContentLength = message.Length;
            await response.WriteAsync(message);
            await response.Body.FlushAsync();
        }

        private void BombFunctionInstance(int status)
        {
            throw new Exception(
                $"{status} Authorization error encountered. This is the only way to stop function execution. The correct status has been communicated to caller");
        }
    }
}
