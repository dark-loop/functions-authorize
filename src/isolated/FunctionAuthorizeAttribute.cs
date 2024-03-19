// <copyright file="FunctionAuthorizeAttribute.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DebuggerDisplay("{ToString(),nq}")]
    [ExcludeFromCodeCoverage]
    public class FunctionAuthorizeAttribute : AuthorizeAttribute, IAuthorizeData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizeAttribute"/> class.
        /// </summary>
        public FunctionAuthorizeAttribute()
            : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="policy">The policy name that determines access to the resource</param>
        public FunctionAuthorizeAttribute(string policy)
            : base(policy) { }
    }
}
