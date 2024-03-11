// <copyright file="" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Strategy to use when schemes are not specified in function authorization definition.
    /// </summary>
    public enum EmptySchemeStrategy
    {
        /// <summary>
        /// Use all authentication schemes specified in the application.
        /// </summary>
        /// <remarks>
        /// This does not apply to <see langword="WebJobsAuthLevel"/> and <see langword="ArmToken"/> within the In-Proc hosting model.
        /// </remarks>
        UseAllSchemes,

        /// <summary>
        /// Use the default authentication scheme specified in the application.
        /// </summary>
        UseDefaultScheme,
    }
}