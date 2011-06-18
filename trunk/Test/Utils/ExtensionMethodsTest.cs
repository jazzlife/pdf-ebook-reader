using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PdfBookReader.Utils;

namespace PdfBookReaderTest.Utils
{
    [TestFixture]
    public class ExtensionMethodsTest
    {
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
