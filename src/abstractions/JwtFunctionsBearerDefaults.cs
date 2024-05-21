// <copyright file="JwtFunctionsBearerDefaults.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Default values used for Jwt Functions Bearer authentication.
    /// </summary>
    public class JwtFunctionsBearerDefaults
    {
        /// <summary>
        /// The default scheme used for Jwt Functions Bearer authentication.
        /// </summary>
        public const string AuthenticationScheme = "FunctionsBearer";
    }
}
