// <copyright file="HttpUtils.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Common.Tests
{
    public static class HttpUtils
    {
        public static HttpContext SetupHttpContext(IServiceProvider services)
        {
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();
            var requestMock = new Mock<HttpRequest>();
            var requestStream = new MemoryStream();
            var streamReader = PipeReader.Create(requestStream);
            var requestHeaders = new HeaderDictionary();
            var streamWriter = PipeWriter.Create(responseStream);

            httpContextMock.SetupGet(x => x.RequestServices).Returns(services);
            httpContextMock.SetupGet(x => x.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(x => x.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(x => x.Features).Returns(Mock.Of<IFeatureCollection>());
            httpContextMock.SetupGet(x => x.Items).Returns(new Dictionary<object, object?>());
            requestMock.SetupGet(x => x.RouteValues).Returns(new RouteValueDictionary());
            requestMock.SetupGet(x => x.Body).Returns(requestStream);
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);
            requestMock.SetupGet(x => x.HttpContext).Returns(httpContextMock.Object);
            responseMock.SetupGet(x => x.HasStarted).Returns(false);
            responseMock.SetupGet(x => x.Body).Returns(responseStream);
            responseMock.SetupGet(x => x.BodyWriter).Returns(streamWriter);
            responseMock.SetupGet(x => x.Headers).Returns(new HeaderDictionary());
            responseMock.SetupGet(x => x.HttpContext).Returns(httpContextMock.Object);

            return httpContextMock.Object;
        }
    }
}
