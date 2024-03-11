// <copyright file="JwtUtils.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Common.Tests
{
    public static class JwtUtils
    {
        private const string __jwtTestSecret = "f24847d0e55027928dd04b707a8d26ce24ed0dd0d7a0c19f545cc0c82c6ac7ae";

        public static string GenerateJwtToken(IEnumerable<Claim>? claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.Unicode.GetBytes(__jwtTestSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static SymmetricSecurityKey GetSigningKey()
        {
            var key = Encoding.Unicode.GetBytes(__jwtTestSecret);
            return new SymmetricSecurityKey(key);
        }
    }
}
