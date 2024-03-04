// <copyright file="FunctionAuthorizationContext.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Represents the context associated with the current request authorization.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    [ExcludeFromCodeCoverage]
    public class FunctionAuthorizationContext<TContext>
        where TContext : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizationContext{TContext}"/> class.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="httpContext">The platform dependent context associated with the current request.</param>
        /// <param name="policy">The authorization policy associated with the current request.</param>
        /// <param name="result">The authorization result associated with the current request.</param>
        public FunctionAuthorizationContext(
            string functionName, TContext httpContext, AuthorizationPolicy policy, PolicyAuthorizationResult result)
        {
            Check.NotNullOrWhiteSpace(functionName, nameof(functionName));
            Check.NotNull(httpContext, nameof(httpContext));
            Check.NotNull(policy, nameof(policy));
            Check.NotNull(result, nameof(result));

            FunctionName = functionName;
            UnderlyingContext = httpContext;
            Policy = policy;
            Result = result;
        }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Gets the underlying context associated with the current request.
        /// </summary>
        public TContext UnderlyingContext { get; }

        /// <summary>
        /// Gets the authorization policy associated with the current request.
        /// </summary>
        public AuthorizationPolicy Policy { get; set; }

        /// <summary>
        /// Gets the authorization result associated with the current request.
        /// </summary>
        public PolicyAuthorizationResult Result { get; set; }
    }
}
