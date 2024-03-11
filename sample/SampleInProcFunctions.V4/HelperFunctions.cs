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
            var firstName = "Test";
            var lastName = "User";
            var email = "test.user@domain.com";
            var token = JwtUtils.GenerateJwtToken(new[] {
                new Claim("aud", "api://default"),
                new Claim("iss", "https://localhost/jwt/"),
                new Claim("scp", "user_impersonation"),
                new Claim("tid", Guid.NewGuid().ToString()),
                new Claim("oid", Guid.NewGuid().ToString()),
                new Claim("name", $"{firstName} {lastName}"),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Upn, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName),
                new Claim("role", "Just a user"),
                new Claim("role", "another user"),
            });

            return await Task.FromResult<IActionResult>(new OkObjectResult(token));
        }
    }
}
