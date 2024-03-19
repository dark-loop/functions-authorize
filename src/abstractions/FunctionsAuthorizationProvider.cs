// <copyright file="FunctionAuthorizationProviderBase.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorization.Cache;
using DarkLoop.Azure.Functions.Authorization.Internal;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <inheritdoc cref="IFunctionsAuthorizationProvider"/>
    internal class FunctionsAuthorizationProvider : IFunctionsAuthorizationProvider
    {
        private static readonly IEnumerable<string> __dismissedSchemes =
            new[] { Constants.WebJobsAuthScheme, Constants.ArmTokenAuthScheme };

        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IFunctionsAuthorizationFilterCache<int> _filterCache;
        private readonly FunctionAuthorizationMetadataCollection _metadataStore;
        private readonly IOptionsMonitor<FunctionsAuthorizationOptions> _options;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionsAuthorizationProvider"/> class.
        /// </summary>
        /// <param name="schemeProvider">ASP.NET Core's scheme provider.</param>
        /// <param name="cache">The function filter cache.</param>
        /// <param name="options">The options object serving metadata. This doesn't change through the life of the application.</param>
        /// <param name="configOptions">The options object to manage configuration settings that might change from config source.</param>
        /// <param name="logger">Logger for class.</param>
        public FunctionsAuthorizationProvider(
            IAuthenticationSchemeProvider schemeProvider,
            IFunctionsAuthorizationFilterCache<int> cache,
            IOptions<FunctionsAuthorizationOptions> options,
            IOptionsMonitor<FunctionsAuthorizationOptions> configOptions,
            ILogger<IFunctionsAuthorizationProvider> logger)
        {
            Check.NotNull(schemeProvider, nameof(schemeProvider));
            Check.NotNull(cache, nameof(cache));
            Check.NotNull(options, nameof(options));
            Check.NotNull(configOptions, nameof(configOptions));
            Check.NotNull(logger, nameof(logger));

            _schemeProvider = schemeProvider;
            _filterCache = cache;
            _metadataStore = options.Value.AuthorizationMetadata;
            _options = configOptions;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<FunctionAuthorizationFilter> GetAuthorizationAsync(string functionName, IAuthorizationPolicyProvider policyProvider)
        {
            Check.NotNullOrWhiteSpace(functionName, nameof(functionName));

            var key = _metadataStore.GetFunctionAuthorizationId(functionName);

            if (_filterCache.TryGetFilter(key, out var filter))
            {
                _logger.LogDebug("Found cached filter for function.");
                return filter!;
            }

            var asyncKey = $"fap:{functionName}";

            await KeyedMonitor.EnterAsync(asyncKey, unblockOnFirstExit: true);

            try
            {
                if (_filterCache.TryGetFilter(key, out filter))
                {
                    _logger.LogDebug("Found cached filter for function while trying to generate one.");
                    return filter!;
                }

                _logger.LogDebug("Generating filter for function.");
                var functionMetadata = _metadataStore.GetMetadata(functionName);

                if (functionMetadata == FunctionAuthorizationMetadata.Empty)
                {
                    filter = new FunctionAuthorizationFilter(null);

                    _logger.LogWarning(Messages.NoAuthMetadataFoundForFunction, functionName);
                }
                else
                {
                    var policy = await GetPolicy(policyProvider, functionMetadata.AuthorizationData);
                    
                    filter = new FunctionAuthorizationFilter(policy, functionMetadata.AllowsAnonymousAccess);
                }

                // In order to comply with the cache policy,
                // we need to check if the policy provider allows caching policies.
#if NET7_0_OR_GREATER
                if (policyProvider.AllowsCachingPolicies)
                {
#endif
                    _filterCache.SetFilter(key, filter);
#if NET7_0_OR_GREATER
                }
#endif

                return filter;
            }
            finally
            {
                KeyedMonitor.Exit(asyncKey);
            }
        }

        private async Task<AuthorizationPolicy?> GetPolicy(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authData)
        {
            var authorizationData = authData.ToList();

            if (authorizationData.Count == 0)
            {
                return null;
            }

            var defaultScheme = await _schemeProvider.GetDefaultAuthenticateSchemeAsync();
            var needsScheme = authorizationData.All(ad => string.IsNullOrWhiteSpace(ad.AuthenticationSchemes));

            if (!needsScheme)
            {
                _logger.LogDebug("Using provided authentication schemes.");
                return await AuthorizationPolicy.CombineAsync(policyProvider, authorizationData);
            }
            else
            {
                _logger.LogDebug("Using empty scheme strategy to assign schemes for policy.");
                var strategy = _options.CurrentValue.EmptySchemeStrategy;

                if (strategy is EmptySchemeStrategy.UseDefaultScheme && defaultScheme is null)
                {
                    throw new InvalidOperationException(Messages.DefaultAuthSchemeNotSet);
                }

                // creating copy to not alter original definition.
                var copy = authorizationData.Select(ad =>
                    new AuthorizeInfoInternal
                    {
                        Policy = ad.Policy,
                        Roles = ad.Roles,
                        AuthenticationSchemes = ad.AuthenticationSchemes
                    }).ToList();

                if (strategy is EmptySchemeStrategy.UseDefaultScheme)
                {
                    _logger.LogDebug("Using default authentication scheme.");
                    copy[0].AuthenticationSchemes = defaultScheme!.Name;
                }
                else
                {
                    _logger.LogDebug("Using all available authentication schemes.");
                    var schemes = (await _schemeProvider.GetRequestHandlerSchemesAsync()).Select(s => s.Name).ToList();
                    schemes = schemes.Count > 0 ? schemes : (await _schemeProvider.GetAllSchemesAsync()).Select(s => s.Name).ToList();

                    copy[0].AuthenticationSchemes = string.Join(",", schemes.Except(__dismissedSchemes));
                }

                return await AuthorizationPolicy.CombineAsync(policyProvider, copy);
            }
        }

        private class AuthorizeInfoInternal : IAuthorizeData
        {
            public string? Policy { get; set; }
            public string? Roles { get; set; }
            public string? AuthenticationSchemes { get; set; }
        }
    }
}
