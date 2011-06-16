using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using PdfBookReader.Utils;
using PdfBookReader;
using System.Drawing.Imaging;
using PdfBookReader.Test.TestUtils;
using System.IO;
using PdfBookReader.Render;

namespace PdfBookReader.Test
{
    [TestFixture]
    public class ScreenRenderTest
    {
        String file_clean = "Clean Large Margins - McCargo.pdf";

        [Test, Explicit]
        public void All_RenderDown()
        {
            var files = Directory.GetFiles(TestConst.PdfFilePath, "*.pdf");

            foreach (String file in files)
            {
                Render(file, 15, new Size(800, 600));
            }
        }

        // Render down
        [Test]
        public void One_RenderDown()
        {
            Render(Path.Combine(TestConst.PdfFilePath, file_clean), 100, new Size(800, 600));
        }


        [Test]
        public void One_RenderDown_ExtraLongPage()
        {
            IPageLayoutAnalyzer analyzer = new DefaultPageLayoutAnalyzer();
            Render(Path.Combine(TestConst.PdfFilePath, file_clean), 100, new Size(440, 1680));
        }


        // Render up
        [Test]
        public void One_RenderUp()
        {
            Render(Path.Combine(TestConst.PdfFilePath, file_clean), 100, new Size(800, 600), true);
        }

        void Render(String file, int maxPages, Size screenPageSize, bool renderUp = false)
        {
            IPhysicalPageProvider pdfReader = new PdfPhysicalPageProvider(file);
            IPageContentProvider contentProvider = new DefaultPageContentProvider(null);

            ScreenPageProvider screenPageProvider =
                new ScreenPageProvider(pdfReader, contentProvider, screenPageSize);

            PTimer timer = new PTimer("Screen Page Load {0}x{1} '{2}'", 
                screenPageSize.Width, screenPageSize.Height, Path.GetFileName(file));

            for (int pageNum = 0; pageNum < maxPages; pageNum++)
            {
                using (timer.NewRun)
                {
                    using (Bitmap screenPage = RenderFollowing(screenPageProvider, renderUp))
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

        Bitmap RenderFollowing(ScreenPageProvider provider, bool renderUp)
        {
            if (renderUp) { return provider.RenderPreviousPage(); }
            else { return provider.RenderNextPage(); }
        }

    }

}
