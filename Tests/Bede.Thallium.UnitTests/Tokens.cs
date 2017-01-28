﻿using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Auth;

    [TestFixture]
    class Tokens
    {
        readonly Mock<IOAuth2> _oauth = new Mock<IOAuth2>();

        [Test]
        public void Request()
        {
            var sut = new OAuth();

            Assert.IsEmpty(sut);
            Assert.IsNull(sut.GrantType);
            Assert.IsNull(sut.Client);
            Assert.IsNull(sut.Secret);
            Assert.IsNull(sut.Scopes);
            Assert.IsNull(sut.AcrValues);

            var grant  = "mygrant";
            var client = "myclient";
            var secret = "mysecret";
            var scopes = new [] { "scope1",   "scope2"   };
            var acr    = new [] { "acr1:foo", "acr2:bar" };

            sut = new OAuth
            {
                GrantType = grant,
                Client    = client,
                Secret    = secret,
                Scopes    = scopes,
                AcrValues = acr
            };

            Assert.AreEqual(5,      sut.Count);
            Assert.AreEqual(grant,  sut.GrantType);
            Assert.AreEqual(client, sut.Client);
            Assert.AreEqual(secret, sut.Secret);
            Assert.AreEqual(scopes, sut.Scopes);
            Assert.AreEqual(acr,    sut.AcrValues);

            Assert.AreNotSame(scopes, sut.Scopes);
            Assert.AreNotSame(acr,    sut.AcrValues);
        }

        [Test]
        public void Expiration()
        {
            var sut = new Cache
            {
                Token = new Token { expires_in = 1 }
            };

            Assert.IsFalse(sut.Expired);

            Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.IsTrue(sut.Expired);
        }

        [Test]
        public void Call()
        {
            _oauth.Setup       (x => x.Auth(It.IsAny<OAuth>()))
                  .ReturnsAsync(default(Token))
                  .Verifiable();

            var sut = _oauth.Object.Tracker<OAuth>();

            sut.Request.GrantType = "client_credentials";
            sut.Request.Client    = "myclient";
            sut.Request.Secret    = "secret";
            sut.Request.Scopes    = new [] { "scope1" };

            sut.Auth().Wait();

            _oauth.Verify();
        }
    }
}
