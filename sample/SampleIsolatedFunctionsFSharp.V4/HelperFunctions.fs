namespace SampleInProcFunctions.V4

open System.Security.Claims
open Common.Tests
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Logging
open System

type HelperFunctions() =    
    
    [<Function("GetTestToken")>]
    member _.Run(
            [<HttpTrigger("get", Route = null)>]
            req: HttpRequest,
            log: ILogger) =
        task {

                let firstName = "Test"
                let lastName = "User"
                let email = "test.user@domain.com"
                let token = JwtUtils.GenerateJwtToken(
                    [
                        new Claim("aud", "api://default")
                        new Claim("iss", "https://localhost/jwt/")
                        new Claim("scp", "user_impersonation")
                        new Claim("tid", Guid.NewGuid().ToString())
                        new Claim("oid", Guid.NewGuid().ToString())
                        new Claim("name", $"{firstName} {lastName}")
                        new Claim(ClaimTypes.Name, email)
                        new Claim(ClaimTypes.Upn, email)
                        new Claim(ClaimTypes.Email, email)
                        new Claim(ClaimTypes.GivenName, firstName)
                        new Claim(ClaimTypes.Surname, lastName)
                        new Claim("role", "Just a user")
                        new Claim("role", "admin")
                    ])

            return OkObjectResult(token)
        }


