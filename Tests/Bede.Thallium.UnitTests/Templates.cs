using System.Linq;
using NUnit.Framework;

namespace Bede.Thallium.UnitTests
{
    using Data;

    [TestFixture]
    public class Templates
    {
        [Test]
        public void ExplodeKeys()
        {
            var sut = new Explode<MyClass>(new MyClass());

            Assert.AreEqual(2, sut.Count());
        }

        class MyClass
        {
            public int    Foo { get; set; } = 1;
            public string Bar { get; set; } = "bar";
        }
    }
}
