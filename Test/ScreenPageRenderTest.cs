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
using PDFViewer.Reader.Render;

namespace PDFViewer.Test
{
    [TestFixture]
    public class ScreenPageRenderTest
    {
        String file_clean = "Clean Large Margins - McCargo.pdf";

        [Test, Explicit]
        public void All_RenderDown()
        {
            var files = Directory.GetFiles(TestConst.PdfFilePath, "*.pdf");

            IPageLayoutAnalyzer analyzer = new BlobPageLayoutAnalyzer();
            foreach (String file in files)
            {
                RenderDown(file, 15, analyzer, new Size(800, 600));
            }
        }

        [Test]
        public void One_RenderDown()
        {
            IPageLayoutAnalyzer analyzer = new BlobPageLayoutAnalyzer();
            RenderDown(Path.Combine(TestConst.PdfFilePath, file_clean), 100, analyzer, new Size(800, 600));
        }

        [Test]
        public void One_RenderDown_ExtraLongPage()
        {
            IPageLayoutAnalyzer analyzer = new BlobPageLayoutAnalyzer();
            RenderDown(Path.Combine(TestConst.PdfFilePath, file_clean), 100, analyzer, new Size(440, 1680));
        }

        [Test]
        public void One_RenderDown_ExtraLongPage_NoAnalysis()
        {
            IPageLayoutAnalyzer analyzer = new BlankPageLayoutAnalyzer();
            RenderDown(Path.Combine(TestConst.PdfFilePath, file_clean), 100, analyzer, new Size(440, 1680));
        }

        void RenderDown(String file, int maxPages, 
            IPageLayoutAnalyzer layoutAnalyzer, Size screenPageSize)
        {
            PdfPhysicalPageProvider pdfReader = new PdfPhysicalPageProvider(file);

            ScreenPageProvider screenPageProvider =
                new ScreenPageProvider(pdfReader, layoutAnalyzer, screenPageSize);

            PerfTimer timer = new PerfTimer("Screen Page Load {0}x{1} '{2}'", 
                screenPageSize.Width, screenPageSize.Height, Path.GetFileName(file));

            for (int pageNum = 0; pageNum < maxPages; pageNum++)
            {
                using (timer.NewRun)
                {
                    using (Bitmap screenPage = screenPageProvider.RenderNextPage())
                    {
                        // Last page
                        if (screenPage == null) { break; }

                        String imgFile = String.Format(@"C:\temp\{0}-{1:000}.png", Path.GetFileNameWithoutExtension(file), pageNum);
                        screenPage.Save(imgFile, ImageFormat.Png);
                    }
                }
            }

            Console.WriteLine(timer);
        }

    }

}
