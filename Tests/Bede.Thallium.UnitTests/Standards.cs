using System.Collections.Generic;

namespace Bede.Thallium.UnitTests
{
    using NUnit.Framework;

    [TestFixture]
    class Standards
    {
        static readonly Dictionary<string, object> Params = new Dictionary<string, object>
        {
            { "count",      new [] { "one", "two", "three" } },
            { "dom",        new [] { "example", "com"} },
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

        [TestCase("{count}"     ,"one,two,three")]
        [TestCase("{count*}"    ,"one,two,three")]
        [TestCase("{/count}"    ,"/one,two,three")]
        [TestCase("{/count*}"   ,"/one/two/three")]
        [TestCase("{;count}"    ,";count=one,two,three")]
        [TestCase("{;count*}"   ,";count=one;count=two;count=three")]
        [TestCase("{?count}"    ,"?count=one,two,three")]
        [TestCase("{?count*}"   ,"?count=one&count=two&count=three")]
        [TestCase("{&count*}"   ,"&count=one&count=two&count=three")]
        [TestCase("{var}"              ,"value")]
        [TestCase("{hello}"            ,"Hello%20World%21")]
        [TestCase("{half}"             ,"50%25")]
        [TestCase("O{empty}X"          ,"OX")]
        [TestCase("O{undef}X"          ,"OX")]
        [TestCase("{x,y}"              ,"1024,768")]
        [TestCase("{x,hello,y}"        ,"1024,Hello%20World%21,768")]
        [TestCase("?{x,empty}"         ,"?1024,")]
        [TestCase("?{x,undef}"         ,"?1024")]
        [TestCase("?{undef,y}"         ,"?768")]
        [TestCase("{var:3}"            ,"val")]
        [TestCase("{var:30}"           ,"value")]
        [TestCase("{list}"             ,"red,green,blue")]
        [TestCase("{list*}"            ,"red,green,blue")]
        [TestCase("{keys}"             ,"semi,%3B,dot,.,comma,%2C")]
        [TestCase("{keys*}"            ,"semi=%3B,dot=.,comma=%2C")]
        [TestCase("{+var}"                ,"value")]
        [TestCase("{+hello}"              ,"Hello%20World!")]
        [TestCase("{+half}"               ,"50%25")]
        [TestCase("{base}index"           ,"http%3A%2F%2Fexample.com%2Fhome%2Findex")]
        [TestCase("{+base}index"          ,"http://example.com/home/index")]
        [TestCase("O{+empty}X"            ,"OX")]
        [TestCase("O{+undef}X"            ,"OX")]
        [TestCase("{+path}/here"          ,"/foo/bar/here")]
        [TestCase("here?ref={+path}"      ,"here?ref=/foo/bar")]
        [TestCase("up{+path}{var}/here"   ,"up/foo/barvalue/here")]
        [TestCase("{+x,hello,y}"          ,"1024,Hello%20World!,768")]
        [TestCase("{+path,x}/here"        ,"/foo/bar,1024/here")]
        [TestCase("{+path:6}/here"        ,"/foo/b/here")]
        [TestCase("{+list}"               ,"red,green,blue")]
        [TestCase("{+list*}"              ,"red,green,blue")]
        [TestCase("{+keys}"               ,"semi,;,dot,.,comma,,")]
        [TestCase("{+keys*}"              ,"semi=;,dot=.,comma=,")]
        [TestCase("{#var}"             ,"#value")]
        [TestCase("{#hello}"           ,"#Hello%20World!")]
        [TestCase("{#half}"            ,"#50%25")]
        [TestCase("foo{#empty}"        ,"foo#")]
        [TestCase("foo{#undef}"        ,"foo")]
        [TestCase("{#x,hello,y}"       ,"#1024,Hello%20World!,768")]
        [TestCase("{#path,x}/here"     ,"#/foo/bar,1024/here")]
        [TestCase("{#path:6}/here"     ,"#/foo/b/here")]
        [TestCase("{#list}"            ,"#red,green,blue")]
        [TestCase("{#list*}"           ,"#red,green,blue")]
        [TestCase("{#keys}"            ,"#semi,;,dot,.,comma,,")]
        [TestCase("{#keys*}"           ,"#semi=;,dot=.,comma=,")]
        [TestCase("{.who}"             ,".fred")]
        [TestCase("{.who,who}"         ,".fred.fred")]
        [TestCase("{.half,who}"        ,".50%25.fred")]
        [TestCase("www{.dom*}"         ,"www.example.com")]
        [TestCase("X{.var}"            ,"X.value")]
        [TestCase("X{.empty}"          ,"X.")]
        [TestCase("X{.undef}"          ,"X")]
        [TestCase("X{.var:3}"          ,"X.val")]
        [TestCase("X{.list}"           ,"X.red,green,blue")]
        [TestCase("X{.list*}"          ,"X.red.green.blue")]
        [TestCase("X{.keys}"           ,"X.semi,%3B,dot,.,comma,%2C")]
        [TestCase("X{.keys*}"          ,"X.semi=%3B.dot=..comma=%2C")]
        [TestCase("X{.empty_keys}"     ,"X")]
        [TestCase("X{.empty_keys*}"    ,"X")]
        [TestCase("{;who}"             ,";who=fred")]
        [TestCase("{;half}"            ,";half=50%25")]
        [TestCase("{;empty}"           ,";empty")]
        [TestCase("{;v,empty,who}"     ,";v=6;empty;who=fred")]
        [TestCase("{;v,bar,who}"       ,";v=6;who=fred")]
        [TestCase("{;x,y}"             ,";x=1024;y=768")]
        [TestCase("{;x,y,empty}"       ,";x=1024;y=768;empty")]
        [TestCase("{;x,y,undef}"       ,";x=1024;y=768")]
        [TestCase("{;hello:5}"         ,";hello=Hello")]
        [TestCase("{;list}"            ,";list=red,green,blue")]
        [TestCase("{;list*}"           ,";list=red;list=green;list=blue")]
        [TestCase("{;keys}"            ,";keys=semi,%3B,dot,.,comma,%2C")]
        [TestCase("{;keys*}"           ,";semi=%3B;dot=.;comma=%2C")]
        [TestCase("{?who}"             ,"?who=fred")]
        [TestCase("{?half}"            ,"?half=50%25")]
        [TestCase("{?x,y}"             ,"?x=1024&y=768")]
        [TestCase("{?x,y,empty}"       ,"?x=1024&y=768&empty=")]
        [TestCase("{?x,y,undef}"       ,"?x=1024&y=768")]
        [TestCase("{?var:3}"           ,"?var=val")]
        [TestCase("{?list}"            ,"?list=red,green,blue")]
        [TestCase("{?list*}"           ,"?list=red&list=green&list=blue")]
        [TestCase("{?keys}"            ,"?keys=semi,%3B,dot,.,comma,%2C")]
        [TestCase("{?keys*}"           ,"?semi=%3B&dot=.&comma=%2C")]
        [TestCase("{&who}"             ,"&who=fred")]
        [TestCase("{&half}"            ,"&half=50%25")]
        [TestCase("?fixed=yes{&x}"     ,"?fixed=yes&x=1024")]
        [TestCase("{&x,y,empty}"       ,"&x=1024&y=768&empty=")]
        [TestCase("{&x,y,undef}"       ,"&x=1024&y=768")]
        [TestCase("{&var:3}"           ,"&var=val")]
        [TestCase("{&list}"            ,"&list=red,green,blue")]
        [TestCase("{&list*}"           ,"&list=red&list=green&list=blue")]
        [TestCase("{&keys}"            ,"&keys=semi,%3B,dot,.,comma,%2C")]
        [TestCase("{&keys*}"           ,"&semi=%3B&dot=.&comma=%2C")]
        public void TemplateHandling(string template, string expected)
        {
            var sut = new Rfc6750();

            var res = sut.Expand(template, Params);

            Assert.AreEqual(expected, res);
        }
    }
}
