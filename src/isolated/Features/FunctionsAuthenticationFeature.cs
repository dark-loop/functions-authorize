// <copyright file="FunctionsAuthenticationFeature.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authentication;

namespace DarkLoop.Azure.Functions.Authorization.Features
{
    internal class FunctionsAuthenticationFeature : IFunctionsAuthenticationFeature
    {
        public FunctionsAuthenticationFeature()
        {
        }

        public AuthenticateResult? Result { get; set; }
    }
}
