using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Testing;

    [TestFixture]
    public class Mocking
    {
        [Test]
        public void Throws()
        {
            var sut = new Apis<IApi>()
                .Cases(x => x.Foo())
                    .Good(HttpStatusCode.OK)
                    .Bad (HttpStatusCode.NotFound)
                    .Ugly(HttpStatusCode.InternalServerError)
                    .Build()
                .Cases(x => x.Bar())
                    .Good(HttpStatusCode.OK)
                    .Bad (HttpStatusCode.NotFound)
                    .Ugly(HttpStatusCode.InternalServerError)
                    .Build();

            Assert.DoesNotThrowAsync(async () => await sut.Good.Foo());
            Assert.DoesNotThrowAsync(async () => await sut.Good.Bar());

            Assert.ThrowsAsync<HttpRequestException>(async () => await sut.Bad.Foo());
            Assert.ThrowsAsync<HttpRequestException>(async () => await sut.Bad.Bar());

            Assert.ThrowsAsync<HttpRequestException>(async () => await sut.Ugly.Foo());
            Assert.ThrowsAsync<HttpRequestException>(async () => await sut.Ugly.Bar());
        }

        public interface IApi
        {
            Task<object> Foo();
            Task         Bar();
        }
    }
}
