using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Templating;

    [Explicit]
    [TestFixture(1e3, 50)]
    [TestFixture(1e6, 10000)]
    class Performance
    {
        readonly int      _iterations;
        readonly TimeSpan _limit;

        public Performance(double iterations, int ms)
        {
            _iterations = (int) iterations;
            _limit      = TimeSpan.FromMilliseconds(ms);
        }

        public static IEnumerable Cases()
        {
            yield return new TestCaseData("http://www{.dom*}:{v}/api/{+dub}{+path}{/count}{?x,y,empty,undef}{&list,keys*}{#hello}");
        }

        [TestCaseSource(nameof(Cases))]
        public void Test(string template)
        {
            var st = Stopwatch.StartNew();

            for (var i = 0; i < _iterations; ++i)
            {
                new Runtime().Expand(template, Standards.Params);
            }

            st.Stop();
            Console.WriteLine(st.Elapsed);
            Assert.Less(st.Elapsed, _limit);
        }

        [TestCaseSource(nameof(Cases))]
        public void New(string template)
        {
            var expander = new Compiled(template).Compile();

            var st = Stopwatch.StartNew();

            for (var i = 0; i < _iterations; ++i)
            {
                expander.Expand(Standards.Params);
            }

            st.Stop();
            Console.WriteLine(st.Elapsed);
            Assert.Less(st.Elapsed, _limit);
        }
    }
}
