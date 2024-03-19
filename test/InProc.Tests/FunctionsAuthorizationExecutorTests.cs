// <copyright file="FunctionsAuthorizationExecutorTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Common.Tests;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Properties;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace InProc.Tests
{
    [TestClass]
    public class FunctionsAuthorizationExecutorTests
    {
        private IServiceProvider? _services;

        [TestInitialize]
        public void Initialize()
        {
            var config = new ConfigurationBuilder().Build();
            var services = new ServiceCollection() as IServiceCollection;
            services
                .AddSingleton<IConfiguration>(config)
                .AddLogging()
                .AddFunctionsAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(delegate { }, true);

            services
                .AddFunctionsAuthorization()
                .AddFunctionsAuthorizationCore()
                .Configure<FunctionsAuthorizationOptions>(delegate { });

            _services = services.BuildServiceProvider();
        }

        [TestMethod("AuthorizationExecutor: should not authorize when disabled")]
        public async Task AuthorizationExecutorShouldNotAuthorizeWhenDisabled()
        {
            // Arrange
            var options = _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            options.CurrentValue.AuthorizationDisabled = true;

            var logs = new List<(LogLevel Level, string Message)>();
            var logger = LoggerUtils.CreateLogger<FunctionsAuthorizationExecutor>(
                LogLevel.Warning,
                (level, eventId, state, exception, formatter) =>
                {
                    logs.Add((level, (formatter?.DynamicInvoke(state, exception) as string)!));
                });

            var executor = new FunctionsAuthorizationExecutor(
                _services!.GetRequiredService<IFunctionsAuthorizationProvider>(),
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                _services!.GetRequiredService<IPolicyEvaluator>(),
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                logger);

            var functionId = Guid.NewGuid();
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupExecutingContext(functionId, "TestFunction", httpContext.Request);

            // Act
            await executor.ExecuteAuthorizationAsync(context, httpContext);

            // Assert
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(string.Format(Strings.DisabledAuthorization, httpContext.Request.GetDisplayUrl()), logs[0].Message);
        }

        [TestMethod("AuthorizationExecutor: should skip authorization with no policy")]
        public async Task AuthorizationMiddlewareShouldSkipAuthorizationWithNoPolicy()
        {
            // Arrange
            var options = _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            options.CurrentValue.AuthorizationDisabled = false;

            var authorizationProviderMock = new Mock<IFunctionsAuthorizationProvider>();
            authorizationProviderMock.Setup(provider => provider.GetAuthorizationAsync(It.IsAny<string>(), It.IsAny<IAuthorizationPolicyProvider>()))
                .ReturnsAsync(new FunctionAuthorizationFilter(null));

            var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

            var executor = new FunctionsAuthorizationExecutor(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationExecutor>>());

            var functionId = Guid.NewGuid();
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupExecutingContext(functionId, "TestFunction", httpContext.Request);

            // Act
            await executor.ExecuteAuthorizationAsync(context, httpContext);

            // Assert
            
        }

        [TestMethod("AuthorizationExecutor: should execute next delegate when failed authentication and allows anonymous")]
        public async Task AuthorizationExecutorShouldExecuteNextDelegateWhenFailedAuthenticationAndAllowsAnonymous()
        {
            // Arrange
            var options = _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            options.CurrentValue.AuthorizationDisabled = false;

            var authorizationProviderMock = new Mock<IFunctionsAuthorizationProvider>();
            authorizationProviderMock
                .Setup(provider => provider.GetAuthorizationAsync(It.IsAny<string>(), It.IsAny<IAuthorizationPolicyProvider>()))
                .ReturnsAsync(new FunctionAuthorizationFilter(new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build(), true));

            var policyEvaluatorMock = new Mock<IPolicyEvaluator>();
            policyEvaluatorMock
                .Setup(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()))
                .ReturnsAsync(AuthenticateResult.Fail("Failed authentication"));

            var executor = new FunctionsAuthorizationExecutor(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationExecutor>>());

            var functionId = Guid.NewGuid();
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupExecutingContext(functionId, "TestFunction", httpContext.Request);

            // Act
            Func<Task> action = async () => await executor.ExecuteAuthorizationAsync(context, httpContext);

            // Assert
            await action.Should().NotThrowAsync();
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()), Times.Once);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()), Times.Never);
        }

        [TestMethod("AuthorizationExecutor: should throw when failed authentication and does not allow anonymous with Challenge")]
        public async Task AuthorizationExecutorShouldNotReturnWhenFailedAuthenticationAndDoesNotAllowAnonymous()
        {
            // Arrange
            var options = _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            options.CurrentValue.AuthorizationDisabled = false;

            var authorizationProviderMock = new Mock<IFunctionsAuthorizationProvider>();
            authorizationProviderMock
                .Setup(provider => provider.GetAuthorizationAsync(It.IsAny<string>(), It.IsAny<IAuthorizationPolicyProvider>()))
                .ReturnsAsync(new FunctionAuthorizationFilter(new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build(), false));

            var policyEvaluatorMock = new Mock<IPolicyEvaluator>();
            policyEvaluatorMock
                .Setup(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()))
                .ReturnsAsync(AuthenticateResult.Fail("Failed authentication"));
            policyEvaluatorMock
                .Setup(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()))
                .ReturnsAsync(PolicyAuthorizationResult.Challenge());

            var executor = new FunctionsAuthorizationExecutor(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationExecutor>>());

            var functionId = Guid.NewGuid();
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupExecutingContext(functionId, "TestFunction", httpContext.Request);

            // Act
            Func<Task> action = async () => await executor.ExecuteAuthorizationAsync(context, httpContext);

            // Assert
            await action.Should().ThrowAsync<FunctionAuthorizationException>();
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()), Times.Once);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()), Times.Once);
        }

        [TestMethod("AuthorizationExecutor: should throw when failed authentication and does not allow anonymous with Forbid")]
        public async Task AuthorizationExecutorShouldThrowWhenFailedAuthenticationAndDoesNotAllowAnonymousWithForbid()
        {
            // Arrange
            var options = _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            options.CurrentValue.AuthorizationDisabled = false;

            var authorizationProviderMock = new Mock<IFunctionsAuthorizationProvider>();
            authorizationProviderMock
                .Setup(provider => provider.GetAuthorizationAsync(It.IsAny<string>(), It.IsAny<IAuthorizationPolicyProvider>()))
                .ReturnsAsync(new FunctionAuthorizationFilter(new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build(), false));

            var policyEvaluatorMock = new Mock<IPolicyEvaluator>();
            policyEvaluatorMock
                .Setup(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()))
                .ReturnsAsync(AuthenticateResult.Fail("Failed authentication"));
            policyEvaluatorMock
                .Setup(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()))
                .ReturnsAsync(PolicyAuthorizationResult.Forbid());

            var executor = new FunctionsAuthorizationExecutor(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationExecutor>>());

            var functionId = Guid.NewGuid();
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupExecutingContext(functionId, "TestFunction", httpContext.Request);

            // Act
            Func<Task> action = async () => await executor.ExecuteAuthorizationAsync(context, httpContext);

            // Assert
            await action.Should().ThrowAsync<FunctionAuthorizationException>();
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()), Times.Once);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()), Times.Once);
        }

        private static FunctionExecutingContext SetupExecutingContext(Guid functionId, string functionName, HttpRequest request)
        {
            var args = new Dictionary<string, object>
            {
                ["request"] = request
            };

            var contextMock = new Mock<FunctionExecutingContext>(args, new Dictionary<string, object>(), functionId, functionName, LoggerUtils.CreateLogger<FunctionExecutingContext>());
            return contextMock.Object;
        }
    }
}