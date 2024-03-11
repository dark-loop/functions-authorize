// <copyright file="FunctionsAuthorizationMiddlewareTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using Common.Tests;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Properties;
using Isolated.Tests.Fakes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Isolated.Tests
{
    [TestClass]
    public class FunctionsAuthorizationMiddlewareTests
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
                .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services
                .AddFunctionsAuthorization()
                .AddFunctionsAuthorizationCore()
                .Configure<FunctionsAuthorizationOptions>(delegate { });

            _services = services.BuildServiceProvider();
        }

        [TestMethod("AuthorizationMiddleware: should throw when no supported environment")]
        public async Task AuthorizationMiddlewareShouldThrowWhenNoSupportedEnvironment()
        {
            // Arrange
            int value = 0;
            var middleware = new FunctionsAuthorizationMiddleware(
                _services!.GetRequiredService<IFunctionsAuthorizationProvider>(),
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                _services!.GetRequiredService<IPolicyEvaluator>(),
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationMiddleware>>());

            var functionId = "098039841";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var context = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request");

            // Act
            Func<Task> act = async () => await middleware.Invoke(context, async fc =>
            {
                value = 1;
                await Task.CompletedTask;
            });

            // Assert
            Assert.AreEqual(0, value);
            await Assert.ThrowsExceptionAsync<NotSupportedException>(act);
        }

        [TestMethod("AuthorizationMiddleware: should not authorize when disabled")]
        public async Task AuthorizationMiddlewareShouldNotAuthorizeWhenDisabled()
        {
            // Arrange
            int value = 0;
            var options = _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
            options.CurrentValue.AuthorizationDisabled = true;

            var logs = new List<(LogLevel Level, string Message)>();
            var logger = LoggerUtils.CreateLogger<FunctionsAuthorizationMiddleware>(
                LogLevel.Warning,
                (level, eventId, state, exception, formatter) =>
                {
                    logs.Add((level, (formatter?.DynamicInvoke(state, exception) as string)!));
                });

            var middleware = new FunctionsAuthorizationMiddleware(
                _services!.GetRequiredService<IFunctionsAuthorizationProvider>(),
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                _services!.GetRequiredService<IPolicyEvaluator>(),
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                logger);

            var functionId = "098039841";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request", httpContext);

            // Act
            await middleware.Invoke(context, async fc =>
            {
                value = 1;
                await Task.CompletedTask;
            });

            // Assert
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(string.Format(IsolatedMessages.FunctionAuthIsDisabled, httpContext.Request.GetDisplayUrl()), logs[0].Message);
        }

        [TestMethod("AuthorizationMiddleware: should skip authorization with no policy")]
        public async Task AuthorizationMiddlewareShouldSkipAuthorizationWithNoPolicy()
        {
            // Arrange
            int value = 0;
            var authorizationProviderMock = new Mock<IFunctionsAuthorizationProvider>();
            authorizationProviderMock.Setup(provider => provider.GetAuthorizationAsync(It.IsAny<string>(), It.IsAny<IAuthorizationPolicyProvider>()))
                .ReturnsAsync(new FunctionAuthorizationFilter(null));

            var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

            var middleware = new FunctionsAuthorizationMiddleware(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationMiddleware>>());

            var functionId = "098039841";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request", httpContext);

            // Act
            await middleware.Invoke(context, async fc =>
            {
                value = 1;
                await Task.CompletedTask;
            });

            // Assert
            Assert.AreEqual(1, value);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()), Times.Never);
        }

        [TestMethod("AuthorizationMiddleware: should execute next delegate when failed authentication and allows anonymous")]
        public async Task AuthorizationMiddlewareShouldExecuteNextDelegateWhenFailedAuthenticationAndAllowsAnonymous()
        {
            // Arrange
            int value = 0;
            var authorizationProviderMock = new Mock<IFunctionsAuthorizationProvider>();
            authorizationProviderMock
                .Setup(provider => provider.GetAuthorizationAsync(It.IsAny<string>(), It.IsAny<IAuthorizationPolicyProvider>()))
                .ReturnsAsync(new FunctionAuthorizationFilter(new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build(), true));

            var policyEvaluatorMock = new Mock<IPolicyEvaluator>();
            policyEvaluatorMock
                .Setup(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()))
                .ReturnsAsync(AuthenticateResult.Fail("Failed authentication"));

            var middleware = new FunctionsAuthorizationMiddleware(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationMiddleware>>());

            var functionId = "098039841";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request", httpContext);

            // Act
            await middleware.Invoke(context, async fc =>
            {
                value = 1;
                await Task.CompletedTask;
            });

            // Assert
            Assert.AreEqual(1, value);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()), Times.Once);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()), Times.Never);
        }

        [TestMethod("AuthorizationMiddleware: should not execute next delegate when failed authentication and does not allow anonymous")]
        public async Task AuthorizationMiddlewareShouldNotExecuteNextDelegateWhenFailedAuthenticationAndDoesNotAllowAnonymous()
        {
            // Arrange
            int value = 0;
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

            var middleware = new FunctionsAuthorizationMiddleware(
                authorizationProviderMock.Object,
                _services!.GetRequiredService<IFunctionsAuthorizationResultHandler>(),
                _services!.GetRequiredService<IAuthorizationPolicyProvider>(),
                policyEvaluatorMock.Object,
                _services!.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>(),
                _services!.GetRequiredService<ILogger<FunctionsAuthorizationMiddleware>>());

            var functionId = "098039841";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var httpContext = HttpUtils.SetupHttpContext(_services!);
            var context = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request", httpContext);

            // Act
            await middleware.Invoke(context, async fc =>
            {
                value = 1;
                await Task.CompletedTask;
            });

            // Assert
            Assert.AreEqual(0, value);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()), Times.Once);
            policyEvaluatorMock.Verify(evaluator => evaluator.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()), Times.Once);
        }

        private FunctionContext SetupFunctionContext(
            string functionId, string functionName, string entryPoint, string triggerType, string boundTriggerParamName, HttpContext? httpContext = null)
        {
            var binding = new Mock<BindingMetadata>();
            binding.SetupGet(binding => binding.Type).Returns(triggerType);

            var bindingsBuilder = ImmutableDictionary.CreateBuilder<string, BindingMetadata>();
            bindingsBuilder.Add(boundTriggerParamName, binding.Object);

            var items = new Dictionary<object, object>();

            if (httpContext != null)
            {
                items.Add("HttpRequestContext", httpContext);
            }

            var context = new Mock<FunctionContext>();
            context.Setup(context => context.FunctionId).Returns(functionId);
            context.Setup(context => context.FunctionDefinition.Name).Returns(functionName);
            context.Setup(context => context.FunctionDefinition.EntryPoint).Returns(entryPoint);
            context.Setup(context => context.Features).Returns(Mock.Of<IInvocationFeatures>());
            context.Setup(context => context.Items).Returns(items);
            context
                .Setup(contextMock => contextMock.FunctionDefinition.InputBindings)
                .Returns(bindingsBuilder.ToImmutable());
            return context.Object;
        }
    }
}
