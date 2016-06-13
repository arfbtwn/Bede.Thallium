using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bede.Thallium.Polly;
using NUnit.Framework;
using Polly.CircuitBreaker;

namespace Bede.Thallium.UnitTests
{
    sealed class AlwaysThrow<E> : HttpMessageHandler where E : Exception, new()
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new E();
        }
    }

    [TestFixture]
    class Handlers
    {
        HttpRequestMessage m => new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost"));

        [Test]
        public void Building()
        {
            var r = new AlwaysThrow<Exception>().RetryOnUnknown(1);

            Exception rex = null, bex = null;

            r.Retry += (o, x) => { rex = x.Last; Console.WriteLine(x.Last.Message); Console.WriteLine("Next retry: " + x.Wait); };

            var b = r.BreakOnUnknown(1);

            b.Broken   += (o, x) => { bex = x.Last; Console.WriteLine(x.Last.Message); Console.WriteLine("Unbroken in: " + x.Wait); };
            b.Reset    += (o, x) => { };
            b.HalfOpen += (o, x) => { };

            var c = new HttpClient(b);

            try
            {
                c.SendAsync(m).GetAwaiter().GetResult();

                Assert.Fail();
            }
            catch (Exception) { }

            Assert.IsNotNull(rex);

            try
            {
                c.SendAsync(m).GetAwaiter().GetResult();

                Assert.Fail();
            }
            catch (BrokenCircuitException) { }

            Assert.IsNotNull(bex);

            Assert.IsTrue(b.IsOpen);
            b.Close();

            Assert.IsTrue(b.IsClosed);
            b.Open();

            try
            {
                c.SendAsync(m).GetAwaiter().GetResult();

                Assert.Fail();
            }
            catch (IsolatedCircuitException) { }

            Assert.IsTrue(b.IsIsolated);
            b.Close();
        }
    }
}
