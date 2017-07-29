using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Clients;
    using Thallium.Introspection;

    public class FooController : ApiController
    {
        [HttpGet, System.Web.Http.Route("ping")]
        public Task<Ping> Ping() => Task.FromResult(new Ping());

        [HttpDelete, System.Web.Http.Route("gamesession/{id}/{session}")]
        public Task Delete(long id, string session) => Task.FromResult(true);

        [HttpPost, System.Web.Http.Route]
        public Task<Ping> Create(Ping body) => Task.FromResult(body);

        [System.Web.Http.Route("{id}")]
        [HttpGet]
        public Task<Ping> Read(long id) => Task.FromResult(new Ping());

        [HttpPut, System.Web.Http.Route]
        public Task<Ping> Update(Ping body) => Task.FromResult(body);

        [System.Web.Http.Route("{id}")]
        [HttpDelete]
        public Task Delete(long id) => Task.FromResult(true);

        [HttpPut, System.Web.Http.Route("valueType")]
        public Task Put(long id) => Task.FromResult(true);
    }

    [TestFixture]
    class Generation
    {
        readonly HttpServer _server;

        public Generation()
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            _server = new HttpServer(config);
        }

        public void ObsoleteSyntax()
        {
#pragma warning disable 618
            Api.On<TestClient>().New<ICrudApi<Ping>>("http://localhost");
#pragma warning restore 618
        }

        public void Syntax()
        {
            var uri = new Uri("http://localhost");

            using (Api.Rest().New<IFoo>(uri)) { }
        }

        public void Concurrent()
        {
            IFoo api = null;

            var tasks = new List<Task>();

            for(var i = 0; i < 5; ++i)
            {
                tasks.Add(Task.Run(() =>
                {
                    Assert.DoesNotThrow(() => api = Api.Rest().New<IFoo>(new Uri("http://locahost")));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.IsNotNull(api);
        }

        [Test]
        public void Extension()
        {
            var type = Api.On<TestClient>().Emit<IFoo>();

            Assert.IsNotNull(type);
            Assert.IsTrue(typeof(TestClient).IsAssignableFrom(type));
        }

        [Test]
        public void Builder()
        {
            var fluent = Api.Fluent();

            fluent.Api<IBar>().Get("ping").Method(x => x.Ping());

            var sut = Api.Rest().Using(fluent);

            var bar = sut.New<IBar>(new Uri("http://localhost"), _server);

            var result = bar.Ping().Result;

            Assert.IsNotNull(result);
        }

        [Test]
        public void CrudClientGen()
        {
            var sut = Api.Rest().New<ICrudApi<Ping>>(new Uri("http://localhost"), _server);

            Assert.IsNotNull(sut);

            var p1 = sut.Create(new Ping()).Result;

            Assert.IsNotNull(p1);
        }

        [Test]
        public void DynamicClientGen()
        {
            var sut = Api.Dynamic().New<IFoo>(new FixedConfig("http://localhost") { Handler = _server });

            Assert.IsNotNull(sut);

            var p1 = sut.Ping().Result;

            Assert.IsNotNull(p1);
        }

        [Test]
        public void ValueTypeBody()
        {
            var sut = Api.Rest().New<IValueType>(new Uri("http://localhost"), _server);

            Assert.DoesNotThrow(() => sut.Put(1).Wait());
        }

        [Test]
        public void StaticHeaders()
        {
            var sut = Api.Rest().New<IStaticHeader>(new Uri("http://localhost"), _server);

            Assert.DoesNotThrow(() => sut.Post().Wait());
        }

        [Test]
        public void Exceptions()
        {
            Assert.Throws<ArgumentNullException>(() => Api.Rest().New<IFoo>(null));
        }

        [Test]
        public void Cancellation()
        {
            var sut = Api.Rest().New<ICancellationToken>(new Uri("http://localhost"), _server);

            sut.None().Wait();
            sut.Value().Wait();
            sut.Nullable().Wait();
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

    public interface IValueType
    {
        [Put("valueType")]
        Task Put(long body);
    }

    [Type("application/myapp")]
    public interface IStaticHeader
    {
        [Post]
        Task Post();
    }

    public interface ICancellationToken
    {
        [Get]
        Task None();

        [Get]
        Task Value(CancellationToken token = default(CancellationToken));

        [Get]
        Task Nullable(CancellationToken? token = null);
    }
}
