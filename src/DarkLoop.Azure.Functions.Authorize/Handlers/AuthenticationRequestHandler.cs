using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorize.Handlers
{
    public static class AuthenticationRequestHandler
    {
        public static async Task<IActionResult?> HandleAuthenticateAsync(HttpContext context, string scheme)
        {
            var handler = await GetSchemeHandler(context, scheme);
            if(await handler.HandleRequestAsync())
            {
                return null;
            }

            return new UnauthorizedResult();
        }


        private static async Task<IAuthenticationRequestHandler> GetSchemeHandler(HttpContext context, string scheme)
        {
            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            var handler = await handlers.GetHandlerAsync(context, scheme) as IAuthenticationRequestHandler;

            if (handler != null)
            {
                return handler;
            }
            else
            {
                var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                var reqSchemes = await schemes.GetRequestHandlerSchemesAsync();

                throw new InvalidOperationException($"Scheme '{scheme}' is not a valid request authentication handler. List of valid available handlers: {string.Join(", ", reqSchemes.Select(x => x.Name))}");
            }
        }
    }
}