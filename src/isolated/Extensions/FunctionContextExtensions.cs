// <copyright file="FunctionContextExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Linq;
using Microsoft.Azure.Functions.Worker;

namespace DarkLoop.Azure.Functions.Authorization.Extensions
{
    internal static class FunctionContextExtensions
    {
        private const string HttpTriggerBindingType = "httpTrigger";

        /// <summary>
        /// Determines if a function is an HTTP trigger.
        /// </summary>
        /// <param name="context">The current function context.</param>
        /// <returns><see langword="true"/> if the function is an HTTP function; otherwise <see langword="false"/>.</returns>
        internal static bool IsHttpTrigger(this FunctionContext context)
        {
            return context.FunctionDefinition.InputBindings.Any(b => 
                b.Value.Type.Equals(HttpTriggerBindingType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
