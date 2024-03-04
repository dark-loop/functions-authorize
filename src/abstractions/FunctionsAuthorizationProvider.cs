// <copyright file="FunctionAuthorizationProviderBase.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Cache;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <inheritdoc cref="IFunctionsAuthorizationProvider"/>
    internal class FunctionsAuthorizationProvider : IFunctionsAuthorizationProvider
    {
        private static readonly IEnumerable<string> __dismissedSchemes =
            new[] { Constants.WebJobsAuthScheme, Constants.ArmTokenAuthScheme };


        private readonly IFunctionsAuthorizationFilterCache<int> _filterCache;
        private readonly IAuthenticationSchemeProvider _schemesProvider;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly FunctionsAuthorizationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationProvider"/> class.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="schemeProvider"></param>
        public FunctionsAuthorizationProvider(
            IFunctionsAuthorizationFilterCache<int> cache,
            IAuthenticationSchemeProvider schemeProvider,
            IAuthorizationPolicyProvider policyProvider,
            IOptions<FunctionsAuthorizationOptions> options)
        {
            _filterCache = cache;
            _schemesProvider = schemeProvider;
            _policyProvider = policyProvider;
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task<FunctionAuthorizationFilter> GetAuthorizationAsync(string functionName)
        {
            Check.NotNullOrWhiteSpace(functionName, nameof(functionName));

            var declaringType = _options.GetFunctionDeclaringType(functionName);

            var key = FunctionAuthorizationMetadata.GetId(functionName, declaringType);

            if (_filterCache.TryGetFilter(key, out var filter))
            {
                return filter!;
            }

            var functionRule = _options.GetMetadata(functionName);

            if (functionRule is null)
            {
                filter = new FunctionAuthorizationFilter(null, true);
            }
            else
            {
                var policy = await AuthorizationPolicy.CombineAsync(_policyProvider, functionRule.AuthorizationData);

                filter = new FunctionAuthorizationFilter(policy, functionRule.AllowsAnonymousAccess);
            }

            _filterCache.SetFilter(key, filter);

            return filter;
        }
    }
}
