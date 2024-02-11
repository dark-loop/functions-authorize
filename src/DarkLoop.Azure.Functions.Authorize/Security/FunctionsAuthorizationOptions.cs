using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkLoop.Azure.Functions.Authorize.Security
{
    /// <summary>
    /// Options to manage Authorization functionality for Azure Functions.
    /// </summary>
    public class FunctionsAuthorizationOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether authorization is disabled.
        /// </summary>
        public bool AuthorizationDisabled {get; set;}
    }
}
