﻿namespace PassKitHelper
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class PassKitMiddlewareTests_Passes : PassKitMiddlewareTestBase
    {
        private readonly PathString pathBase = "/v1/passes";

        private readonly string passType = "sometype";

        private readonly string passSerial = "someserial";

        private readonly string authToken = "sometoken";

        private readonly MemoryStream body;

        public PassKitMiddlewareTests_Passes()
        {
            httpContext.Request.Method = "GET";
            httpContext.Request.Path = pathBase + $"/{passType}/{passSerial}";
            httpContext.Request.Headers["Authorization"] = $"ApplePass {authToken}";

            body = new MemoryStream();
            httpContext.Response.Body = body;
        }

        [Fact]
        public async Task DoNothingOnNonGET()
        {
            httpContext.Request.Method = "POST";

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(405, httpContext.Response.StatusCode);
            mocks.Verify();
        }

        [Fact]
        public async Task DoNothingOnMoreSegments()
        {
            httpContext.Request.Path += "/extra-segment";

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(400, httpContext.Response.StatusCode);
            mocks.Verify();
        }

        [Fact]
        public async Task DoNothingOnLessSegments()
        {
            httpContext.Request.Path = pathBase + $"/{passType}";

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(400, httpContext.Response.StatusCode);
            mocks.Verify();
        }

        [Fact]
        public async Task DoNothingWithoutAuth()
        {
            httpContext.Request.Headers.Remove("Authorization");

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(401, httpContext.Response.StatusCode);
            mocks.Verify();
        }

        [Fact]
        public async Task DoNothingWithWrongAuth()
        {
            httpContext.Request.Headers["Authorization"] = "invalid value";

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(401, httpContext.Response.StatusCode);
            mocks.Verify();
        }

        [Fact]
        public async Task ServiceIsCalled()
        {
            var obj = JObject.Parse("{ \"ok\": 1 }");

            passkitServiceMock
                .Setup(x => x.GetPassAsync(passType, passSerial, authToken, null))
                .ReturnsAsync((200, obj))
                .Verifiable();

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(200, httpContext.Response.StatusCode);
            mocks.Verify();

            Assert.Equal(PassInfoBuilder.MimeContentType, httpContext.Response.ContentType);

            httpContext.Response.Body.Position = 0;
            using var sr = new StreamReader(httpContext.Response.Body);
            var resp = await sr.ReadToEndAsync();

            Assert.Equal("{\"ok\":1}", resp);
        }

        [Fact]
        public async Task ServiceIsCalled_WithIfModifiedSince()
        {
            var dt = DateTimeOffset.UtcNow.AddHours(-1);

            httpContext.Request.Headers["If-Modified-Since"] = dt.ToString("R");

            DateTimeOffset? actualDt = null;

            passkitServiceMock
                .Setup(x => x.GetPassAsync(passType, passSerial, authToken, It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync((304, null))
                .Callback<string, string, string, DateTimeOffset?>((x, y, z, d) => { actualDt = dt; })
                .Verifiable();

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(304, httpContext.Response.StatusCode);
            mocks.Verify();

            Assert.NotNull(actualDt);
            Assert.Equal(dt, actualDt);
        }

        [Fact]
        public async Task ServiceIsCalled_Non200Result()
        {
            passkitServiceMock
                .Setup(x => x.GetPassAsync(passType, passSerial, authToken, null))
                .ReturnsAsync((304, null))
                .Verifiable();

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(304, httpContext.Response.StatusCode);
            Assert.Equal(0, httpContext.Response.Body.Length);
        }

        [Fact]
        public async Task ExceptionThrownOnWrongServiceResult()
        {
            passkitServiceMock
                .Setup(x => x.GetPassAsync(passType, passSerial, authToken, null))
                .ReturnsAsync((200, null))
                .Verifiable();

            await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(httpContext));
        }
    }
}