using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Formatting;
    using Thallium.Content;

    [TestFixture]
    class Behaviours
    {
        [Test]
        public void Formatters()
        {
            var sut = new MediaTypeFormatterCollection();

            Assert.Throws<ArgumentNullException>(() => sut.FindWriter(typeof(object), null));
        }

        [Test]
        public void Content()
        {
            var sut = new ObjectContent<int>(1, new MediaTypeFormatterCollection());

            Assert.IsNull   (sut.Headers.ContentType);
            sut.Ready();
            Assert.IsNotNull(sut.Headers.ContentType);
        }

        [Test]
        public void FormUrl()
        {
            var sut = new FormUrlEncoder();

            Assert.IsNotEmpty(sut.SupportedMediaTypes);

            Assert.IsTrue (sut.CanReadType(typeof(FormDataCollection)));
            Assert.IsFalse(sut.CanReadType(typeof(IEnumerable<KeyValuePair<string, string>>)));

            Assert.IsTrue (sut.CanWriteType(typeof(FormDataCollection)));
            Assert.IsTrue (sut.CanWriteType(typeof(IEnumerable<KeyValuePair<string, string>>)));
            Assert.IsFalse(sut.CanWriteType(typeof(IEnumerable<KeyValuePair<object, object>>)));
        }
    }
}
