// <copyright file="FunctionsAuthExtension.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace DarkLoop.Azure.Functions.Authorization
{
    [Extension("FunctionsAuthorize")]
    class FunctionsAuthExtension : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context) { }
    }
}
