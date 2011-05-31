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

namespace PDFViewer.Test
{
    [TestFixture]
    public class PdfLoadAndRenderTest
    {
        const String PdfFilePath = @"..\..\PDFs";
        String file1 = "Pictures - Predictably Irrational.pdf";
        String file2 = "Bad Scan Tilted Facing Pages Big - Solzhenitsyn.pdf";
        String file3 = "Clean Tex - Hitchhiker's.pdf";

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

            using (PerfTimer.SingleRun("Loading " + file))
            {
                file = Path.Combine(PdfFilePath, file);
                r.LoadPdf(file);
            }

            Size size = new Size(1000, 1000);
            PerfTimer pageRenderTimer = new PerfTimer("Render pages {0}x{1} {2}", size.Width, size.Height, quality);
            
            int numPages = Math.Min(20, r.PageCount);
            for (int pageNum = 5; pageNum <= numPages; pageNum++)
            {
                Bitmap bmp;
                
                // Only time the render method
                using (pageRenderTimer.NewRun)
                {
                    bmp = r.RenderPdfPageToBitmap(pageNum, size, quality);
                }

                String imgFile = String.Format(@"C:\temp\{0}-{1:000}_{1}.png", Path.GetFileNameWithoutExtension(file), pageNum, quality);
                bmp.Save(imgFile, ImageFormat.Png);
                bmp.Dispose();
            }
            Console.WriteLine();
            Console.WriteLine(pageRenderTimer);
            Console.WriteLine();
        }

        [Test, Explicit]
        public void GeneratePagesForAll_Fast()
        {
            var files = Directory.GetFiles(PdfFilePath, "*.pdf").Select(x => Path.GetFileName(x));

            foreach (String file in files)
            {
                GeneratePages(file, RenderQuality.Fast);
            }

        }

        [Test]
        public void aRenderScreenPagesForAll_Fast()
        {
            var files = Directory.GetFiles(PdfFilePath, "*.pdf").Select(x => Path.GetFileName(x));

            foreach (String file in files)
            {
                RenderScreenPages(file);
            }

        }

        void RenderScreenPages(String file)
        {
            file = Path.Combine(PdfFilePath, file);

            PdfEBookRenderer r = new PdfEBookRenderer();
            r.LoadPdf(file);

            Size screenPageSize = new Size(800, 600);

            PerfTimer timer = new PerfTimer("Screen Page Load {0}x{1} '{2}'", 
                screenPageSize.Width, screenPageSize.Height, Path.GetFileName(file));

            int numPages = Math.Min(10, r.PageCount);
            for (int pageNum = 1; pageNum <= numPages; pageNum++)
            {
                using (timer.NewRun)
                {
                    using (Bitmap screenPage = r.RenderScreenPageToBitmap(pageNum, 0, screenPageSize))
                    {
                        String imgFile = String.Format(@"C:\temp\{0}-{1:000}a.png", Path.GetFileNameWithoutExtension(file), pageNum);
                        screenPage.Save(imgFile, ImageFormat.Png);
                    }

                    using (Bitmap screenPage = r.RenderScreenPageToBitmap(pageNum, screenPageSize.Height, screenPageSize))
                    {
                        String imgFile = String.Format(@"C:\temp\{0}-{1:000}b.png", Path.GetFileNameWithoutExtension(file), pageNum);
                        screenPage.Save(imgFile, ImageFormat.Png);
                    }
                }
            }

            Console.WriteLine(timer);
        }

    }

}
