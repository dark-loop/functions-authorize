// <copyright file="IFunctionsAuthorizationFeature.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

namespace DarkLoop.Azure.Functions.Authorization.Features
{
    /// <summary>
    /// Marker interface for the functions authorization feature.
    /// </summary>
    internal interface IFunctionsAuthorizationFeature
    {
        /// <summary>
        /// Gets the function name.
        /// </summary>
        string Name { get; }
    }
}
