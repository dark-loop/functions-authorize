// <copyright file="UnitTest1.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using DarkLoop.Azure.Functions.Authorization.Extensions;
using Microsoft.Azure.Functions.Worker;
using Moq;

namespace Isolated.Tests
{
    [TestClass]
    public class FunctionContextExtensionsTests
    {
        [TestMethod("FunctionContextExtensions: should return true when httpTrigger binding")]
        public void FunctionContextExtensionsShouldReturnTrueWhenHttpTriggerBinding()
        {
            // Arrange
            var binding = new Mock<BindingMetadata>();
            binding.SetupGet(binding => binding.Type).Returns("httpTrigger");

            var bindingsBuilder = ImmutableDictionary.CreateBuilder<string, BindingMetadata>();
            bindingsBuilder.Add("request", binding.Object);

            var contextMock = new Mock<FunctionContext>();
            contextMock
                .Setup(contextMock => contextMock.FunctionDefinition.InputBindings)
                .Returns(bindingsBuilder.ToImmutable());

            // Act
            var result = contextMock.Object.IsHttpTrigger();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod("FunctionContextExtensions: should return false when no httpTrigger binding")]
        public void FunctionContextExtensionsShouldReturnFalseWhenNoHttpTriggerBinding()
        {
            // Arrange
            var binding = new Mock<BindingMetadata>();
            binding.SetupGet(binding => binding.Type).Returns("timerTrigger");

            var bindingsBuilder = ImmutableDictionary.CreateBuilder<string, BindingMetadata>();
            bindingsBuilder.Add("cron", binding.Object);

            var contextMock = new Mock<FunctionContext>();
            contextMock
                .Setup(contextMock => contextMock.FunctionDefinition.InputBindings)
                .Returns(bindingsBuilder.ToImmutable());

            // Act
            var result = contextMock.Object.IsHttpTrigger();

            // Assert
            Assert.IsFalse(result);
        }
    }
}