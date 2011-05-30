using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using PDFViewer.Reader.Utils;
using PDFViewer.Reader;
using System.Drawing.Imaging;

namespace Test
{
    [TestFixture]
    public class PdfEBookRendererTest
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

        [Test]
        public void GeneratePages()
        {
            String file = @"..\..\..\SampleTestPDFs\Different Page Sizes - Dawkins God.pdf";

            PdfEBookRenderer r = new PdfEBookRenderer();
            r.LoadPdf(file);

            for (int pageNum = 1; pageNum < 15; pageNum++)
            {
                using (Bitmap bmp = r.RenderPdfPageToBitmap(new Size(600, 600), pageNum))
                {
                        String imgFile = @"C:\temp\page" + pageNum + ".png";
                        bmp.Save(imgFile, ImageFormat.Png);
                }
            }

        }
    }
}
