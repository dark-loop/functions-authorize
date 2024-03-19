// <copyright file="FunctionsAuthorizationFeature.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Internal;

namespace DarkLoop.Azure.Functions.Authorization.Features
{
    /// <summary>
    /// Marker class for the functions authorization feature.
    /// </summary>
    internal sealed class FunctionsAuthorizationFeature : IFunctionsAuthorizationFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationFeature"/> class.
        /// </summary>
        /// <param name="name">The function name.</param>
        public FunctionsAuthorizationFeature(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            Name = name;
        }

        /// <inheritdoc />
        public string Name { get; }
    }
}
