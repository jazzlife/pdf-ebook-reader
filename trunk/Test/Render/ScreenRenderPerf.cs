using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PdfBookReader.Render;
using PdfBookReaderTest.TestUtils;
using System.IO;
using System.Drawing;
using PdfBookReader.Model;

namespace PdfBookReaderTest.Render
{
    [TestFixture]
    class ScreenRenderPerf
    {
        Size ScreenSize = new Size(800, 600);

        // Test on 3 different PDFs
        // RenderNext, RenderPrev, Render(page)

        [Test]
        public void RenderNormalFileNoCache()
        {
            RenderOneFile(new PTimer("Render"), 
                Path.Combine(TestConst.PdfFilePath, @"Clean Margins Pictures - Balasevic.pdf"));
        }

        [Test]
        public void RenderBigSlowFileNoCache()
        {
            RenderOneFile(new PTimer("Render"),
                Path.Combine(TestConst.PdfFilePath, @"Bad Scan Tilted Facing Pages Big - Solzhenitsyn.pdf"));
        }

        void RenderOneFile(PTimer timer, String file)
        {
            DefaultPageContentProvider dpcp = new DefaultPageContentProvider(null);

            ScreenProvider provider = new ScreenProvider(
                new PdfBookPageProvider(file),
                dpcp, ScreenSize);

            const int RenderNextRepeats = 10;
            const int RenderMiddleRepeats = 5;
            const int RenderResizeRepeats = 3;
            const int RenderPreviousRepeats = 5;

            PTimer fileTimer = new PTimer("{0} file: {1}", timer.Name, Path.GetFileName(file));

            // Render first, forward
            using(IDisposable a = timer.NewRun, b = fileTimer.NewRun)
            {
                provider.RenderFirstPage();
            }            
            for (int i = 0; i < RenderNextRepeats; i++)
            {
                using (IDisposable a = timer.NewRun, b = fileTimer.NewRun)
                {
                    provider.RenderNextPage();
                }
            }

            // Render middle and re-render
            int middle = provider.PageProvider.PageCount / 2;            
            for (int i = 0; i < RenderMiddleRepeats; i++)
            {
                using (IDisposable a = timer.NewRun, b = fileTimer.NewRun)
                {
                    PositionInBook pos = PositionInBook.FromPositionUnit(middle, provider.PageProvider.PageCount);
                    provider.RenderPage(pos);
                }
            }

            // Render at different sizes
            for (int i = 0; i < RenderResizeRepeats; i++)
            {
                // Render at different size
                Size newSize = new Size((int)(provider.ScreenSize.Width * 1.2), (int)(provider.ScreenSize.Height * 1.2));
                provider.RenderCurrentPage(newSize);
            }
            using (IDisposable a = timer.NewRun, b = fileTimer.NewRun)
            {
                provider.RenderCurrentPage(ScreenSize); // back to normal
            }

            // Render last, backward
            using (IDisposable a = timer.NewRun, b = fileTimer.NewRun)
            {
                provider.RenderLastPage();
            }
            for (int i = 0; i < RenderPreviousRepeats; i++)
            {
                using (IDisposable a = timer.NewRun, b = fileTimer.NewRun)
                {
                    provider.RenderPreviousPage();
                }
            }

            Console.WriteLine();
            Console.Write(fileTimer);
        }
    }
}
