using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PdfBookReader.Utils;

namespace PdfBookReaderTest.Utils
{
    [TestFixture]
    public class DisposableWrapperTest
    {
        [Test]
        public void ActiveCountCorrect()
        {
            var d1 = DW.Wrap(new Dummy());
            var d2 = DW.Wrap(new Fummy(2));
            var d3 = DW.Wrap(new Dummy());

            Assert.AreEqual(3, DW.Created);
            Assert.AreEqual(3, DW.Active);
            Assert.AreEqual(0, DW.Disposed);

            Console.WriteLine(DW.GetDebugInfo());
            DW.GetAllActiveWrappers().ForEach( x => Console.Write(x + "\r\n"));
            Console.WriteLine();

            d1.Dispose();
            d2.Dispose();

            Assert.AreEqual(3, DW.Created);
            Assert.AreEqual(2, DW.Disposed);
            Assert.AreEqual(1, DW.Active);

            Console.WriteLine(DW.GetDebugInfo());
            DW.GetAllActiveWrappers().ForEach(x => Console.Write(x + "\r\n"));
            Console.WriteLine();

            d1.Dispose();
            Console.WriteLine(DW.GetDebugInfo());
            DW.GetAllActiveWrappers().ForEach(x => Console.Write(x + "\r\n"));
            Console.WriteLine();
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DisposedObjectAccessThrows()
        {
            var d1 = DW.Wrap(new Dummy());
            d1.Dispose();

            d1.o.Method(); // must throw
        }

        [Test]
        public void EqualInnerObjects()
        {
            var d1 = DW.Wrap(new Fummy(4));
            var d2 = DW.Wrap(new Fummy(4));
            var nd = DW.Wrap(new Fummy(10));

            Assert.AreNotEqual(d1, nd);
            Assert.AreNotEqual(d2, nd);

            Assert.AreEqual(d1, d2);
            Assert.AreEqual(d1.GetHashCode(), d2.GetHashCode());
        }

        class Dummy : IDisposable
        {
            public void Dispose() { }
            public void Method() { }
        }

        class Fummy : Dummy 
        {
            readonly int Num;
            public Fummy(int num)
            {
                Num = num;
            }

            public override bool Equals(object obj)
            {
                Fummy that = obj as Fummy;
                if (that == null) { return false; }
                return this.Num == that.Num;
            }

            public override int GetHashCode()
            {
                return Num.GetHashCode();
            }

            public override string ToString()
            {
                return "Fummy(" + Num + ")";
            }

        }
    }
}
