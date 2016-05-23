using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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

            var api = Api<IBar>.Emit(sut);

            Assert.IsNotNull(api);
        }

        [Test]
        public void Fluent()
        {
            var sut = Api.Fluent();

            sut
                .Fallback<Simple>()
                .Api<IFoo>()
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

            var sut2 = Api.Fluent().Api<IBar>().Get("ping").Method(api => api.Ping()).Back();

            sut.Include(sut2);

            Api<IFoo>.Emit(sut);

            var rc = Api<IFoo>.New(new Uri("http://ew1-dv01-484-ilb.ad.bedegaming.com:3638/api"));

            var bc = rc as RestClient;
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
}
