// <copyright file="FunctionAuthorizationMetadataTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abstractions.Tests.Fakes;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Abstractions.Tests
{
    [TestClass]
    public class FunctionAuthorizationMetadataTests
    {
        [TestMethod("Metadata: should return the same number of authorization data elements it received")]
        public void Metadata_ShouldReturnTheSameNumberOfAuthorizationDataElementsItReceived()
        {
            // Arrange
            var metadata = new FunctionAuthorizationMetadata("TestFunction", this.GetType());

            // Act
            metadata
                .AddAuthorizeData(new AuthorizeDataFake())
                .AddAuthorizeData(new[] { new AuthorizeDataFake(), new AuthorizeDataFake() });


            // Assert
            Assert.AreEqual(3, metadata.AuthorizationData.Count);
        }
    }
}
