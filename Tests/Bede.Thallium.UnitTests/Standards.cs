using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Templating;

    [TestFixture]
    class Standards
    {
        internal static readonly Dictionary<string, object> Params = new Dictionary<string, object>
        {
            { "count",      new [] { "one", "two", "three" } },
            { "dom",        new [] { "example", "com" } },
            { "dub",        "me/too" },
            { "hello",      "Hello World!" },
            { "half",       "50%" },
            { "var",        "value" },
            { "who",        "fred" },
            { "base",       "http://example.com/home/" },
            { "path",       "/foo/bar" },
            { "list",       new List<string> { "red", "green", "blue" } },
            { "keys",       new Dictionary<string, string> { { "semi", ";" }, { "dot", "." }, { "comma", "," } } },
            { "v",          6 },
            { "x",          1024 },
            { "y",          768 },
            { "empty",      string.Empty },
            { "empty_keys", new Dictionary<string, string>() },
            { "undef",      null },
        };

        internal static IEnumerable Cases()
        {
            yield return new TestCaseData("{count}", "one,two,three");
            yield return new TestCaseData("{count*}", "one,two,three");
            yield return new TestCaseData("{/count}", "/one,two,three");
            yield return new TestCaseData("{/count*}", "/one/two/three");
            yield return new TestCaseData("{;count}", ";count=one,two,three");
            yield return new TestCaseData("{;count*}", ";count=one;count=two;count=three");
            yield return new TestCaseData("{?count}", "?count=one,two,three");
            yield return new TestCaseData("{?count*}", "?count=one&count=two&count=three");
            yield return new TestCaseData("{&count*}", "&count=one&count=two&count=three");
            yield return new TestCaseData("{var}", "value");
            yield return new TestCaseData("{hello}", "Hello%20World%21");
            yield return new TestCaseData("{half}", "50%25");
            yield return new TestCaseData("O{empty}X", "OX");
            yield return new TestCaseData("O{undef}X", "OX");
            yield return new TestCaseData("{x,y}", "1024,768");
            yield return new TestCaseData("{x,hello,y}", "1024,Hello%20World%21,768");
            yield return new TestCaseData("?{x,empty}", "?1024,");
            yield return new TestCaseData("?{x,undef}", "?1024");
            yield return new TestCaseData("?{undef,y}", "?768");
            yield return new TestCaseData("{var:3}", "val");
            yield return new TestCaseData("{var:30}", "value");
            yield return new TestCaseData("{list}", "red,green,blue");
            yield return new TestCaseData("{list*}", "red,green,blue");
            yield return new TestCaseData("{keys}", "semi,%3B,dot,.,comma,%2C");
            yield return new TestCaseData("{keys*}", "semi=%3B,dot=.,comma=%2C");
            yield return new TestCaseData("{+var}", "value");
            yield return new TestCaseData("{+hello}", "Hello%20World!");
            yield return new TestCaseData("{+half}", "50%25");
            yield return new TestCaseData("{base}index", "http%3A%2F%2Fexample.com%2Fhome%2Findex");
            yield return new TestCaseData("{+base}index", "http://example.com/home/index");
            yield return new TestCaseData("O{+empty}X", "OX");
            yield return new TestCaseData("O{+undef}X", "OX");
            yield return new TestCaseData("{+path}/here", "/foo/bar/here");
            yield return new TestCaseData("here?ref={+path}", "here?ref=/foo/bar");
            yield return new TestCaseData("up{+path}{var}/here", "up/foo/barvalue/here");
            yield return new TestCaseData("{+x,hello,y}", "1024,Hello%20World!,768");
            yield return new TestCaseData("{+path,x}/here", "/foo/bar,1024/here");
            yield return new TestCaseData("{+path:6}/here", "/foo/b/here");
            yield return new TestCaseData("{+list}", "red,green,blue");
            yield return new TestCaseData("{+list*}", "red,green,blue");
            yield return new TestCaseData("{+keys}", "semi,;,dot,.,comma,,");
            yield return new TestCaseData("{+keys*}", "semi=;,dot=.,comma=,");
            yield return new TestCaseData("{#var}", "#value");
            yield return new TestCaseData("{#hello}", "#Hello%20World!");
            yield return new TestCaseData("{#half}", "#50%25");
            yield return new TestCaseData("foo{#empty}", "foo#");
            yield return new TestCaseData("foo{#undef}", "foo");
            yield return new TestCaseData("{#x,hello,y}", "#1024,Hello%20World!,768");
            yield return new TestCaseData("{#path,x}/here", "#/foo/bar,1024/here");
            yield return new TestCaseData("{#path:6}/here", "#/foo/b/here");
            yield return new TestCaseData("{#list}", "#red,green,blue");
            yield return new TestCaseData("{#list*}", "#red,green,blue");
            yield return new TestCaseData("{#keys}", "#semi,;,dot,.,comma,,");
            yield return new TestCaseData("{#keys*}", "#semi=;,dot=.,comma=,");
            yield return new TestCaseData("{.who}", ".fred");
            yield return new TestCaseData("{.who,who}", ".fred.fred");
            yield return new TestCaseData("{.half,who}", ".50%25.fred");
            yield return new TestCaseData("www{.dom*}", "www.example.com");
            yield return new TestCaseData("X{.var}", "X.value");
            yield return new TestCaseData("X{.empty}", "X.");
            yield return new TestCaseData("X{.undef}", "X");
            yield return new TestCaseData("X{.var:3}", "X.val");
            yield return new TestCaseData("X{.list}", "X.red,green,blue");
            yield return new TestCaseData("X{.list*}", "X.red.green.blue");
            yield return new TestCaseData("X{.keys}", "X.semi,%3B,dot,.,comma,%2C");
            yield return new TestCaseData("X{.keys*}", "X.semi=%3B.dot=..comma=%2C");
            yield return new TestCaseData("X{.empty_keys}", "X");
            yield return new TestCaseData("X{.empty_keys*}", "X");
            yield return new TestCaseData("{;who}", ";who=fred");
            yield return new TestCaseData("{;half}", ";half=50%25");
            yield return new TestCaseData("{;empty}", ";empty");
            yield return new TestCaseData("{;v,empty,who}", ";v=6;empty;who=fred");
            yield return new TestCaseData("{;v,bar,who}", ";v=6;who=fred");
            yield return new TestCaseData("{;x,y}", ";x=1024;y=768");
            yield return new TestCaseData("{;x,y,empty}", ";x=1024;y=768;empty");
            yield return new TestCaseData("{;x,y,undef}", ";x=1024;y=768");
            yield return new TestCaseData("{;hello:5}", ";hello=Hello");
            yield return new TestCaseData("{;list}", ";list=red,green,blue");
            yield return new TestCaseData("{;list*}", ";list=red;list=green;list=blue");
            yield return new TestCaseData("{;keys}", ";keys=semi,%3B,dot,.,comma,%2C");
            yield return new TestCaseData("{;keys*}", ";semi=%3B;dot=.;comma=%2C");
            yield return new TestCaseData("{?who}", "?who=fred");
            yield return new TestCaseData("{?half}", "?half=50%25");
            yield return new TestCaseData("{?x,y}", "?x=1024&y=768");
            yield return new TestCaseData("{?x,y,empty}", "?x=1024&y=768&empty=");
            yield return new TestCaseData("{?x,y,undef}", "?x=1024&y=768");
            yield return new TestCaseData("{?var:3}", "?var=val");
            yield return new TestCaseData("{?list}", "?list=red,green,blue");
            yield return new TestCaseData("{?list*}", "?list=red&list=green&list=blue");
            yield return new TestCaseData("{?keys}", "?keys=semi,%3B,dot,.,comma,%2C");
            yield return new TestCaseData("{?keys*}", "?semi=%3B&dot=.&comma=%2C");
            yield return new TestCaseData("{&who}", "&who=fred");
            yield return new TestCaseData("{&half}", "&half=50%25");
            yield return new TestCaseData("?fixed=yes{&x}", "?fixed=yes&x=1024");
            yield return new TestCaseData("{&x,y,empty}", "&x=1024&y=768&empty=");
            yield return new TestCaseData("{&x,y,undef}", "&x=1024&y=768");
            yield return new TestCaseData("{&var:3}", "&var=val");
            yield return new TestCaseData("{&list}", "&list=red,green,blue");
            yield return new TestCaseData("{&list*}", "&list=red&list=green&list=blue");
            yield return new TestCaseData("{&keys}", "&keys=semi,%3B,dot,.,comma,%2C");
            yield return new TestCaseData("{&keys*}", "&semi=%3B&dot=.&comma=%2C");
        }

        [TestCaseSource(nameof(Cases))]
        public void Test(string template, string expected)
        {
            var sut = new Runtime();

            var res = sut.Expand(template, Params);

            Assert.AreEqual(expected, res);
        }

        [TestCaseSource(nameof(Cases))]
        public void New(string template, string expected)
        {
            var sut = new Compiled(template);

            var res = sut.Expand(Params);

            Assert.AreEqual(expected, res);
        }
    }
}
