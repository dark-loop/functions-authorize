// <copyright file="FunctionsFeatureCollectionExtension.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkLoop.Azure.Functions.Authorization.Internal
{
    internal static class FunctionsFeatureCollectionExtension
    {
        public static void SetAuthenticationFeatures(this IFeatureCollection features, AuthenticateResult result)
        {
            // A single object is used to handle both of these features so that they stay in sync.
            // This is in line with what asp core normally does.
            var feature = new FunctionAuthorizationFeature(result);

            features.Set<IAuthenticateResultFeature>(feature);
            features.Set<IHttpAuthenticationFeature>(feature);
        }
    }
}
