// <copyright file="AuthorizationBuilderExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorization
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Extension methods for <see cref="AuthorizationBuilder"/>
    /// </summary>
    public static class AuthorizationBuilderExtensions
    {
        /// <summary>
        /// Disables authorization for functions instrumented for authorization.
        /// </summary>
        /// <param name="builder">The current <see cref="AuthorizationBuilder"/> instance.</param>
        /// <param name="disabled">A value indicating whether authorization is disabled.</param>
        /// <returns></returns>
        public static AuthorizationBuilder DisableAuthorization(this AuthorizationBuilder builder, bool disabled)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Services.Configure<FunctionsAuthorizationOptions>(options => options.AuthorizationDisabled = disabled);
            return builder;
        }
    }
#endif
}
