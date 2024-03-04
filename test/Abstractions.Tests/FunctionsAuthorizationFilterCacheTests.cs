// <copyright file="UnitTest1.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Abstractions.Tests
{
    [TestClass]
    public class FunctionsAuthorizationFilterCacheTests
    {
        [TestMethod("FilterCache: SetFilter should not replace existing instance")]
        public void SetFilterShouldNotReplaceExisting()
        {
            // Arrange
            var cache = new FunctionsAuthorizationFilterCache<int>();
            var filter = new FunctionAuthorizationFilter(null, true);

            // Act
            cache.SetFilter(1, filter);
            cache.SetFilter(1, new FunctionAuthorizationFilter(
                new AuthorizationPolicy(
                    new[] { new DenyAnonymousAuthorizationRequirement() },
                    new[] { JwtBearerDefaults.AuthenticationScheme })));

            // Assert
            cache.TryGetFilter(1, out var extractedFilter);
            Assert.AreSame(filter, extractedFilter);
            Assert.AreSame(null, filter.Policy);
        }
    }
}