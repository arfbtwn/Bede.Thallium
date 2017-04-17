using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Data;
    using Templating;

    [TestFixture]
    public class Templates
    {
        static readonly DateTime?      Null   = null;
        static readonly DateTime       Date   = DateTime.UtcNow;
        static readonly DateTimeOffset Offset = DateTimeOffset.UtcNow;

        [Test]
        public void Nested()
        {
            const string gen1 = "{?nested}";
            const string gen2 = "{?nested*}";

            var nested3 = new List<object> { "foo", "bar", new List<object> { "baz" } };

            var nested2 = new Dictionary<string, object>
            {
                { "foobar", new [] { "bar", "foo" } }
            };

            var nested1 = new Dictionary<string, object>
            {
                { "foo", new [] { "foo", "bar" } },
                { "bar", nested2 },
                { "baz", nested3 }
            };

            var param = new Dictionary<string, object>
            {
                { "nested", nested1 }
            };

            const string expect1 = "?nested=foo,foo,bar,foobar,bar,foo,baz,foo,bar,baz";
            const string expect2 = "?foo=foo,bar&foobar=bar,foo&baz=foo,bar,baz";

            var sut = new Runtime();

            var text1 = sut.Expand(gen1, param);
            var text2 = sut.Expand(gen2, param);

            Assert.AreEqual(expect1, text1);
            Assert.AreEqual(expect2, text2);
        }

        [Test]
        public void Bytes()
        {
            const string gen = "{?bytes}";

            var sut      = new Runtime();
            var param    = new Dictionary<string, object> { { "bytes", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } } };

            var expected = "?bytes=" + Uri.EscapeDataString("AQIDBAUGBwg=");
            var text     = sut.Expand(gen, param);

            Assert.AreEqual(expected, text);
        }

        [Test]
        public void Dates()
        {
            const string gen = "{?now}";

            var sut      = new Runtime();
            var param    = new Dictionary<string, object>();
            var expected = (string) null;
            var text     = (string) null;

            param["now"] = Date;
            expected     = "?now=" + Uri.EscapeDataString(Date.ToString("o"));
            text         = sut.Expand(gen, param);

            Assert.AreEqual(expected, text);

            param["now"] = Offset;
            expected     = "?now=" + Uri.EscapeDataString(Offset.ToString("o"));
            text         = sut.Expand(gen, param);

            Assert.AreEqual(expected, text);

            param["now"] = (Iso) Null;
            expected     = string.Empty;
            text         = sut.Expand(gen, param);

            Assert.AreEqual(expected, text);
        }

        [Test]
        public void Explode()
        {
            var sut = new Explode<MyClass>(new MyClass());

            var dict = sut.ToDictionary(x => x.Key, x => x.Value);

            Assert.AreEqual(3,     dict.Count);
            Assert.AreEqual(1,     dict["Foo"]);
            Assert.AreEqual("bar", dict["Bar"]);
            Assert.AreEqual(Date,  dict["Foobar"]);

            var rfc = new Runtime();

            var expected = "?Foo=1&Bar=bar&Foobar=" + Uri.EscapeDataString(Date.ToString("o"));

            var text = rfc.Expand("{?foo*}", new Dictionary<string, object> { { "foo", sut } });

            Assert.AreEqual(expected, text);
        }

        class MyClass
        {
            public int      Foo    { get; set; } = 1;
            public string   Bar    { get; set; } = "bar";
            public DateTime Foobar { get; set; } = Date;
        }
    }
}
