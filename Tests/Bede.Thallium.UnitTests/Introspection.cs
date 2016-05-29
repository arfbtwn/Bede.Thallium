using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Thallium.Introspection;

    using Dict = IDictionary<string, object>;

    [TestFixture]
    class Introspection
    {
        [Test]
        public void Single()
        {
            var sut = Api.Fluent().Api<IBar>().Call(x => x.Ping()).Get("ping").Back().Back();

            var api = Api.RestClient().Using(sut).Emit<IBar>();

            Assert.IsNotNull(api);
        }

        [Test]
        public void Fluent()
        {
            var sut = Api.Fluent();

            sut
                .Fallback<Simple>()
                .Api<IFluentFoo>()
                    .Delete("gamesession{/id*}")
                        .Method(api => api.DeleteSession(P.Uri<string[]>(),
                                                         P.Form<string>("string"),
                                                         P.Form<Dict>("first").FormUrl().Done(),
                                                         P.Form<Dict>("second", "theFile").Done(),
                                                         P.Form<FileStream>().Octet()))
                        .Multi("form-data", "BOUNDARY")
                        .Back()
                    .Delete("gamesession{/id,session}")
                        .Method(api => api.DeleteSession(P.Uri<long>(),
                                                         P.Uri<string>(),
                                                         P.Header("X-Correlation-Token"),
                                                         P.Header("X-Site-Code")));

            var sut2 = Api.Fluent().Api<IBar>().Get("ping").Method(api => api.Ping()).Back();

            sut.Include(sut2.Map);

            var rc = Api.RestClient()
                        .Using(sut)
                        .New<IFluentFoo>(new Uri("http://ew1-dv01-484-ilb.ad.bedegaming.com:3638/api/"));

            var bc = (RestClient) rc;
            bc.Head["X-Correlation-Token"] = "foo";
            bc.Head["X-Site-Code"]         = "site";

            var res = rc.Ping().Result;

            Assert.IsNotNull(res);

            dynamic dict = new ExpandoObject();
            dict.Foo = 2;
            dict.Bar = 5;

            var fs = File.OpenRead("..\\..\\app.config");

            rc.DeleteSession(12345, "mySession", "foobar", "sitecode").Wait();
            rc.DeleteSession(new [] { "123", "mySession" }, "arfbtwn", dict, dict, fs).Wait();
        }
    }

    public interface IFluentFoo : IBar
    {
        Task DeleteSession(long id, string session, string coral, string siteCode);

        Task DeleteSession(string[] id, string body1, Dict body2, Dict body3, FileStream theOtherFile);
    }
}