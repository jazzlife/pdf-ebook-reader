﻿using System;
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

        [Test]
        public void All_RenderPagesHQ()
        {
            var files = Directory.GetFiles(TestConst.PdfFilePath, "*.pdf");

            IPageLayoutAnalyzer analyzer = new BlobPageLayoutAnalyzer();
            foreach (String file in files)
            {
                RenderScreenPages(file, 1, 15, analyzer, new Size(800, 600));
            }
        }

        [Test]
        public void One_RenderExtraLongPage()
        {
            IPageLayoutAnalyzer analyzer = new BlobPageLayoutAnalyzer();
            RenderScreenPages(Path.Combine(TestConst.PdfFilePath, file_clean), 
                1, 10, analyzer, new Size(400, 1680));
        }

        [Test]
        public void One_RenderExtraLongPage_NoAnalysis()
        {
            IPageLayoutAnalyzer analyzer = new BlankPageLayoutAnalyzer();

            RenderScreenPages(Path.Combine(TestConst.PdfFilePath, file_clean),
                1, 10, analyzer, new Size(400, 1680));
        }

        void RenderScreenPages(String file, int start, int count, 
            IPageLayoutAnalyzer layoutAnalyzer, Size screenPageSize)
        {
            PdfPhysicalPageProvider pdfReader = new PdfPhysicalPageProvider(file);

            ScreenPageProvider screenPageProvider =
                new ScreenPageProvider(pdfReader, layoutAnalyzer, screenPageSize);

            PerfTimer timer = new PerfTimer("Screen Page Load {0}x{1} '{2}'", 
                screenPageSize.Width, screenPageSize.Height, Path.GetFileName(file));

            int numPages = Math.Min(start+count-1, pdfReader.PageCount);
            for (int pageNum = start; pageNum <= numPages; pageNum++)
            {
                using (timer.NewRun)
                {
                    using (Bitmap screenPage = screenPageProvider.RenderScreenPageToBitmap(pageNum, 0))
                    {
                        String imgFile = String.Format(@"C:\temp\{0}-{1:000}.png", Path.GetFileNameWithoutExtension(file), pageNum);
                        screenPage.Save(imgFile, ImageFormat.Png);
                    }
                }
            }

            Console.WriteLine(timer);
        }

    }

}
