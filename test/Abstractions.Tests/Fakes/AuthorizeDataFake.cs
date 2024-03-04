// <copyright file="AuthorizeDataFake.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace Abstractions.Tests.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class AuthorizeDataFake : IAuthorizeData
    {
        public string? Policy { get; set; }
        public string? Roles { get; set; }
        public string? AuthenticationSchemes { get; set; }
    }
}
