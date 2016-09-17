using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Clients;
    using Thallium.Introspection;

    [TestFixture]
    class Generation
    {
        public void ObsoleteSyntax()
        {
#pragma warning disable 612, 618
            Api.Emit(typeof(IFoo));
            Api.Emit(typeof(IFoo), typeof(RestClient));
            Api.New<IFoo>("http://localhost.:80");

            Api<TestClient, ICrudApi<Ping>>.New("http://localhost.:80");
            Api<IFoo>.New("http://localhost.:80");

            Api.On<TestClient>().New<ICrudApi<Ping>>("http://localhost.:80");
#pragma warning restore 612, 618
        }

        public void Syntax()
        {
            var uri = new Uri("http://localhost.80");

            using (Api.Rest().New<IBar>(uri)) { }
            using (Api.Rest().New<IFoo>(uri)) { }
        }

        [Test]
        public void Extension()
        {
            var type = Api.On<TestClient>().Emit<IFoo>();

            Assert.IsNotNull(type);
        }

        [Test]
        public void Builder()
        {
            var fluent = Api.Fluent().Fallback<Simple>();

            fluent.Api<IBar>().Get("ping").Method(x => x.Ping());

            var sut = Api.Rest().Using(fluent);

            var bar = sut.New<IBar>(new Uri("http://localhost.:80"));

            var result = bar.Ping().Result;

            Assert.IsNotNull(result);
        }

        [Test]
        public void CrudClientGen()
        {
            var sut = Api.Rest().New<ICrudApi<Ping>>(new Uri("http://localhost.:80"));

            Assert.IsNotNull(sut);

            sut.Create(new Ping()).Wait();
            sut.Create(new Ping()).Wait();
        }

        [Test]
        public void RestClientGen()
        {
            var sut = Api.Rest().New<IFoo>(new Uri("http://localhost.:80"));

            var rc = sut as RestClient;

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

        [Test]
        public void DynamicClientGen()
        {
            var sut = Api.Dynamic().New<IFoo>(new FixedConfig("http://localhost.:80"));

            Assert.IsNotNull(sut);
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

    public interface IBar : IDisposable
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
