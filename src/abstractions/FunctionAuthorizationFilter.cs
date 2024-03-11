// <copyright file="FunctionAuthorizationFilter.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Represents the authorization filter for a function.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class FunctionAuthorizationFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizationFilter"/> class.
        /// </summary>
        /// <param name="authorizationPolicy">The <see cref="AuthorizationPolicy"/> to be used for the function.</param>
        /// <param name="allowAnonymous">A value indicating whether the function allows anonymous access.</param>
        public FunctionAuthorizationFilter(AuthorizationPolicy? authorizationPolicy, bool allowAnonymous = false)
        {
            Policy = authorizationPolicy;
            AllowAnonymous = allowAnonymous;
        }

        /// <summary>
        /// A value indicating whether the function allows anonymous access.
        /// </summary>
        public bool AllowAnonymous { get; }

        /// <summary>
        /// Gets or sets the <see cref="AuthorizationPolicy"/> to be used for the function.
        /// </summary>
        public AuthorizationPolicy? Policy { get; }
    }
}
