// <copyright file="IFunctionsAuthorizationProvider.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Provides a bridge between Authorization rules and filter cache.
    /// </summary>
    internal interface IFunctionsAuthorizationProvider
    {
        /// <summary>
        /// Returns the authorization filter for the given function.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns></returns>
        Task<FunctionAuthorizationFilter> GetAuthorizationAsync(string functionName);
    }
}
