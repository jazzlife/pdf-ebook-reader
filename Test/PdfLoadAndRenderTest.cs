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

namespace PDFViewer.Test
{
    [TestFixture]
    public class PdfLoadAndRenderTest
    {
        const String PdfFilePath = @"..\..\PDFs";
        //String file_normal = "Pictures - Predictably Irrational.pdf";
        //String file_badScan = "Bad Scan Tilted Facing Pages Big - Solzhenitsyn.pdf";
        String file_clean = "Clean Large Margins - McCargo.pdf";

        #region PDF page load and render

        [Test, Explicit]
        public void All_LoadAndRenderPdfPages_Fast()
        {
            var files = Directory.GetFiles(PdfFilePath, "*.pdf").Select(x => Path.GetFileName(x));

            foreach (String file in files)
            {
                LoadAndRenderPdfPages(file, RenderQuality.Fast, 1, 10);
            }
        }

        void LoadAndRenderPdfPages(String file, RenderQuality quality, int start, int count)
        {
            PdfEBookRenderer r = new PdfEBookRenderer();

            using (PerfTimer.SingleRun("Loading " + file))
            {
                file = Path.Combine(PdfFilePath, file);
                r.LoadPdf(file);
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

        #endregion

        #region Screen Page Render

        [Test, Explicit]
        public void All_RenderScreenPages()
        {
            var files = Directory.GetFiles(PdfFilePath, "*.pdf").Select(x => Path.GetFileName(x));

            foreach (String file in files)
            {
                RenderScreenPages(file, 1, 15, new Size(800, 600));
            }
        }

        [Test]
        public void One_RenderScreenPageExtraLongMultiplePDFPages()
        {
            RenderScreenPages(file_clean, 1, 10, new Size(600, 1680));
        }

        void RenderScreenPages(String file, int start, int count, Size screenPageSize)
        {
            file = Path.Combine(PdfFilePath, file);

            PdfEBookRenderer r = new PdfEBookRenderer();
            r.LoadPdf(file);

            PerfTimer timer = new PerfTimer("Screen Page Load {0}x{1} '{2}'", 
                screenPageSize.Width, screenPageSize.Height, Path.GetFileName(file));

            int numPages = Math.Min(start+count-1, r.PageCount);
            for (int pageNum = start; pageNum <= numPages; pageNum++)
            {
                using (timer.NewRun)
                {
                    using (Bitmap screenPage = r.RenderScreenPageToBitmap(pageNum, 0, screenPageSize))
                    {
                        String imgFile = String.Format(@"C:\temp\{0}-{1:000}.png", Path.GetFileNameWithoutExtension(file), pageNum);
                        screenPage.Save(imgFile, ImageFormat.Png);
                    }
                }
            }

            Console.WriteLine(timer);
        }

        #endregion

    }

}
