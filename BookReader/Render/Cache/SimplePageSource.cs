using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Utils;
using System.Drawing;

namespace PdfBookReader.Render.Cache
{
    class SimplePageSource : IPageSource
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IPageLayoutStrategy LayoutStrategy { get; set; }

        // Simple optimization -- try to render in last size
        int lastPageWidth = 1000; // for first page

        public Page GetPage(int pageNum, Size screenSize, ScreenBook screenBook)
        {
            logger.Debug("Rendering: #{0} w={1}", pageNum, screenSize.Width);

            // NOTE: rendering the page twice -- we need the layout in order to figure out
            // the best dimensions for the final render.
                
            PageLayoutInfo layout;
            Size layoutRenderSize = new Size(lastPageWidth, int.MaxValue); 
            DW<Bitmap> layoutPage = screenBook.BookProvider.o.RenderPageImage(pageNum, layoutRenderSize, RenderQuality.Optimal);
            layout = LayoutStrategy.DetectLayout(layoutPage);

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
                displayPage = screenBook.BookProvider.o.RenderPageImage(pageNum, displayPageMaxSize, RenderQuality.Optimal);
                layout.ScaleBounds(displayPage.o.Size);
            }

            // Update width
            lastPageWidth = pageWidth;

            // QQ: would cropping the display bitmap to content area yield any benefits?
            // Not doing it for now, as it has a cost as well.
            return new Page(pageNum, displayPage, layout);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
