// <copyright file="FunctionsAuthorizationMetadataMiddlewareTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using DarkLoop.Azure.Functions.Authorization;
using DarkLoop.Azure.Functions.Authorization.Metadata;
using Isolated.Tests.Fakes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Moq;

namespace Isolated.Tests.Metadata
{
    [TestClass]
    public partial class FunctionsAuthorizationMetadataMiddlewareTests
    {
        private IOptions<FunctionsAuthorizationOptions>? _options;

        [TestInitialize]
        public void Setup()
        {
            _options = Options.Create(new FunctionsAuthorizationOptions());
        }

        [TestMethod("MetadataMiddleware: should skip over non http trigger")]
        public async Task MetadataMiddlewareShouldSkipOverNonHttpTrigger()
        {
            // Arrange
            var functionId = "098039840";
            var metadata = new FunctionsAuthorizationMetadataMiddleware(_options!);
            var functionContext = SetupFunctionContext(functionId, "TestFunction", "TestFunction", "timerTrigger", "cron");

            // Act
            await metadata.Invoke(functionContext, async fc => await Task.CompletedTask);

            // Assert
            Assert.AreEqual(0, _options!.Value.AuthorizationMetadata.Count);
        }

        [TestMethod("MetadataMiddleware: should register function when not registered")]
        public async Task MetadataMiddlewareShouldRegisterFunctionWhenNotRegistered()
        {
            // Arrange
            var functionId = "098039840";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var metadata = new FunctionsAuthorizationMetadataMiddleware(_options!);
            var functionContext = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request");

            // Act
            await metadata.Invoke(functionContext, async fc => await Task.CompletedTask);

            // Assert
            Assert.AreEqual(2, _options!.Value.AuthorizationMetadata.Count);
            Assert.IsTrue(_options!.Value.IsFunctionRegistered("TestFunction"));
        }

        [TestMethod("MetadataMiddleware: should not register function when already registered")]
        public async Task MetadataMiddlewareShouldNotRegisterFunctionWhenAlreadyRegistered()
        {
            // Arrange
            var functionId = "098039840";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction";
            var metadata = new FunctionsAuthorizationMetadataMiddleware(_options!);
            var functionContext = SetupFunctionContext(functionId, "TestFunction", entryPoint, "httpTrigger", "request");

            // Act
            await metadata.Invoke(functionContext, async fc => await Task.CompletedTask);
            await metadata.Invoke(functionContext, async fc => await Task.CompletedTask);

            // Assert
            Assert.AreEqual(2, _options!.Value.AuthorizationMetadata.Count);
            Assert.IsTrue(_options!.Value.IsFunctionRegistered("TestFunction"));
        }

        [TestMethod("MetadataMiddleware: should throw when defining function with internal method")]
        public async Task MetadataMiddlewareShouldThrowWhenDefiningFunctionWithInternalMethod()
        {
            // Arrange
            var functionId = "098039841";
            var entryPoint = $"{typeof(FakeFunctionClass).FullName}.TestFunction2";
            var metadata = new FunctionsAuthorizationMetadataMiddleware(_options!);
            var functionContext = SetupFunctionContext(functionId, "TestFunction2", entryPoint, "httpTrigger", "request");

            // Act
            var exception = await Assert.ThrowsExceptionAsync<MethodAccessException>(
                async () => await metadata.Invoke(functionContext, async fc => await Task.CompletedTask));

            // Assert
            Assert.IsNotNull(exception);
        }

        private FunctionContext SetupFunctionContext(string functionId, string functionName, string entryPoint, string triggerType, string boundTriggerParamName)
        {
            var binding = new Mock<BindingMetadata>();
            binding.SetupGet(binding => binding.Type).Returns(triggerType);

            var bindingsBuilder = ImmutableDictionary.CreateBuilder<string, BindingMetadata>();
            bindingsBuilder.Add(boundTriggerParamName, binding.Object);

            var context = new Mock<FunctionContext>();
            context.Setup(context => context.FunctionId).Returns(functionId);
            context.Setup(context => context.FunctionDefinition.Name).Returns(functionName);
            context.Setup(context => context.FunctionDefinition.EntryPoint).Returns(entryPoint);
            context.Setup(context => context.Features).Returns(Mock.Of<IInvocationFeatures>());
            context
                .Setup(contextMock => contextMock.FunctionDefinition.InputBindings)
                .Returns(bindingsBuilder.ToImmutable());
            return context.Object;
        }
    }
}
