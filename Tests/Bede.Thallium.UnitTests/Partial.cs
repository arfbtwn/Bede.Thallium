using System;
using System.Threading.Tasks;
using NUnit.Framework;

using Bede.Thallium.Clients;

#pragma warning disable 618

namespace Bede.Thallium.UnitTests
{
    [TestFixture]
    public class Partial
    {
        [Test]
        public void Test()
        {
            IBar sut = null;

            Assert.DoesNotThrow(() => sut = Api.On<TestClient>().New<IFoo>(new Uri("http://localhost")));

            Assert.Throws<NotImplementedException>(() => sut.Ping());
        }

        public interface IFoo : IBar
        {
            [Get]
            Task Foo();
        }

        public interface IBar
        {
            [Get]
            Task Ping();

            [Get]
            Task Pong();
        }

        public abstract class TestClient : RestClient, IBar
        {
            public TestClient(Uri uri) : base(uri) { }

            public Task Ping()
            {
                throw new NotImplementedException();
            }

            public abstract Task Pong();
        }
    }
}
