// <copyright file="FunctionAuthorizationTypeMapTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using DarkLoop.Azure.Functions.Authorization;

namespace Abstractions.Tests
{
    [TestClass]
    public class FunctionAuthorizationTypeMapTests
    {
        [TestMethod("FunctionAuthorizationTypeMap: should return same instance for same type")]
        public void FunctionAuthorizationTypeMapShouldReturnSameInstanceForSameType()
        {
            // Arrange
            var map = new FunctionAuthorizationTypeMap();
            var functionName = "TestFunction";

            // Act
            var result1 = map.AddFunctionType(functionName, this.GetType());
            var result2 = map.AddFunctionType(functionName, this.GetType());

            // Assert
            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod("FunctionAuthorizationTypeMap: indexer should return type for existing map")]
        public void FunctionAuthorizationTypeMapIndexerShouldReturnTypeForExistingMap()
        {
            // Arrange
            var map = new FunctionAuthorizationTypeMap();
            var functionName = "TestFunction";
            map.AddFunctionType(functionName, this.GetType());

            // Act
            var result = map[functionName];

            // Assert
            Assert.AreEqual(this.GetType(), result);
        }

        [TestMethod("FunctionAuthorizationTypeMap: indexer should return null for non-existing map")]
        public void FunctionAuthorizationTypeMapIndexerShouldReturnNullForNonExistingMap()
        {
            // Arrange
            var map = new FunctionAuthorizationTypeMap();
            var functionName = "TestFunction";

            // Act
            var result = map[functionName];

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod("FunctionAuthorizationTypeMap: IsFunctionRegistered should return true for existing map")]
        public void FunctionAuthorizationTypeMapIsFunctionRegisteredShouldReturnTrueForExistingMap()
        {
            // Arrange
            var map = new FunctionAuthorizationTypeMap();
            var functionName = "TestFunction";
            map.AddFunctionType(functionName, this.GetType());

            // Act
            var result = map.IsFunctionRegistered(functionName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod("FunctionAuthorizationTypeMap: IsFunctionRegistered should return false for non-existing map")]
        public void FunctionAuthorizationTypeMapIsFunctionRegisteredShouldReturnFalseForNonExistingMap()
        {
            // Arrange
            var map = new FunctionAuthorizationTypeMap();
            var functionName = "TestFunction";

            // Act
            var result = map.IsFunctionRegistered(functionName);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
