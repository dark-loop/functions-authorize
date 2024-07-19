// <copyright file="FunctionsFeatureCollectionExtension.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace DarkLoop.Azure.Functions.Authorization.Internal
{
    // This functionality is used internally to emulate Asp.net's treatment of AuthenticateResult
    internal static class FunctionsFeatureCollectionExtension
    {
        /// <summary>
        /// Store the given AuthenticateResult in the IFeatureCollection accessible via
        /// IAuthenticateResultFeature and IHttpAuthenticationFeature
        /// </summary>
        /// <param name="features">The feature collection to add to</param>
        /// <param name="result">The authentication to expose in the feature collection</param>
        /// <returns>The object associated with the features</returns>
        public static FunctionAuthorizationFeature SetAuthenticationFeatures(this IFeatureCollection features, AuthenticateResult result)
        {
            // A single object is used to handle both of these features so that they stay in sync.
            // This is in line with what asp core normally does.
            var feature = new FunctionAuthorizationFeature(result);

            features.Set<IAuthenticateResultFeature>(feature);
            features.Set<IHttpAuthenticationFeature>(feature);

            return feature;
        }
    }
}
