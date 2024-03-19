// <copyright file="FunctionAuthorizationMetadataCollection.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Function authorization rule store to allow rules to be defined even outside of attributes
    /// covering the Isolated hosting model.
    /// </summary>
    internal sealed class FunctionAuthorizationMetadataCollection
    {
        private readonly object _syncLock = new();
        internal readonly FunctionAuthorizationTypeMap _typeMap = new();
        private readonly IDictionary<int, FunctionAuthorizationMetadata> _items =
            new Dictionary<int, FunctionAuthorizationMetadata>();

        /// <summary>
        /// Gets the number of authorization metadata items in the collection.
        /// </summary>
        internal int Count => _items.Count;

        /// <summary>
        /// Adds a rule for all functions on the declaring type.
        /// </summary>
        /// <param name="declaringType">The type containing the functions.</param>
        /// <param name="existing">A value indicating whether the function declaring type is already registered.</param>
        /// <returns>An instance of <see cref="FunctionAuthorizationMetadata"/>.</returns>
        internal FunctionAuthorizationMetadata Add(Type declaringType, out bool existing)
        {
            return this.GetOrAdd(null, declaringType, out existing);
        }

        /// <summary>
        /// Adds a rule for a specific function on the declaring type.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="declaringType">The type declaring the function method.</param>
        /// <returns>An instance of <see cref="FunctionAuthorizationMetadata"/></returns>
        internal FunctionAuthorizationMetadata Add(string functionName, Type declaringType)
        {
            this.RegisterFunctionDeclaringType(functionName, declaringType);

            return this.GetOrAdd(functionName, declaringType, out _);
        }

        internal FunctionAuthorizationMetadata GetMetadata(string functionName)
        {
            var declaringType = GetFunctionDeclaringType(functionName);

            if (declaringType is null)
            {
                // TODO: Log warning
                return FunctionAuthorizationMetadata.Empty;
            }

            return GetMetadata(functionName, declaringType);
        }


        internal bool IsFunctionRegistered(string functionName)
        {
            return _typeMap.IsFunctionRegistered(functionName);
        }

        internal int GetFunctionAuthorizationId(string functionName)
        {
            var declaringType = GetFunctionDeclaringType(functionName);

            return FunctionAuthorizationMetadata.GetId(functionName, declaringType);
        }

        private bool RegisterFunctionDeclaringType(string functionName, Type declaringType)
        {
            return _typeMap.AddFunctionType(functionName, declaringType);
        }

        /// <summary>
        /// Retrieves the function authorization metadata for a specific function on the declaring type.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="declaringType">The type declaring the function.</param>
        /// <returns>An instance of <see cref="FunctionAuthorizationMetadata"/></returns>
        private FunctionAuthorizationMetadata GetMetadata(string functionName, Type declaringType)
        {
            Check.NotNullOrWhiteSpace(functionName, nameof(functionName));
            Check.NotNull(declaringType, nameof(declaringType));

            var typeId = FunctionAuthorizationMetadata.GetId(null, declaringType);
            var functionId = FunctionAuthorizationMetadata.GetId(functionName, declaringType);

            _items.TryGetValue(functionId, out var functionRule);
            _items.TryGetValue(typeId, out var typeRule);

            var typeAuthData = typeRule?.AuthorizationData ?? Enumerable.Empty<IAuthorizeData>();
            var functionAuthData = functionRule?.AuthorizationData ?? Enumerable.Empty<IAuthorizeData>();

            var merged = new FunctionAuthorizationMetadata(functionName, declaringType)
            {
                AllowsAnonymousAccess = (typeRule?.AllowsAnonymousAccess ?? false) || (functionRule?.AllowsAnonymousAccess ?? false)
            };

            return merged.AddAuthorizeData(typeAuthData.Concat(functionAuthData));
        }

        private Type? GetFunctionDeclaringType(string functionName)
        {
            return _typeMap[functionName];
        }

        private FunctionAuthorizationMetadata GetOrAdd(string? functionName, Type declaringType, out bool existing)
        {
            var key = FunctionAuthorizationMetadata.GetId(functionName, declaringType);

            existing = false;

            if (_items.TryGetValue(key, out var metadata))
            {
                existing = true;
                return metadata;
            }

            lock (_syncLock)
            {
                if (_items.TryGetValue(key, out metadata))
                {
                    existing = true;
                    return metadata;
                }

                metadata = string.IsNullOrWhiteSpace(functionName)
                    ? new FunctionAuthorizationMetadata(declaringType)
                    : new FunctionAuthorizationMetadata(functionName, declaringType);

                _items.Add(key, metadata);
            }

            return metadata;
        }
    }
}
