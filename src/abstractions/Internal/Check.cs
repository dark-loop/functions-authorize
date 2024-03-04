// <copyright file="Check.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DarkLoop.Azure.Functions.Authorization.Internal
{
    [ExcludeFromCodeCoverage]
    internal class Check
    {
        internal static void NotNull(object value, string name, string? message = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name, message);
            }
        }

        internal static void NotNullOrWhiteSpace(string value, string name, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(message, name);
            }
        }

        internal static void All<T>(IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var item in sequence)
            {
                action(item);
            }
        }
    }
}
