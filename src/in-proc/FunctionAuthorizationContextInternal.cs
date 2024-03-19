// <copyright file="FunctionAuthorizationContextInternal.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Internal implementation of the function authorization context.
    /// </summary>
    internal sealed class FunctionAuthorizationContextInternal : FunctionAuthorizationContext<HttpContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizationContextInternal"/> class.
        /// </summary>
        internal FunctionAuthorizationContextInternal(
            string functionName, HttpContext httpContext, AuthorizationPolicy policy, PolicyAuthorizationResult result)
            : base(functionName, httpContext, policy, result)
        {
        }
    }
}
