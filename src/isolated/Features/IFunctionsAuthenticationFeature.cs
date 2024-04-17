// <copyright file="IFunctionsAuthenticationFeature.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authentication;

namespace DarkLoop.Azure.Functions.Authorization.Features
{
    internal interface IFunctionsAuthenticationFeature
    {
        AuthenticateResult? Result { get; set; }
    }
}
