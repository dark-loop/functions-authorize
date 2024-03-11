// <copyright file="FunctionAuthorizationMetadataCollectionTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Abstractions.Tests.Fakes;
using DarkLoop.Azure.Functions.Authorization;

namespace Abstractions.Tests
{
    [TestClass]
    public class FunctionAuthorizationMetadataCollectionTests
    {
        [TestMethod("MetadataCollection: should return same instance for same type")]
        public void MetadataCollectionShouldReturnSameInstanceForSameType()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata1 = collection.Add(this.GetType(), out var existsFirst);
            var metadata2 = collection.Add(this.GetType(), out var existsSecond);

            // Assert
            Assert.AreSame(metadata1, metadata2);
            Assert.IsFalse(existsFirst);
            Assert.IsTrue(existsSecond);
        }

        [TestMethod("MetadataCollection: should return different instance for different type")]
        public void MetadataCollectionShouldReturnDifferentInstanceForDifferentType()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata1 = collection.Add(this.GetType(), out var existsFirst);
            var metadata2 = collection.Add(typeof(FunctionAuthorizationMetadataCollection), out var existsSecond);

            // Assert
            Assert.AreNotSame(metadata1, metadata2);
            Assert.IsFalse(existsFirst);
            Assert.IsFalse(existsSecond);
        }

        [TestMethod("MetadataCollection: should return same instance for same function and type")]
        public void MetadataCollectionShouldReturnSameInstanceForSameFunctionAndType()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata1 = collection.Add("TestFunction", this.GetType());
            var metadata2 = collection.Add("TestFunction", this.GetType());

            // Assert
            Assert.AreSame(metadata1, metadata2);
            Assert.AreEqual(metadata1.AuthorizationId, metadata2.AuthorizationId);
        }

        [TestMethod("MetadataCollection: should return different instance for different function and type")]
        public void MetadataCollectionShouldReturnDifferentInstanceForDifferentFunctionAndType()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata1 = collection.Add("TestFunction", this.GetType());
            var metadata2 = collection.Add("TestFunction", typeof(FunctionAuthorizationMetadataCollection));

            // Assert
            Assert.AreNotSame(metadata1, metadata2);
            Assert.AreNotEqual(metadata1.AuthorizationId, metadata2.AuthorizationId);
        }

        [TestMethod("MetadataCollection: should aggregate type and function metadata")]
        public void MetadataCollectionShouldAggregateTypeAndFunctionMetadata()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata1 = collection
                .Add(this.GetType(), out _)
                .AddAuthorizeData(new AuthorizeDataFake{ Policy = "Policy1" });

            var metadata2 = collection
                .Add("TestFunction", this.GetType())
                .AddAuthorizeData(new AuthorizeDataFake{ Policy = "Policy2"})
                .AllowAnonymousAccess();

            var single = collection
                .GetMetadata("TestFunction");

            // Assert
            Assert.AreNotSame(metadata1, metadata2);
            Assert.AreNotSame(metadata2, single);
            Assert.AreEqual(metadata2.AuthorizationId, single.AuthorizationId);
            Assert.AreEqual(2, single.AuthorizationData.Count);

            Assert.AreSame("Policy1", single.AuthorizationData[0].Policy);
            Assert.AreSame("Policy2", single.AuthorizationData[1].Policy);
            Assert.IsTrue(single.AllowsAnonymousAccess);
        }

        [TestMethod("MetadataCollection: AllowAnonymousAccess on type should inherit to function")]
        public void MetadataCollectionAllowAnonymousAccessOnTypeShouldInheritToFunction()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata1 = collection
                .Add(this.GetType(), out _)
                .AllowAnonymousAccess();

            var metadata2 = collection
                .Add("TestFunction", this.GetType());

            var metadata = collection
                .GetMetadata("TestFunction");

            // Assert
            Assert.AreNotSame(metadata1, metadata);
            Assert.IsTrue(metadata.AllowsAnonymousAccess);
        }

        [TestMethod("MetadataCollection: should return metadata for unregistered function")]
        public void MetadataCollectionShouldReturnEmptyMetadataForUnregisteredFunction()
        {
            // Arrange
            var collection = new FunctionAuthorizationMetadataCollection();

            // Act
            var metadata = collection.GetMetadata("TestFunction");

            // Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual(0, metadata.AuthorizationData.Count);
            Assert.IsFalse(metadata.AllowsAnonymousAccess);
        }
    }
}
