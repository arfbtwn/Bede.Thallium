﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bede.Thallium.Handlers;
using Bede.Thallium.Polly;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    [TestFixture]
    class Handlers
    {
        HttpRequestMessage m => new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost"));

        [Test]
        public void Syntax()
        {
            const int tries = 1, limit = 2;

            var ms50 = TimeSpan.FromMilliseconds(50);

            var count  = 0;
            var broken = false;

            var mock = new Mock<HttpMessageHandler>();

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)))
                .Callback(() => ++count);

            var sut = mock.Object
                .Record(rec =>
                {
                    rec.Request  += (o, x) => { };
                    rec.Response += (o, x) => { };
                })
                .Retry(tries, i => ms50, ret =>
                {
                    ret.Retry += (o, x) => { };
                })
                .On(x => x.StatusCode.IsServerError())
                .Break(limit, ms50, brk =>
                {
                    brk.Broken   += (o, x) => { broken = true; };
                    brk.Reset    += (o, x) => { };
                    brk.HalfOpen += (o, x) => { };

                    Assert.IsTrue (brk.IsClosed);
                    Assert.IsFalse(brk.IsOpen);
                })
                .On(x => x.StatusCode.IsServerError())
                .ThrowOnFail()
                .WrapAll();

            Assert.IsNotNull(sut);

            using (var client = new HttpClient(sut))
            {
                for (var i = 0; i < limit + 1; ++i)
                {
                    Assert.Throws<HttpRequestException>(async () => await client.SendAsync(m));
                }
            }

            Assert.AreEqual((tries + 1) * limit, count);
            Assert.IsTrue(broken);
        }
    }
}
