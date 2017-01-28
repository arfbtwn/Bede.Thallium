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
    class Content
    {
        [Test]
        public void String()
        {
            var sut = new ContentBuilder();

            var content = sut.String("foobar").Build();

            Assert.IsInstanceOf<StringContent>(content);
        }

        [Test]
        public void Form()
        {
            var sut = new ContentBuilder();

            var content = sut.FormUrl(new Dictionary<string, string> { { "foo", "bar" } }).Build();

            Assert.IsInstanceOf<FormUrlEncodedContent>(content);
        }

        [Test]
        public void Multi()
        {
            var sut = new ContentBuilder();

            sut.Multi("form-data", null);

            var content = sut.String("foobar").String("foobar").Build();

            Assert.IsInstanceOf<MultipartContent>(content);
        }
    }
}
