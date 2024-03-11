// <copyright file="FunctionsAuthorizationFeatureTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization.Features;

namespace Isolated.Tests.Features
{
    [TestClass]
    public class FunctionsAuthorizationFeatureTests
    {
        [TestMethod("Feature: should return name instance as passed in the constructor")]
        public void FeatureShouldReturnNameInstanceAsPassedInTheConstructor()
        {
            // Arrange
            var name = "TestFunction";

            // Act
            var feature = new FunctionsAuthorizationFeature(name);

            // Assert
            Assert.AreEqual(name, feature.Name);
        }
    }
}
