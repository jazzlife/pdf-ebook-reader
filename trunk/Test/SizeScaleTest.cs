using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using PDFViewer.Reader.Utils;

namespace PDFViewer.Test
{
    [TestFixture]
    public class SizeScaleTest
    {
        [Test]
        public void ScaleToFitBounds_Equal()
        {
            Size real = new Size(120, 100);
            Size limit = new Size(120, 100);
            Size result = new Size(120, 100);
            Assert.AreEqual(result, real.ScaleToFitBounds(limit));
        }

        [Test]
        public void ScaleToFitBounds_Wide()
        {
            Size real = new Size(240, 100);
            Size limit = new Size(120, 100);
            Size result = new Size(120, 50);
            Assert.AreEqual(result, real.ScaleToFitBounds(limit));
        }

        [Test]
        public void ScaleToFitBounds_Tall()
        {
            Size real = new Size(120, 200);
            Size limit = new Size(120, 100);
            Size result = new Size(60, 100);
            Assert.AreEqual(result, real.ScaleToFitBounds(limit));
        }

    }
}
