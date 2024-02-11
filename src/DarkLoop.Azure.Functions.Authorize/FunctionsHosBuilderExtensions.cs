using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Functions.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IFunctionsHostBuilder"/>.
    /// </summary>
    public static class FunctionsHostBuilderExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the current environment is local development.
        /// </summary>
        /// <param name="builder">The current builder.</param>
        /// <returns></returns>
        public static bool IsLocalAuthorizationContext(this IFunctionsHostBuilder builder)
        {
            return AuthHelper.IsLocalDevelopment;
        }
    }
}
