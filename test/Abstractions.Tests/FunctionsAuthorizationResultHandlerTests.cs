// <copyright file="FunctionsAuthorizationResultHandlerTests.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System.IO.Pipelines;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Abstractions.Tests
{
    [TestClass]
    public class FunctionsAuthorizationResultHandlerTests
    {
        private IServiceProvider? _services;
        private IOptionsMonitor<FunctionsAuthorizationOptions>? _options;
        private Mock<IAuthenticationService>? _authServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthenticationService>();

            var services = new ServiceCollection();
            services.Configure<FunctionsAuthorizationOptions>(delegate { });
            services.AddSingleton(_authServiceMock.Object);
            services.AddMvcCore();
            services.AddLogging();

            _services = services.BuildServiceProvider();
            _options = _services.GetRequiredService<IOptionsMonitor<FunctionsAuthorizationOptions>>();
        }

        [TestMethod("ResultHandler: should invoke onSuccess when authorization succeeds")]
        public async Task ResultHandlerShouldInvokeOnSuccessWhenAuthorizationSucceeds()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestServices).Returns(_services!);

            var authBuilder = new AuthorizationPolicyBuilder();
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContextMock.Object, policy, PolicyAuthorizationResult.Success());

            var onSuccessInvoked = false;
            Func<HttpContext, Task> onSuccess = _ =>
            {
                onSuccessInvoked = true;
                return Task.CompletedTask;
            };

            // Act
            await handler.HandleResultAsync(context, httpContextMock.Object, onSuccess);

            // Assert
            Assert.IsTrue(onSuccessInvoked);
        }

        [TestMethod("ResultHandler: should return UnauthorizedResult when authorization fails with no scheme IAuthenticationService is invoked")]
        public async Task ResultHandlerShouldReturnUnauthorizedResultWhenAuthorizationFails()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestServices).Returns(_services!);

            var authBuilder = new AuthorizationPolicyBuilder();
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContextMock.Object, policy, PolicyAuthorizationResult.Challenge());

            // Act
            await handler.HandleResultAsync(context, httpContextMock.Object);

            // Assert
            _authServiceMock!.Verify(x => x.ChallengeAsync(httpContextMock.Object, null, null), Times.Once);
        }

        [TestMethod("ResultHandler: should return UnauthorizedResult when authorization fails with scheme IAuthenticationService is invoked")]
        public async Task ResultHandlerShouldReturnUnauthorizedResultWhenAuthorizationFailsWithScheme()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestServices).Returns(_services!);

            var authBuilder = new AuthorizationPolicyBuilder("TestScheme");
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContextMock.Object, policy, PolicyAuthorizationResult.Challenge());

            // Act
            await handler.HandleResultAsync(context, httpContextMock.Object);

            // Assert
            _authServiceMock!.Verify(x => x.ChallengeAsync(httpContextMock.Object, "TestScheme", null), Times.Once);
        }

        [TestMethod("ResultHandler: should return ForbidResult when authorization fails with no scheme IAuthenticationService is invoked")]
        public async Task ResultHandlerShouldReturnForbidResultWhenAuthorizationFails()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestServices).Returns(_services!);

            var authBuilder = new AuthorizationPolicyBuilder();
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContextMock.Object, policy, PolicyAuthorizationResult.Forbid());

            // Act
            await handler.HandleResultAsync(context, httpContextMock.Object);

            // Assert
            _authServiceMock!.Verify(x => x.ForbidAsync(httpContextMock.Object, null, null), Times.Once);
        }

        [TestMethod("ResultHandler: should return ForbidResult when authorization fails with scheme IAuthenticationService is invoked")]
        public async Task ResultHandlerShouldReturnForbidResultWhenAuthorizationFailsWithScheme()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestServices).Returns(_services!);

            var authBuilder = new AuthorizationPolicyBuilder("TestScheme");
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContextMock.Object, policy, PolicyAuthorizationResult.Forbid());

            // Act
            await handler.HandleResultAsync(context, httpContextMock.Object);

            // Assert
            _authServiceMock!.Verify(x => x.ForbidAsync(httpContextMock.Object, "TestScheme", null), Times.Once);
        }

        [TestMethod("ResultHandler: should return unauthorized in response when fails with Challenge")]
        public async Task ResultHandlerShouldReturnUnauthorizedInResponseWhenFailsWithChallenge()
        {
            // Arrange
            _options!.CurrentValue.WriteHttpStatusToResponse = true;

            var httpContext = SetupHttpContext();

            var authBuilder = new AuthorizationPolicyBuilder();
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContext, policy, PolicyAuthorizationResult.Challenge());

            // Act
            await handler.HandleResultAsync(context, httpContext);

            // Assert
            Assert.AreEqual("Unauthorized".Length, httpContext.Response.Body.Length);
        }

        [TestMethod("ResultHandler: should return forbidden in response when fails with Forbid")]
        public async Task ResultHandlerShouldReturnForbiddenInResponseWhenFailsWithForbid()
        {
            // Arrange
            _options!.CurrentValue.WriteHttpStatusToResponse = true;

            var httpContext = SetupHttpContext();

            var authBuilder = new AuthorizationPolicyBuilder();
            authBuilder.Requirements.Add(new FakeRequirement());
            var policy = authBuilder.Build();

            var handler = new FunctionsAuthorizationResultHandler(_options!);
            var context = new FunctionAuthorizationContext<HttpContext>("TestFunction", httpContext, policy, PolicyAuthorizationResult.Forbid());

            // Act
            await handler.HandleResultAsync(context, httpContext);

            // Assert
            Assert.AreEqual("Forbidden".Length, httpContext.Response.Body.Length);
        }

        private HttpContext SetupHttpContext()
        {
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var requestMock = new Mock<HttpRequest>();
            var stream = new MemoryStream();
            var requestHeaders = new HeaderDictionary();
            var streamWriter = PipeWriter.Create(stream);

            httpContextMock.SetupGet(x => x.RequestServices).Returns(_services!);
            httpContextMock.SetupGet(x => x.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(x => x.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(x => x.Features).Returns(Mock.Of<IFeatureCollection>());
            requestMock.SetupGet(x => x.RouteValues).Returns(new RouteValueDictionary());
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);
            responseMock.SetupGet(x => x.HasStarted).Returns(false);
            responseMock.SetupGet(x => x.Body).Returns(stream);
            responseMock.SetupGet(x => x.BodyWriter).Returns(streamWriter);

            return httpContextMock.Object;
        }

        private class FakeRequirement : IAuthorizationRequirement
        {
        }
    }
}
