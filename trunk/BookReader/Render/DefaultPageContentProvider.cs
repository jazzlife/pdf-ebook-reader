using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PdfBookReader.Render.Cache;

namespace PdfBookReader.Render
{
    class DefaultPageContentProvider : IPageContentProvider
    {
        // Smaller probably renders faster, but rows are hard to distinguish.
        // experiment later. Maybe move into the layout analyzer.
        readonly Size LayoutRenderSize = new Size(1000, 1000);

        public IPageLayoutAnalyzer LayoutAnalyzer { get; set; }

        // Cache
        readonly PageContentCache Cache;

        object MyLock = new object();

        public DefaultPageContentProvider(PageContentCache cache, 
            IPageLayoutAnalyzer layoutAnalyzer = null)
        {
            if (layoutAnalyzer == null) { layoutAnalyzer = new DefaultPageLayoutAnalyzer(); }
            LayoutAnalyzer = layoutAnalyzer;

            Cache = cache;
        }

        public PageContent RenderPhysicalPage(int pageNum, Size screenSize, IPhysicalPageProvider physicalPageProvider) 
        {
            // Try to get from cache
            PageContent pageInfo;
            if (Cache != null)
            {
                pageInfo = Cache.Get(physicalPageProvider.FullPath, pageNum, screenSize.Width);
                if (pageInfo != null)
                {
                    Trace.WriteLine("GetPhysicalPage: returning cached page");
                    return pageInfo;
                }
            }

            pageInfo = RenderPhysicalPageCore(pageNum, screenSize, physicalPageProvider);

            // Save to cache
            if (Cache != null)
            {
                Cache.Add(physicalPageProvider.FullPath, pageNum, screenSize.Width, pageInfo);
            }

            return pageInfo;
        }

        PageContent RenderPhysicalPageCore(int pageNum, Size screenSize, IPhysicalPageProvider physicalPageProvider)
        {
            // Lock to protect physical provider 
            // Note: cache is already protected
            lock (MyLock)
            {

                // NOTE: rendering the page twice -- we need the layout in order to figure out
                // the best dimensions for the final render.

                PageLayoutInfo layout;
                using (Bitmap bmpLayoutPage = physicalPageProvider.RenderPage(pageNum, LayoutRenderSize, RenderQuality.Fast))
                {
                    layout = LayoutAnalyzer.DetectPageLayout(bmpLayoutPage);
                }

                // Empty page
                if (layout.Bounds.IsEmpty)
                {
                    layout.Bounds = new Rectangle(0, 0, screenSize.Width, 100);
                    return new PageContent(pageNum, new Bitmap(layout.Bounds.Width, layout.Bounds.Height), layout);
                }

                // Render actual page. Bounded by width, but not height.
                int maxWidth = (int)((float)screenSize.Width / layout.BoundsRelative.Width);
                Size displayPageMaxSize = new Size(maxWidth, int.MaxValue);

                // Get the actual page
                Bitmap image = physicalPageProvider.RenderPage(pageNum, displayPageMaxSize, RenderQuality.Optimal);

                layout.ScaleBounds(image.Size);
                return new PageContent(pageNum, image, layout);
            }
        }

    }
}
