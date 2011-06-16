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
    class DefaultPageContentProvider : IPageContentProvider
    {
        readonly static Logger Log = LogManager.GetCurrentClassLogger();

        public IPageLayoutAnalyzer LayoutAnalyzer { get; set; }

        // Cache
        readonly DW<PageContentCache> Cache;

        object MyLock = new object();

        public DefaultPageContentProvider(DW<PageContentCache> cache, 
            IPageLayoutAnalyzer layoutAnalyzer = null)
        {
            if (layoutAnalyzer == null) { layoutAnalyzer = new BlobPageLayoutAnalyzer(); }
            LayoutAnalyzer = layoutAnalyzer;

            Cache = cache;
        }

        public PageContent RenderPhysicalPage(int pageNum, Size screenSize, IPhysicalPageProvider physicalPageProvider) 
        {
            // Try to get from cache
            PageContent pageInfo;
            if (Cache != null)
            {
                pageInfo = Cache.o.Get(physicalPageProvider.FullPath, pageNum, screenSize.Width);
                if (pageInfo != null)
                {
                    return pageInfo;
                }
            }

            pageInfo = RenderPhysicalPageCore(pageNum, screenSize, physicalPageProvider);

            // Save to cache
            if (Cache != null)
            {
                Cache.o.Add(physicalPageProvider.FullPath, pageNum, screenSize.Width, pageInfo);
            }

            return pageInfo;
        }

        // Simple optimization -- try to render in appropriate size
        int lastPageWidth = 1000; // for first page

        PageContent RenderPhysicalPageCore(int pageNum, Size screenSize, IPhysicalPageProvider physicalPageProvider)
        {
            Log.Debug("Rendering: #{0} w={1}", pageNum, screenSize.Width);

            // Lock to protect physical provider 
            // Note: cache is already protected
            lock (MyLock)
            {
                // NOTE: rendering the page twice -- we need the layout in order to figure out
                // the best dimensions for the final render.
                
                PageLayoutInfo layout;
                Size layoutRenderSize = new Size(lastPageWidth, int.MaxValue); 
                DW<Bitmap> layoutPage = physicalPageProvider.RenderPage(pageNum, layoutRenderSize, RenderQuality.Optimal);
                layout = LayoutAnalyzer.DetectPageLayout(layoutPage);

                // Special case - empty page
                if (layout.Bounds.IsEmpty)
                {
                    layoutPage.Dispose();

                    // Dummy layout -- screenWidth x 100
                    layout.Bounds = new Rectangle(0, 0, screenSize.Width, 100);
                    DW<Bitmap> emptyPage = DW.Wrap(new Bitmap(layout.Bounds.Width, layout.Bounds.Height));
                    return new PageContent(pageNum, emptyPage, layout);
                }

                // Render actual page. Bounded by width, but not height.
                int pageWidth = (int)((float)screenSize.Width / layout.BoundsRelative.Width);

                DW<Bitmap> displayPage;
                if (lastPageWidth - 10 < pageWidth && pageWidth < lastPageWidth + 2)
                {
                    // keep the image
                    displayPage = layoutPage;
                }
                else
                {
                    layoutPage.Dispose();

                    // render a new image
                    Log.Debug("Slow: rendering second page for display. old:{0} - new:{1} = {2}", 
                        lastPageWidth, pageWidth, lastPageWidth - pageWidth);

                    Size displayPageMaxSize = new Size(pageWidth, int.MaxValue);
                    displayPage = physicalPageProvider.RenderPage(pageNum, displayPageMaxSize, RenderQuality.Optimal);
                    layout.ScaleBounds(displayPage.o.Size);
                }

                // Update width
                lastPageWidth = pageWidth;

                // QQ: would cropping the display bitmap to content area yield any benefits?
                // Not doing it for now, as it has a cost as well.
                return new PageContent(pageNum, displayPage, layout);
            }
        }

    }
}
