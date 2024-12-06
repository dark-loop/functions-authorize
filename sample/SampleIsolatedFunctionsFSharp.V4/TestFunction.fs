namespace SampleIsolatedFunctionsFSharp.V4

open System.Text
open DarkLoop.Azure.Functions.Authorization
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging


[<FunctionAuthorize(AuthenticationSchemes = "FunctionsBearer")>]
type TestFunction(logger:ILogger<TestFunction>) =
    let _logger = logger
        
    [<Function("TestFunction")>]
    [<Authorize(Roles = "admin")>]
    member _.Run([<HttpTrigger("get", "post")>] req:HttpRequest) =
                task {
                        _logger.LogInformation("F# HTTP trigger function processed a request.")

                        let provider = req.HttpContext.RequestServices
                        let schProvider = provider.GetService<IAuthenticationSchemeProvider>()

                        let sb = new StringBuilder()
                        sb.AppendLine("Authentication schemes:") |> ignore

                        if (schProvider <> null) then             
                            let! allScheme = schProvider.GetAllSchemesAsync()
                            for scheme in allScheme do
                                sb.AppendLine($"  {scheme.Name} -> {scheme.HandlerType}") |> ignore
             

                        sb.AppendLine()|> ignore
                        sb.AppendLine($"User:")|> ignore
                        sb.AppendLine($"  Name  -> {req.HttpContext.User.Identity.Name}")|> ignore
                        let email = req.HttpContext.User.FindFirst("email")|> Option.ofObj|>Option.map _.Value
                        sb.AppendLine($"  Email -> {email}")|> ignore

                        return OkObjectResult(sb.ToString())
            }