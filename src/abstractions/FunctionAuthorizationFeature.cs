// <copyright file="FunctionAuthorizationFeature.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization
{
    internal sealed class FunctionAuthorizationFeature : IAuthenticateResultFeature, IHttpAuthenticationFeature
    {
        private ClaimsPrincipal? _principal;
        private AuthenticateResult? _authenticateResult;

        public FunctionAuthorizationFeature(AuthenticateResult result)
        {
            Check.NotNull(result, nameof(result));

            AuthenticateResult = result;
        }


        public AuthenticateResult? AuthenticateResult 
        { 
            get => _authenticateResult;
            set
            {
                _authenticateResult = value;
                _principal = value?.Principal;
            }
        }

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
