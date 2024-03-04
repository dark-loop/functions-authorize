using DarkLoop.Azure.Functions.Authorization.Utils;

namespace Microsoft.Azure.Functions.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IFunctionsHostBuilder"/>.
    /// </summary>
    public static class FunctionsAuthorizationHostBuilderExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the current environment is local development.
        /// </summary>
        /// <param name="builder">The current builder.</param>
        /// <returns></returns>
        public static bool IsLocalAuthorizationContext(this IFunctionsHostBuilder builder)
        {
            return HostUtils.IsLocalDevelopment;
        }
    }
}
