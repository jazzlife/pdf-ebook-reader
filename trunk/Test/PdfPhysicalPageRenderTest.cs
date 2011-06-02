using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using PdfBookReader.Render;
using PdfBookReader.Test.TestUtils;
using System.Drawing;
using System.Drawing.Imaging;

namespace PdfBookReader.Test
{
    [TestFixture]
    class PdfPhysicalPageRenderTest
    {
        [Test]
        public void All_Fast()
        {
            var files = Directory.GetFiles(TestConst.PdfFilePath, "*.pdf");

            foreach (String file in files)
            {
                LoadAndRenderPdfFile(file, RenderQuality.Fast, 1, 10);
            }
        }

        void LoadAndRenderPdfFile(String file, RenderQuality quality, int start, int count)
        {
            PdfPhysicalPageProvider r;

            using (PerfTimer.SingleRun("Loading " + file))
            {
                r = new PdfPhysicalPageProvider(file);
            }

            Size size = new Size(1000, 1000);
            PerfTimer pageRenderTimer = new PerfTimer("Render pages {0}x{1} {2}", size.Width, size.Height, quality);

            int numPages = Math.Min(start + count - 1, r.PageCount);
            for (int pageNum = start; pageNum <= numPages; pageNum++)
            {
                Bitmap bmp;

                // Only time the render method
                using (pageRenderTimer.NewRun)
                {
                    bmp = r.RenderPage(pageNum, size, quality);
                }

                String imgFile = String.Format(@"C:\temp\{0}-{1:000}_{1}.png", Path.GetFileNameWithoutExtension(file), pageNum, quality);
                bmp.Save(imgFile, ImageFormat.Png);
                bmp.Dispose();
            }
            Console.WriteLine();
            Console.WriteLine(pageRenderTimer);
            Console.WriteLine();
        }

    }
}
