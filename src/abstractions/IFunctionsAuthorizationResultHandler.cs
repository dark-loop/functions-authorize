// <copyright file="" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Handles the result of the authorization process.
    /// </summary>
    internal interface IFunctionsAuthorizationResultHandler
    {
        /// <summary>
        /// Handles the result of the authorization process.
        /// </summary>
        /// <param name="authorizationContext">The function authorization context.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="onSuccess">The action to execute if the authorization process succeeded.</param>
        /// <returns></returns>
        Task HandleResultAsync<TContext>(
            FunctionAuthorizationContext<TContext> authorizationContext, HttpContext httpContext, Func<TContext, Task>? onSuccess = null)
            where TContext : class;
    }
}