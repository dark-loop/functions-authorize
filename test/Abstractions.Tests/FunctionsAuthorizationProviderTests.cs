// <copyright file="FunctionsAuthorizationProviderTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Abstractions.Tests.Fakes;
using Common.Tests;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Cache;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Abstractions.Tests
{
    [TestClass]
    public class FunctionsAuthorizationProviderTests
    {
        private IOptions<FunctionsAuthorizationOptions>? _options;
        private Mock<IAuthorizationPolicyProvider>? _policyProviderMock;
        private ILogger<IFunctionsAuthorizationProvider>? _logger;
        private Action<LogLevel, EventId, object, Exception?, Delegate>? _onLog;
        private IOptionsMonitor<FunctionsAuthorizationOptions>? _configOptions;
        private IAuthenticationSchemeProvider? _schemeProvider;

        [TestInitialize]
        public void Initialize()
        {
            _onLog = null;
            _options = Options.Create(new FunctionsAuthorizationOptions());
            _logger = LoggerUtils.CreateLogger<IFunctionsAuthorizationProvider>(LogLevel.Warning, ActOnLog);

            _policyProviderMock = new Mock<DefaultAuthorizationPolicyProvider>(Options.Create(new AuthorizationOptions())).As<IAuthorizationPolicyProvider>();

#if NET7_0_OR_GREATER
            _policyProviderMock.Setup(x => x.AllowsCachingPolicies).Returns(true);
#endif

            var configOptionsMock = new Mock<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            configOptionsMock.SetupGet(m => m.CurrentValue).Returns(_options!.Value);

            _configOptions = configOptionsMock.Object;

            var schemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
            schemeProviderMock
                .Setup(x => x.GetDefaultAuthenticateSchemeAsync())
                .ReturnsAsync(new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)));

            schemeProviderMock
                .Setup(x => x.GetRequestHandlerSchemesAsync())
                .ReturnsAsync(new[] { new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)) });

            _schemeProvider = schemeProviderMock.Object;
        }

        [TestMethod("AuthorizationProvider: should return filter with no policy when metadata is null")]
        public async Task AuthorizationProviderShouldReturnFilterWithNoPolicyWhenMetadataIsNull()
        {
            var messages = new List<(LogLevel Level, string Message)>();

            // Arrange
            var provider = new FunctionsAuthorizationProvider(
                _schemeProvider!, new FunctionsAuthorizationFilterCache<int>(), _options!, _configOptions!, _logger!);

            _onLog = (level, eventId, state, exception, formatter) =>
            {
                messages.Add((level, (formatter?.DynamicInvoke(state, exception) as string)!));
            };

            // Act
            var filter = await provider.GetAuthorizationAsync("TestFunction", _policyProviderMock!.Object);

            // Assert
            Assert.IsNotNull(filter);
            Assert.IsNull(filter.Policy);
            Assert.IsFalse(filter.AllowAnonymous);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(LogLevel.Warning, messages[0].Level);
            Assert.AreEqual(string.Format(Messages.NoAuthMetadataFoundForFunction, "TestFunction"), messages[0].Message);
        }

        [TestMethod("AuthorizationProvider: should return filter with no policy when metadata present")]
        public async Task AuthorizationProviderShouldReturnFilterWithNoPolicyWhenMetadataPresent()
        {
            var messages = new List<(LogLevel Level, string Message)>();

            _options!.Value.SetFunctionAuthorizationInfo("TestFunction", this.GetType());

            // Arrange
            var provider = new FunctionsAuthorizationProvider(
                _schemeProvider!, new FunctionsAuthorizationFilterCache<int>(), _options!, _configOptions!, _logger!);

            _onLog = (level, eventId, state, exception, formatter) =>
            {
                messages.Add((level, (formatter?.DynamicInvoke(state, exception) as string)!));
            };

            // Act
            var filter = await provider.GetAuthorizationAsync("TestFunction", _policyProviderMock!.Object);

            // Assert
            Assert.IsNotNull(filter);
            Assert.IsNull(filter.Policy);
            Assert.IsFalse(filter.AllowAnonymous);
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod("AuthorizationProvider: should return filter with policy when metadata present")]
        public async Task AuthorizationProviderShouldReturnFilterWithPolicyWhenMetadataPresent()
        {
            var messages = new List<(LogLevel Level, string Message)>();

            _options!.Value
                .SetFunctionAuthorizationInfo("TestFunction", this.GetType())
                .AddAuthorizeData(new AuthorizeDataFake());

            _policyProviderMock!
                .Setup(x => x.GetDefaultPolicyAsync())
                .ReturnsAsync(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            // Arrange
            var provider = new FunctionsAuthorizationProvider(
                _schemeProvider!, new FunctionsAuthorizationFilterCache<int>(), _options!, _configOptions!, _logger!);

            _onLog = (level, eventId, state, exception, formatter) =>
            {
                messages.Add((level, (formatter?.DynamicInvoke(state, exception) as string)!));
            };

            // Act
            var filter = await provider.GetAuthorizationAsync("TestFunction", _policyProviderMock!.Object);

            // Assert
            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Policy);
            Assert.IsFalse(filter.AllowAnonymous);
            Assert.AreEqual(0, messages.Count);
        }

        private void ActOnLog(LogLevel level, EventId eventId, object state, Exception? exception, Delegate formatter)
        {
            _onLog?.Invoke(level, eventId, state, exception, formatter);
        }
    }
}
