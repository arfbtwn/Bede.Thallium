using System;
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
            Assert.IsNull(sut.Grant);
            Assert.IsNull(sut.Client);
            Assert.IsNull(sut.Secret);
            Assert.IsEmpty(sut.Scope);
            Assert.IsEmpty(sut.Acr);

            var grant  = "mygrant";
            var client = "myclient";
            var secret = "mysecret";
            var scope  = new [] { "scope1",   "scope2"   };
            var acr    = new [] { "acr1:foo", "acr2:bar" };

            sut = new OAuth
            {
                Grant  = grant,
                Client = client,
                Secret = secret,
                Scope  = scope,
                Acr    = acr
            };

            Assert.AreEqual(grant,  sut.Grant);
            Assert.AreEqual(client, sut.Client);
            Assert.AreEqual(secret, sut.Secret);
            Assert.AreEqual(scope,  sut.Scope);
            Assert.AreEqual(acr,    sut.Acr);

            Assert.AreNotSame(scope, sut.Scope);
            Assert.AreNotSame(acr,   sut.Acr);
        }

        [Test]
        public void Expiration()
        {
            var sut = new Cache
            {
                Token = new Token { expires_in = 1 }
            };

            Assert.IsFalse(sut.Expired);

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Assert.IsTrue(sut.Expired);
        }

        [Test]
        public void Call()
        {
            _oauth.Setup       (x => x.Auth(It.IsAny<OAuth>(), It.IsAny<Basic>()))
                  .ReturnsAsync(default(Token))
                  .Verifiable();

            var sut = _oauth.Object.Tracker<OAuth>();

            sut.Auth().Wait();

            _oauth.Verify();
        }
    }
}
