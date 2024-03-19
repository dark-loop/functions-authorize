// <copyright file="FunctionsAuthorizationFilterCache.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace DarkLoop.Azure.Functions.Authorization.Cache
{
    /// <inheritdoc cref="IFunctionsAuthorizationFilterCache{TIdentifier}"/>
    internal sealed class FunctionsAuthorizationFilterCache<TIdentifier> : IFunctionsAuthorizationFilterCache<TIdentifier>
        where TIdentifier : notnull
    {
        private readonly ConcurrentDictionary<TIdentifier, FunctionAuthorizationFilter> _filters = new();

        /// <inheritdoc />
        public bool TryGetFilter(TIdentifier functionIdentifier, out FunctionAuthorizationFilter? filter)
        {
            return _filters.TryGetValue(functionIdentifier, out filter);
        }

        /// <inheritdoc />
        public bool SetFilter(TIdentifier functionIdentifier, FunctionAuthorizationFilter builder)
        {
            return _filters.TryAdd(functionIdentifier, builder);
        }
    }
}
