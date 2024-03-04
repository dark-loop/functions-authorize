// <copyright file="FunctionsAuthorizationOptions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Options to manage Authorization functionality for Azure Functions.
    /// </summary>
    public class FunctionsAuthorizationOptions
    {
        internal readonly FunctionAuthorizationMetadataCollection AuthorizationMetadata = new();
        internal readonly FunctionAuthorizationTypeMap TypeMap = new();

        /// <summary>
        /// Gets or sets a value indicating whether authorization is disabled.
        /// </summary>
        public bool AuthorizationDisabled {get; set;}

        /// <summary>
        /// Gets or sets a value indicating whether to write the HTTP status
        /// to the response when authorization failure occurs.
        /// </summary>
        public bool WriteHttpStatusToResponse { get; set; }
    }
}
