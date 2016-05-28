using System;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Clients;

    [TestFixture]
    class Config
    {
        [Test]
        public void Dynamic()
        {
            var foo = new Foo { Uri = new Uri("http://localhost:80") };

            var sut = new DynamicConfig(() => foo.Uri);

            var first = foo.Uri;

            foo.Uri = new Uri("http://example.com");

            Assert.AreNotEqual(first,   sut.Uri);
            Assert.AreEqual   (foo.Uri, sut.Uri);
        }

        class Foo
        {
            public Uri Uri { get; set; }
        }
    }
}
