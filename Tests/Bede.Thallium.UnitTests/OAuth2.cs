using System;
using Bede.Thallium.Auth;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    [TestFixture]
    class OAuth2
    {
        static IOAuth2 Sut()
        {
            return Api.Rest().New<IOAuth2>(new Uri("http://ew1-dv01-501-ilb.ad.bedegaming.com:8888/core/"));
        }

        [Test]
        public void Jwks()
        {
            var sut = Sut();

            var keys = sut.Keys().Result;

            Assert.IsNotNull(keys);
            Assert.IsNotEmpty(keys.keys);
        }

        [Test]
        public void ClientCredentials()
        {
            var sut = Sut();

            var req = new Client
            {
                Client = "test_client_credentials",
                Secret = "secret",

                Scopes = new [] { "sitecode" }
            };

            var tok = sut.Auth(req).Result;

            Assert.IsNotNull(tok);
            Assert.IsNotEmpty(tok.access_token);
        }

        [Test]
        public void ResourceOwner()
        {
            var sut = Sut();

            var req = new Owner
            {
                Client   = "test_client_ro",
                Secret   = "secret",
                Username = "aladin",
                Password = "open sesame",

                Scopes   = new [] { "sitecode" }
            };

            var tok = sut.Auth(req).Result;

            Assert.IsNotNull(tok);
            Assert.IsNotEmpty(tok.access_token);
        }

        [Test]
        public void Refresh()
        {
            var sut = Sut();

            var r1 = new Owner
            {
                Client   = "test_client_ro",
                Secret   = "secret",
                Username = "aladin",
                Password = "open sesame",

                Scopes   = new [] { "sitecode", "offline_access" }
            };

            var t1 = sut.Auth(r1).Result;

            Assert.IsNotNull(t1);
            Assert.IsNotEmpty(t1.access_token);
            Assert.IsNotEmpty(t1.refresh_token);

            var r2 = new Refresh(r1, t1);

            var t2 = sut.Auth(r2).Result;

            Assert.IsNotNull(t2);
            Assert.IsNotEmpty(t2.access_token);
        }
    }
}