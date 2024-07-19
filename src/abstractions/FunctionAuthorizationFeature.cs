// <copyright file="FunctionAuthorizationFeature.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Security.Claims;

namespace DarkLoop.Azure.Functions.Authorization
{
    // This was designed with maximum compatibility with ASP.NET core. It keeps
    // two separate features in sync with each other automatically.
    internal sealed class FunctionAuthorizationFeature : IAuthenticateResultFeature, IHttpAuthenticationFeature
    {
        private ClaimsPrincipal? _principal;
        private AuthenticateResult? _authenticateResult;

        /// <summary>
        /// Construct an instance of the feature with the given AuthenticateResult
        /// </summary>
        /// <param name="result"></param>
        public FunctionAuthorizationFeature(AuthenticateResult result)
        {
            Check.NotNull(result, nameof(result));

            AuthenticateResult = result;
        }

        /// <inheritdoc/>
        public AuthenticateResult? AuthenticateResult 
        { 
            get => _authenticateResult;
            set
            {
                _authenticateResult = value;
                _principal = value?.Principal;
            }
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? User 
        { 
            get => _principal; 
            set
            {
                _authenticateResult = null;
                _principal = value;
            }
        }
    }
}
