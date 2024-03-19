// <copyright file="FunctionsAuthorizationExtensionStartup.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Metadata;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core;
using Microsoft.Extensions.Hosting;

[assembly: WorkerExtensionStartup(typeof(FunctionsAuthorizationExtensionStartup))]

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Functions authorization extension startup.
    /// </summary>
    public class FunctionsAuthorizationExtensionStartup : WorkerExtensionStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsWorkerApplicationBuilder applicationBuilder)
        {
            applicationBuilder.Services.AddFunctionsAuthorizationCore();

            // This is the only middleware we add in startup as it executes prior to other built-in extensions.
            // Adding AuthorizationMiddleware at this point removes the ability to access to the request context.
            // Package consumer is in charge of adding the AuthorizationMiddleware by calling UseFunctionsAuthorization.
            applicationBuilder.UseMiddleware<FunctionsAuthorizationMetadataMiddleware>();
        }
    }
}
