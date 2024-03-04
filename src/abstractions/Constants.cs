// <copyright file="Constants.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace DarkLoop.Azure.Functions.Authorization
{
    [ExcludeFromCodeCoverage]
    internal class Constants
    {
        internal const string AuthInvokedKey = "__WebJobAuthInvoked";
        internal const string WebJobsAuthScheme = "WebJobsAuthLevel";
        internal const string ArmTokenAuthScheme = "ArmToken";
    }
}
