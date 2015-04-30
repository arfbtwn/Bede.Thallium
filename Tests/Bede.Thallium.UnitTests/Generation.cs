using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Bede.Thallium.UnitTests
{
    using NUnit.Framework;

    [TestFixture]
    class Generation
    {
        [Test]
        public void RestClientGen()
        {
            var sut = Api<TestClient, IFoo>.New(new Uri("http://ew1-dv01-484-01.ad.bedegaming.com:3638"));

            var rc = sut as TestClient;

            Assert.IsNotNull(rc);

            rc.Head["X-Correlation-Token"] = "coral";
            rc.Head["X-Site-Code"] = "mysite.com";

            var res = sut.Ping().Result;

            Assert.IsNotNull(res);

            dynamic dict = new ExpandoObject();
            dict.Foo = 2;
            dict.Bar = 5;

            sut.DeleteSession(12345, "mySession", "foobar", "sitecode").ConfigureAwait(false).GetAwaiter().GetResult();
            sut.DeleteSession(new [] { "123", "mySession" }, dict).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    public class TestClient : RestClient
    {
        public TestClient(Uri uri) : base(uri) { }
    }

    public class Ping
    {
        public string RespondingHost { get; set; }
        public string Version        { get; set; }
    }

    [Route("api")]
    public interface IBar
    {
        [Get("ping")]
        Task<Ping> Ping();
    }

    [Route("api")]
    public interface IFoo : IBar
    {
        [Delete, Route("gamesession{/id,session}")]
        Task DeleteSession(long id, string session, [Header("X-Correlation-Token")] string coral, [Header("X-Site-Code")] string siteCode);

        [Delete("gamesession{/id*}"), FormUrl]
        Task DeleteSession(string[] id, IDictionary<string, object> body);
    }
}
