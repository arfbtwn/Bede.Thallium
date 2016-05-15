using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    [TestFixture]
    class Generation
    {
        [Test]
        public void CrudClientGen()
        {
            var sut = Api<TestClient, ICrudApi<Ping>>.New(new Uri("http://localhost.:80"));

            Assert.IsNotNull(sut);

            sut.Create(new Ping()).Wait();
            sut.Create(new Ping()).Wait();
        }

        [Test]
        public void RestClientGen()
        {
            var sut = Api<TestClient, IFoo>.New(new Uri("http://ew1-dv01-484-ilb.ad.bedegaming.com:3638"));

            var rc = sut as TestClient;

            Assert.IsNotNull(rc);

            rc.Head["X-Correlation-Token"] = "coral";
            rc.Head["X-Site-Code"] = "mysite.com";

            var res = sut.Ping().Result;

            Assert.IsNotNull(res);

            sut.DeleteSession(12345, "mySession", "foobar", "sitecode").Wait();

            dynamic dict = new ExpandoObject();
            dict.Foo = 2;
            dict.Bar = 5;

            var fs = File.OpenRead("..\\..\\app.config");

            sut.DeleteSession(new [] { "123", "mySession" }, "arfbtwn", dict, dict, fs).Wait();
        }
    }

    public class TestClient : RestClient
    {
        public TestClient(Uri uri) : base(uri) { }
        public TestClient(Uri uri, HttpMessageHandler handler) : base(uri, handler) { }
    }

    public class Ping
    {
        public string RespondingHost { get; set; }
        public string Version        { get; set; }
    }

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

        [Delete("gamesession{/id*}"), Multipart("form-data", "BOUNDARY")]
        Task DeleteSession(string[] id, [FormData("string")]            string                      body1,
                                        [FormData("first"), FormUrl]    IDictionary<string, object> body2,
                                        [FormData("second", "theFile")] IDictionary<string, object> body3,
                                        [FormData, Octet]               FileStream                  theOtherFile);
    }

    public interface ICrudApi<T>
    {
        [Post]           Task<T> Create(T body);

        [Get("{id}")]    Task<T> Read(long id);

        [Put]            Task<T> Update(T body);

        [Delete("{id}")] Task    Delete(long id);
    }
}
