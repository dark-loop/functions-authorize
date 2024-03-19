// <copyright file="IFunctionsAuthorizationFilterCache.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

namespace DarkLoop.Azure.Functions.Authorization.Cache
{
    /// <summary>
    /// Provides a fully built authorization filter cache for functions.
    /// </summary>
    public interface IFunctionsAuthorizationFilterCache<TIdentifier>
    {
        /// <summary>
        /// Gets the authorization filter for the specified function if exists.
        /// </summary>
        bool TryGetFilter(TIdentifier functionIdentifier, out FunctionAuthorizationFilter? filter);

        /// <summary>
        /// Sets the authorization filter for the specified function.
        /// </summary>
        /// <param name="functionIdentifier">The function unique identifier</param>
        /// <param name="filter">The filter to cache.</param>
        /// <returns></returns>
        bool SetFilter(TIdentifier functionIdentifier, FunctionAuthorizationFilter filter);
    }
}