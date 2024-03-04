// <copyright file="FunctionAuthorizationTypeMap.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization
{
    internal sealed class FunctionAuthorizationTypeMap
    {
        private readonly ConcurrentDictionary<string, Type> _typeMap = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers the type declaring a function with the specified <paramref name="functionName"/>.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="functionType">The type declaring the function.</param>
        /// <returns><see langword="true"/> if the registration was successful; otherwise <see langword="false"/>.</returns>
        internal bool AddFunctionType(string functionName, Type functionType)
        {
            return _typeMap.TryAdd(functionName, functionType);
        }

        /// <summary>
        /// Gets the type declaring a function with the specified <paramref name="functionName"/>.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns></returns>
        internal Type? this[string functionName]
        {
            get => _typeMap.GetValueOrDefault(functionName);
        }

        /// <summary>
        /// Returns a value indicating whether the function with the specified <paramref name="functionName"/> is registered.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns><see langword="true" /> if function is registered; otherwise <see langword="false"/></returns>
        internal bool IsFunctionRegistered(string functionName)
        {
            return _typeMap.ContainsKey(functionName);
        }
    }
}
