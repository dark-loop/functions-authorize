﻿// <copyright file="IFunctionsAuthorizationHandler.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Azure.WebJobs.Host;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Executes the authorization for a given function.
    /// </summary>
    internal interface IFunctionsAuthorizationExecutor
    {
        /// <summary>
        /// Executes the authorization for a given function.
        /// </summary>
        /// <param name="context">The function authorization context.</param>
        Task ExecuteAuthorizationAsync(FunctionExecutingContext context);
    }
}
