using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Clients;
    using Thallium.Introspection;

    using Dict = IDictionary<string, object>;

    [TestFixture]
    class Introspection
    {
        [Test]
        public void Single()
        {
            var sut = Api.Fluent();

            sut.Api<IBar>().Call(x => x.Ping()).Get("ping");

            var api = Api.Rest().Using(sut).Emit<IBar>();

            Assert.IsNotNull(api);
        }

        [Test]
        public void Fluent()
        {
            var sut = Api.Fluent();

            sut
                .Api<IFluentFoo>()
                    .Delete("gamesession{/id*}")
                        .Method(api => api.DeleteSession(P.Uri<string[]>(),
                                                         P.Form<string>("string"),
                                                         P.Form<Dict>("first").FormUrl(),
                                                         P.Form<Dict>("second", "theFile"),
                                                         P.Form<FileStream>().Octet()))
                        .Multi("form-data", "BOUNDARY")
                        .Back()
                    .Delete("gamesession{/id,session}")
                        .Method(api => api.DeleteSession(P.Uri<long>(),
                                                         P.Uri<string>(),
                                                         P.Header("X-Correlation-Token"),
                                                         P.Header("X-Site-Code")));

            var sut2 = Api.Fluent();

            sut2.Api<IBar>().Get("ping").Method(api => api.Ping());

            Assert.AreEqual(2, sut.Map.Count);
            Assert.AreEqual(1, sut2.Map.Count);

            sut.Include(sut2.Map);

            Assert.AreEqual(3, sut.Map.Count);
        }
    }

    public interface IFluentFoo : IBar
    {
        Task DeleteSession(long id, string session, string coral, string siteCode);

        Task DeleteSession(string[] id, string body1, Dict body2, Dict body3, FileStream theOtherFile);
    }
}