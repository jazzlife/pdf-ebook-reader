using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using BookReader.Utils;
using BookReaderTest.TestUtils;

namespace BookReaderTest.Utils
{
    [TestFixture]
    public class ExtensionMethodsTest
    {

        [Test]
        public void Linq_Split()
        {
            var exp = L(1,1,2,1,0,0);
            var act = exp.Split((a, b) => a != b).ToList();

            Assert.AreEqual(4, act.Count);
            act[0].AssertEqualsTo(1, 1);
            act[1].AssertEqualsTo(2);
            act[2].AssertEqualsTo(1);
            act[3].AssertEqualsTo(0,0);
        }

        [Test]
        public void TrueForAny()
        {
            Assert.IsTrue(L(1, 2, 3).TrueForAny(x => x == 2));
            Assert.IsFalse(L(1, 2, 3).TrueForAny(x => x > 5));
        }

        [Test]
        public void TrueForAll()
        {
            Assert.IsTrue(L(1, 2, 3).TrueForAll(x => x > 0));
            Assert.IsFalse(L(1, 2, 3).TrueForAll(x => x < 3));
        }

        T[] L<T>(params T[] items) { return items; }
    }
}
