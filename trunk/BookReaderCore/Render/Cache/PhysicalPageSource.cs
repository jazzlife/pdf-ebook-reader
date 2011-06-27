using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;
using System.Drawing;
using BookReader.Properties;
using BookReader.Render.Layout;

namespace BookReader.Render.Cache
{
    class PhysicalPageSource : IPageSource
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IPageLayoutStrategy LayoutStrategy { get; set; }

        // Simple optimization -- try to render in last size
        int lastPageWidth = 1000; // for first page

        public PhysicalPageSource()
        {
            LayoutStrategy = RenderFactory.Default.GetLayoutStrategy();
        }

        public Page GetPage(int pageNum, Size screenSize, ScreenBook screenBook)
        {
            logger.Debug("Rendering: #{0} w={1}", pageNum, screenSize.Width);
            
            // Try detecting from book first
            PageLayout layout = LayoutStrategy.DetectLayoutFromBook(screenBook, pageNum);
                        
            // If above is not supported, detect from page
            DW<Bitmap> layoutPage = null;
            if (layout == null)
            {
                // NOTE: rendering the page twice -- we need the layout in order to figure out
                // the best dimensions for the final render.

                Size layoutRenderSize = new Size(lastPageWidth, int.MaxValue);
                layoutPage = screenBook.BookProvider.o.RenderPageImage(pageNum, layoutRenderSize, RenderQuality.Optimal);
                layout = LayoutStrategy.DetectLayoutFromImage(layoutPage);

                if (layout == null)
                {
                    throw new ApplicationException("Layout strategy: " + LayoutStrategy + " returns null both FromBook and FromPage. It must support at least one.");
                }
            }

            // Special case - empty page
            if (layout.Bounds.IsEmpty)
            {
                if (layoutPage != null) { layoutPage.DisposeItem(); }

                // Dummy layout -- screenWidth x 100
                layout.Bounds = new Rectangle(0, 0, screenSize.Width, 100);
                DW<Bitmap> emptyPage = DW.Wrap(new Bitmap(layout.Bounds.Width, layout.Bounds.Height));
                return new Page(pageNum, emptyPage, layout);
            }

            // Render actual page. Bounded by width, but not height.
            int pageWidth = ((float)screenSize.Width / layout.UnitBounds.Width).Round();

            DW<Bitmap> displayPage;
            if (layoutPage != null &&
                lastPageWidth - 10 < pageWidth && pageWidth < lastPageWidth + 2)
            {
                // Optimization -- use layout page image as the final one
                // keep the image
                displayPage = layoutPage;
            }
            else
            {
                if (layoutPage != null) { layoutPage.DisposeItem(); }

                // Render a new image
                Size displayPageMaxSize = new Size(pageWidth, int.MaxValue);
                displayPage = screenBook.BookProvider.o.RenderPageImage(pageNum, displayPageMaxSize, RenderQuality.Optimal);
                layout.GetScaledBounds(displayPage.o.Size);
            }

            // Update width
            lastPageWidth = pageWidth;

            if (Settings.Default.Debug_DrawPageLayout)
            {
                DW<Bitmap> tmp = layout.Debug_DrawLayout(displayPage);
                displayPage.DisposeItem();
                displayPage = tmp;
            }

            Page page = new Page(pageNum, displayPage, layout);
            return page;
        }

        public void Dispose()
        {
        }
    }
}
