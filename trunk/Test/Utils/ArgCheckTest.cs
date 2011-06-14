using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using PdfBookReader.Utils;

namespace PdfBookReader.Test.Utils
{
    [TestFixture]
    public class ArgCheckTest
    {

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage="fail")]
        public void TestArgIs()
        {
            Point pt = new Point();

            ArgCheck.Is(pt.IsEmpty, "pass");
            ArgCheck.Is(pt.X == 10, "fail");
        }


        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "fail")]
        public void TestArgIsNot()
        {
            Point pt = new Point();

            ArgCheck.IsNot(pt.X == 10, "pass");
            ArgCheck.IsNot(pt.IsEmpty, "fail");
        }

    }
}
