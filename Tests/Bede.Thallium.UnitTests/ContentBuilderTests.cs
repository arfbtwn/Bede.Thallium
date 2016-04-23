using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net.Http;
using Bede.Thallium.Content;

namespace Bede.Thallium.UnitTests
{
    [TestFixture]
    class ContentBuilderTests
    {
        [Test]
        public void Multi()
        {
            var b = new ContentBuilder(new MediaTypeFormatterCollection());

            b.Multi("form-data", null);

            b.String("Foo Bar")      .FormData("first")
             .String("Hello, World!").FormData("second");

            b.FormUrl(new Dictionary<string, string> { { "foo", "bar" }, { "baz", "foobar" } })
             .FormData("third");

            b.Bytes(Encoding.UTF8.GetBytes("1234")).Octet().FormData("theFile", "someFile");

            var c = b.Build();

            Assert.IsInstanceOf(typeof(MultipartContent), c);

            var client = new HttpClient();

            var task = Task.Run(() => client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost.:80/foo")
                {
                    Content = c
                }));

            var msg = task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
