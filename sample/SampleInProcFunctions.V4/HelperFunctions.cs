// <copyright file="HelperFunctions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SampleInProcFunctions.V4
{
    public static class HelperFunctions
    {
        [FunctionName("GetTestToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var token = JwtUtils.GenerateJwtToken(new[] {
                new Claim("aud", "api://default"),
                new Claim("iss", "https://localhost/jwt/"),
                new Claim("scp", "user_impersonation"),
                new Claim("tid", Guid.NewGuid().ToString()),
                new Claim("oid", Guid.NewGuid().ToString()),
                new Claim("name", "Test User"),
                new Claim(ClaimTypes.Upn, "test.user@domain.com"),
                new Claim(ClaimTypes.GivenName, "Test"),
                new Claim(ClaimTypes.Surname, "User"),
                new Claim(ClaimTypes.Role, "Just a user")
            });

            return await Task.FromResult<IActionResult>(new OkObjectResult(token));
        }
    }
}
