// <copyright file="FunctionsAuthenticationBuilder.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// An <see cref="AuthenticationBuilder"/> that enhances the built-in authentication behavior for Azure Functions.
    /// </summary>
    public class FunctionsAuthenticationBuilder : AuthenticationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthenticationBuilder"/> class.
        /// </summary>
        /// <param name="services">The current service collection instance.</param>
        internal FunctionsAuthenticationBuilder(IServiceCollection services)
            : base(services) { }
    }
}
