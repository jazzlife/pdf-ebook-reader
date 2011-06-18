using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PdfBookReader.Render;
using PdfBookReader.Test.TestUtils;
using System.Drawing;
using System.Drawing.Imaging;
using PdfBookReader.Render.Cache;
using System.IO;
using PdfBookReader.Utils;
using System.Windows.Forms;
using Utils;

namespace PdfBookReader.Test
{
    [TestFixture]
    class PhysicalPageRenderPerf
    {
        Size PageSize = new Size(1000, int.MaxValue);

        IEnumerable<String> Files
        {
            get
            {
                return Directory.GetFiles(TestConst.PdfFilePath, "*.pdf").Take(4);
            }
        }
        IEnumerable<int> PageNums(int pageCount)
        {
            int numPages = Math.Min(Start + Count - 1, pageCount);
            for (int pageNum = Start; pageNum <= numPages; pageNum++)
            {
                yield return pageNum;
            }
        }
        const int Start = 10;
        const int Count = 15;

        [Test, Explicit]
        public void _ShowLogInExplorer()
        {
            LogUtils.ShowLogInExplorer();
        }

        [Test]
        public void t1a_PDFPhysicalPageRenderOptimal()
        {
            using (PTimer sumTimer = new PTimer("PDF Physical Render - Optimal"))
            {
                foreach (String file in Files)
                {
                    RenderPDFPages(sumTimer, file, RenderQuality.Optimal);
                }
            }
        }

        [Test]
        public void t1b_PDFPhysicalPageRenderHighQuality()
        {
            using (PTimer sumTimer = new PTimer("PDF Physical Render - HighQuality"))
            {
                foreach (String file in Files)
                {
                    RenderPDFPages(sumTimer, file, RenderQuality.HighQuality);
                }
            }
        }

        void RenderPDFPages(PTimer sumTimer, String file, RenderQuality quality)
        {
            PdfBookPageProvider r = new PdfBookPageProvider(file);

            using (PTimer localTimer = new PTimer(">>{0}: {1}".F(sumTimer.Name, Path.GetFileNameWithoutExtension(file))))
            {
                foreach (int pageNum in PageNums(r.PageCount))
                {
                    DW<Bitmap> bmp;

                    // Only time the render method
                    using (IDisposable d1 = localTimer.NewRun, d2 = sumTimer.NewRun)
                    {
                        bmp = r.RenderPage(pageNum, PageSize, quality);
                    }

                    // Count save when caching, but not normally
                    // String tempFile = Path.GetTempFileName();
                    // bmp.Save(tempFile, ImageFormat.Png);

                    bmp.Dispose();
                }
            }
        }

        [Test]
        public void t2a_BaselinePageRead()
        {
            // Remove cache
            Directory.Delete(CacheUtils.CacheFolderPath, true);

            using (PTimer sumTimer = new PTimer("Baseline read/analyze"))
            {
                foreach (String file in Files)
                {
                    RenderAndAnalyzePages(sumTimer, file);
                }
            }
        }

        [Test]
        public void t2b_BaselinePageReadNoAnalysis()
        {
            // Remove cache
            Directory.Delete(CacheUtils.CacheFolderPath, true);

            using (PTimer sumTimer = new PTimer("Baseline read, no analysis"))
            {
                foreach (String file in Files)
                {
                    RenderAndAnalyzePages(sumTimer, file, analyzer: new BlankPageLayoutAnalyzer());
                }
            }
        }

        [Test]
        public void t3_CachedPageRead()
        {
            // Create cache if it doesn't exist
            if (!Directory.Exists(CacheUtils.CacheFolderPath)) { t2b_BaselinePageReadNoAnalysis(); }

            // Disk cache
            using(PageContentCache cache = new PageContentCache())
            {
                using (PTimer sumTimer = new PTimer("Cached retrieval"))
                {
                    foreach (String file in Files)
                    {
                        RenderAndAnalyzePages(sumTimer, file, cache: cache);
                    }
                }

                // Memory cache
                using (PTimer sumTimer = new PTimer("Memory cache"))
                {
                    foreach (String file in Files)
                    {
                        RenderAndAnalyzePages(sumTimer, file, cache: cache);
                    }
                }
            }
        }

        void RenderAndAnalyzePages(PTimer sumTimer, String file, RenderQuality quality = RenderQuality.Optimal,
            IPageLayoutAnalyzer analyzer = null, PageContentCache cache = null)
        {
            bool disposeCache = false;
            if (cache == null)
            {
                cache = new PageContentCache();
                disposeCache = true;
            }

            DefaultPageContentProvider pcp = new DefaultPageContentProvider(DW.Wrap(cache), analyzer);
            PdfBookPageProvider pageProvider = new PdfBookPageProvider(file);

            using (PTimer localTimer = new PTimer(">>{0}: {1}".F(sumTimer.Name, Path.GetFileNameWithoutExtension(file))))
            {
                foreach(int pageNum in PageNums(pageProvider.PageCount))
                {
                    PageContent pc;

                    // Only time the render method
                    using (IDisposable d1 = localTimer.NewRun, d2 = sumTimer.NewRun)
                    {
                        pc = pcp.GetPage(pageNum, PageSize, pageProvider);
                    }
                }
            }

            cache.SaveCache();
            if (disposeCache) { cache.Dispose(); }
        }
    }
}
