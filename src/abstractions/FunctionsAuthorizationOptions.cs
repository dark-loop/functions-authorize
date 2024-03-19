// <copyright file="FunctionsAuthorizationOptions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Options to manage Authorization functionality for Azure Functions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    // Important to keep this class POCO, any special functionality should be done with extension methods.
    public sealed class FunctionsAuthorizationOptions
    {
        internal readonly FunctionAuthorizationMetadataCollection AuthorizationMetadata = new();

        /// <summary>
        /// Gets or sets a value indicating whether authorization is disabled.
        /// </summary>
        public bool AuthorizationDisabled {get; set;}

        /// <summary>
        /// Gets or sets a value indicating whether to write the HTTP status
        /// to the response when authorization failure occurs.
        /// </summary>
        public bool WriteHttpStatusToResponse { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when <see cref="IAuthorizeData.AuthenticationSchemes"/> is empty. Default value is <see cref="EmptySchemeStrategy.UseAllSchemes"/>.
        /// </summary>
        public EmptySchemeStrategy EmptySchemeStrategy { get; set; }
    }
}
