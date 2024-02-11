using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Extension methods for <see cref="AuthorizationBuilder"/>
    /// </summary>
    public static class AuthorizationBuilderExtensions
    {
        /// <summary>
        /// Disables authorization for functions decorated with <see cref="FunctionAuthorizeAttribute"/>
        /// </summary>
        /// <param name="builder">The current <see cref="AuthorizationBuilder"/> instance.</param>
        /// <param name="disabled">A value indicating whether authorization is disabled.</param>
        /// <returns></returns>
        public static AuthorizationBuilder DisableAuthorization(this AuthorizationBuilder builder, bool disabled)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<FunctionsAuthorizationOptions>(options => options.AuthorizationDisabled = disabled);
            return builder;
        }
    }
#endif
}
