// <copyright file="IntegrationTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.Net;

namespace Isolated.Tests
{
    [TestClass]
    public class ConcurrentTests
    {
        [TestMethod]
        [Ignore("This is to test middleware concurrency")]
        public async Task TestFunctionAuthorizationMetadataCollectionAsync()
        {
            // Arrange
            var client = new HttpClient { BaseAddress = new Uri("http://localhost:7005/") };

            // Act
            var message1 = new HttpRequestMessage(HttpMethod.Get, "api/testfunction");
            var message2 = new HttpRequestMessage(HttpMethod.Get, "api/testfunction");
            var message3 = new HttpRequestMessage(HttpMethod.Get, "api/testfunction");
            var message4 = new HttpRequestMessage(HttpMethod.Get, "api/testfunction");
            var request1 = client.SendAsync(message1);
            var request2 = client.SendAsync(message2);
            var request3 = client.SendAsync(message3);
            var request4 = client.SendAsync(message4);

            // Assert
            await Task.WhenAll(request1, request2, request3, request4);
            Assert.AreEqual(HttpStatusCode.Unauthorized, request1.Result.StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, request2.Result.StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, request3.Result.StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, request4.Result.StatusCode);
        }
    }
}
