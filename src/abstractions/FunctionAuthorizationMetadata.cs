// <copyright file="FunctionAuthorizationMetadata.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Authorization metadata for a function or type.
    /// </summary>
    public sealed class FunctionAuthorizationMetadata
    {
        private readonly int _key;
        private readonly string? _functionName;
        private readonly Type? _declaringType;
        private readonly List<IAuthorizeData> _authData;

        /// <summary>
        /// Default authorization rule.
        /// </summary>
        public readonly static FunctionAuthorizationMetadata Empty = new() { AllowsAnonymousAccess = false };

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizationMetadata"/> class.
        /// </summary>
        private FunctionAuthorizationMetadata() => _authData = new List<IAuthorizeData>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizationMetadata"/> class.
        /// </summary>
        /// <param name="functionName">The name of the function as specified in the [Function(Name)Attribute].</param>
        /// <param name="declaringType">The type declaring the function method.</param>
        internal FunctionAuthorizationMetadata(string functionName, Type declaringType) : this()
        {
            Check.NotNullOrWhiteSpace(functionName, nameof(functionName), "The name of the function must be specified.");
            Check.NotNull(declaringType, nameof(declaringType), "The declaring type of the function must be specified.");

            _key = GetId(functionName, declaringType);
            _functionName = functionName;
            _declaringType = declaringType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAuthorizationMetadata"/> class.
        /// </summary>
        /// <param name="declaringType">The type declaring the function method.</param>
        internal FunctionAuthorizationMetadata(Type declaringType) : this()
        {
            Check.NotNull(declaringType, nameof(declaringType), "The declaring type of the function must be specified.");

            _key = GetId(null, declaringType);
            _declaringType = declaringType;
        }

        /// <summary>
        /// Gets the authorization ID for the function or type.
        /// </summary>
        public int AuthorizationId => _key;

        /// <summary>
        /// Gets the name of the function as specified in the [Function(Name)Attribute].
        /// </summary>
        public string? FunctionName => _functionName;

        /// <summary>
        /// Gets the name of the type declaring the function method.
        /// </summary>
        /// <remarks>The returned value is never <see langword="null"/>. Only for <see cref="Empty"/></remarks>
        public Type? DeclaringType => _declaringType;

        /// <summary>
        /// Gets a value indicating whether the function allows anonymous access.
        /// </summary>
        public bool AllowsAnonymousAccess { get; internal set; }

        /// <summary>
        /// Gets the authorization data for the function or type.
        /// </summary>
        public IReadOnlyList<IAuthorizeData> AuthorizationData => _authData.ToImmutableArray();

        /// <summary>
        /// Adds authorization data to metadata.
        /// </summary>
        /// <param name="authorizeData">Authorize data.</param>
        public FunctionAuthorizationMetadata AddAuthorizeData(IAuthorizeData authorizeData)
        {
            Check.NotNull(authorizeData, nameof(authorizeData), "The authorization data must be specified.");

            _authData.Add(authorizeData);

            return this;
        }

        /// <summary>
        /// Adds authorization data to metadata.
        /// </summary>
        /// <param name="authorizeData">Authorize data.</param>
        public FunctionAuthorizationMetadata AddAuthorizeData(IEnumerable<IAuthorizeData> authorizeData)
        {
            Check.NotNull(authorizeData, nameof(authorizeData), "The authorization data must be specified.");
            Check.All(authorizeData, x => Check.NotNull(x, nameof(authorizeData), "All elements in authorization data must be non-null."));

            _authData.AddRange(authorizeData);

            return this;
        }

        /// <summary>
        /// Allows anonymous access to the function.<br/>
        /// Once the value ise set, it cannot be changed.
        /// </summary>
        public FunctionAuthorizationMetadata AllowAnonymousAccess()
        {
            AllowsAnonymousAccess = true;

            return this;
        }

        /// <summary>
        /// Gets the metadata ID for a function.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="declaringType">The type declaring the function.</param>
        internal static int GetId(string? functionName, Type? declaringType) =>
            (declaringType?.GetHashCode() ?? 0) ^
            (functionName?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);
    }
}
