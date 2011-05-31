using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using PDFViewer.Reader.Utils;
using PDFViewer.Reader;
using System.Drawing.Imaging;
using PDFViewer.Test.TestUtils;
using System.IO;
using PDFViewer.Reader.GraphicsUtils;

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


        String file1 = @"..\..\..\SampleTestPDFs\Pictures - Predictably Irrational.pdf";
        String file2 = @"..\..\..\SampleTestPDFs\Bad Scan Tilted Facing Pages Big - Solzhenitsyn.pdf";
        String file3 = @"..\..\..\SampleTestPDFs\Clean Tex - Hitchhiker's.pdf";

        [Test]
        public void GeneratePages1_Fast() { GeneratePages(file1, RenderQuality.Fast); }
        [Test]
        public void GeneratePages1_Quality() { GeneratePages(file1, RenderQuality.HighQualityMuPdf); }

        [Test]
        public void GeneratePages2_Fast() { GeneratePages(file2, RenderQuality.Fast); }
        [Test]
        public void GeneratePages2_Quality() { GeneratePages(file2, RenderQuality.HighQualityMuPdf); }

        [Test]
        public void GeneratePages3_Fast() { GeneratePages(file3, RenderQuality.Fast); }
        [Test]
        public void GeneratePages3_Quality() { GeneratePages(file3, RenderQuality.HighQualityMuPdf); }

        void GeneratePages(String file, RenderQuality quality)
        {
            PdfEBookRenderer r = new PdfEBookRenderer();

            using (PerfTimer t = new PerfTimer("Loading " + Path.GetFileName(file)))
            {
                r.LoadPdf(file);
            }

            int numPages = Math.Min(30, r.PageCount);

            using(PerfTimer t = new PerfTimer("Rendering pages " + quality, numPages))
            {
                for (int pageNum = 1; pageNum <= numPages; pageNum++)
                {
                    Console.Write(".");
                    using (Bitmap bmp = r.RenderPdfPageToBitmap(pageNum, new Size(200, 200), quality, 
                        WhitespaceEdgeDetector.RenderEdgeDetectFrame))
                    {
                            String imgFile = String.Format(@"C:\temp\page{0:000}_{1}.png", pageNum, quality);
                            bmp.Save(imgFile, ImageFormat.Png);
                    }
                }
                Console.WriteLine();
            }

        }

    }

}
