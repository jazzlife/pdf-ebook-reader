using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PdfBookReader.Render.Cache;
using PdfBookReader.Utils;
using NLog;

namespace PdfBookReader.Render
{
    class CachedPageSource : IPageSource
    {
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public IPageLayoutStrategy LayoutAnalyzer { get; set; }

        // Cache
        readonly DW<PageCache> Cache;
        readonly PrefetchManager PrefetchManager;

        object MyLock = new object();

        public CachedPageSource(IPageLayoutStrategy layoutAnalyzer = null)
        {
            if (layoutAnalyzer == null) { layoutAnalyzer = new ConnectedBlobLayoutStrategy(); }
            LayoutAnalyzer = layoutAnalyzer;

            Cache = DW.Wrap(new PageCache());
            PrefetchManager = new PrefetchManager(Cache);
            PrefetchManager.Start();
        }

        public Page GetPage(int pageNum, Size screenSize, DW<IBookProvider> bookProvider) 
        {
            // Try to get from cache
            Page pageInfo;
            if (Cache != null)
            {
                pageInfo = Cache.o.Get(bookProvider.o.BookFilename, pageNum, screenSize.Width);
                if (pageInfo != null)
                {
                    return pageInfo;
                }
            }

            pageInfo = RenderPhysicalPage(pageNum, screenSize, bookProvider);

            // Save to cache
            if (Cache != null)
            {
                Cache.o.Add(bookProvider.o.BookFilename, pageNum, screenSize.Width, pageInfo);
            }

            return pageInfo;
        }

        // Simple optimization -- try to render in appropriate size
        int lastPageWidth = 1000; // for first page

        Page RenderPhysicalPage(int pageNum, Size screenSize, DW<IBookProvider> pageProvider)
        {
            logger.Debug("Rendering: #{0} w={1}", pageNum, screenSize.Width);

            // Lock to protect physical provider 
            // Note: cache is already protected
            lock (MyLock)
            {
                // NOTE: rendering the page twice -- we need the layout in order to figure out
                // the best dimensions for the final render.
                
                PageLayoutInfo layout;
                Size layoutRenderSize = new Size(lastPageWidth, int.MaxValue); 
                DW<Bitmap> layoutPage = pageProvider.o.RenderPageImage(pageNum, layoutRenderSize, RenderQuality.Optimal);
                layout = LayoutAnalyzer.DetectLayout(layoutPage);

                // Special case - empty page
                if (layout.Bounds.IsEmpty)
                {
                    layoutPage.DisposeItem();

                    // Dummy layout -- screenWidth x 100
                    layout.Bounds = new Rectangle(0, 0, screenSize.Width, 100);
                    DW<Bitmap> emptyPage = DW.Wrap(new Bitmap(layout.Bounds.Width, layout.Bounds.Height));
                    return new Page(pageNum, emptyPage, layout);
                }

                // Render actual page. Bounded by width, but not height.
                int pageWidth = (int)((float)screenSize.Width / layout.BoundsUnit.Width);

                DW<Bitmap> displayPage;
                if (lastPageWidth - 10 < pageWidth && pageWidth < lastPageWidth + 2)
                {
                    // keep the image
                    displayPage = layoutPage;
                }
                else
                {
                    layoutPage.DisposeItem();

                    // render a new image
                    logger.Debug("Slow: rendering second page for display. old:{0} - new:{1} = {2}", 
                        lastPageWidth, pageWidth, lastPageWidth - pageWidth);

                    Size displayPageMaxSize = new Size(pageWidth, int.MaxValue);
                    displayPage = pageProvider.o.RenderPageImage(pageNum, displayPageMaxSize, RenderQuality.Optimal);
                    layout.ScaleBounds(displayPage.o.Size);
                }

                // Update width
                lastPageWidth = pageWidth;

                // QQ: would cropping the display bitmap to content area yield any benefits?
                // Not doing it for now, as it has a cost as well.
                return new Page(pageNum, displayPage, layout);
            }
        }




        public void Dispose()
        {
            PrefetchManager.Stop();

            Cache.o.SaveCache();
            Cache.o.Dispose();
        }
    }
}
