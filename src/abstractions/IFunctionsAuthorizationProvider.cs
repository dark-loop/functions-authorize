// <copyright file="IFunctionsAuthorizationProvider.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Provides a bridge between Authorization rules and filter cache.
    /// </summary>
    public interface IFunctionsAuthorizationProvider
    {
        /// <summary>
        /// Returns the authorization filter for the given function.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="policyProvider">The <see cref="IAuthorizationPolicyProvider"/> to be used to construct the policy.</param>
        /// <remarks>It's recommended to cache the value in this method before returning it, as this method is called for every function invocation.</remarks>
        /// <returns>A non-null authorization filter for the function.</returns>
        Task<FunctionAuthorizationFilter> GetAuthorizationAsync(string functionName, IAuthorizationPolicyProvider policyProvider);
    }
}
