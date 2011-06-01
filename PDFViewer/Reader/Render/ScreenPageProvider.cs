using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PDFViewer.Reader.Utils;

namespace PDFViewer.Reader.Render
{
    /// <summary>
    /// Renders screen pages based on physical pages.
    /// Keeps track of current page with ability to request next/previous.
    /// </summary>
    public class ScreenPageProvider 
    {
        Size _screenPageSize;
        IPhysicalPageProvider _physicalPageProvider;
        IPageLayoutAnalyzer _pageLayoutAnalyzer;

        public ScreenPageProvider(
            IPhysicalPageProvider physicalPageProvider,
            IPageLayoutAnalyzer layoutAnalyzer,
            Size screenPageSize)
        {
            ScreenPageSize = screenPageSize;
            PhysicalPageProvider = physicalPageProvider;
            LayoutAnalyzer = layoutAnalyzer;
        }

        #region Config Properties

        public Size ScreenPageSize 
        {
            get { return _screenPageSize; }
            set
            {
                ArgCheck.NotEmpty(value);

                _screenPageSize = value;
            }
        }

        public IPhysicalPageProvider PhysicalPageProvider 
        {
            get { return _physicalPageProvider; }
            set
            {
                ArgCheck.NotNull(value);
                _physicalPageProvider = value;
            }
        }

        public IPageLayoutAnalyzer LayoutAnalyzer 
        {
            get { return _pageLayoutAnalyzer; }
            set
            {
                ArgCheck.NotNull(value);
                _pageLayoutAnalyzer = value;
            }
        }

        #endregion



        // Notion of screen page number is fuzzy/undefined.
        // We do not render the whole document at once. Current 
        // page number depends on screen page size, whitespace 
        // in physical pages, etc.

        public Bitmap RenderPage(float positionInBook)
        {
            throw new NotImplementedException();
        }

        //public float Position { get; } 

        public Bitmap RenderFirstPage() { return RenderPage(0); }

        public Bitmap RenderNextPage()
        {
            throw new NotImplementedException();
        }
        public Bitmap RenderPreviousPage()
        {
            throw new NotImplementedException();
        }

        /*
        // MUST have this, to know the next page
        // This is the position equivalent
        Bitmap ScreenPage;
        int LastPhysicalPageNumber;
        int LastPhysicalPageTop;
        PageLayoutInfo LastPhysicalPageLayout;
        */

        readonly Size LayoutRenderSize = new Size(1000, 1000);

        internal Bitmap RenderScreenPageToBitmap(int physicalPageNum, int topOfPhysicalPage)
        {
            ArgCheck.InRange(physicalPageNum, 1, PhysicalPageProvider.PageCount, "physicalPageNum");

            // 24bpp format for compatibility with AForge
            Bitmap screenPage = new Bitmap(ScreenPageSize.Width, ScreenPageSize.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(screenPage))
            {
                int screenPageTop = 0;
                while (screenPageTop < ScreenPageSize.Height &&
                       physicalPageNum <= PhysicalPageProvider.PageCount)
                {
                    // Figure out layout
                    PageLayoutInfo pageLayout;
                    using (Bitmap bmpLayoutPage = PhysicalPageProvider.RenderPage(physicalPageNum, LayoutRenderSize))
                    {
                        pageLayout = LayoutAnalyzer.DetectPageLayout(bmpLayoutPage);
                    }

                    // Empty page special case
                    if (pageLayout.IsEmpty)
                    {
                        // TODO: do something more sensible
                        g.FillEllipse(Brushes.DarkSlateGray, 10, 10, 30, 30);
                        break;
                    }

                    // Render actual page. Bounded by width, but not height.
                    int maxWidth = (int)((float)ScreenPageSize.Width / pageLayout.BoundsRelative.Width);
                    Size displayPageMaxSize = new Size(maxWidth, int.MaxValue);

                    using (Bitmap bmpDisplayPage = PhysicalPageProvider.RenderPage(physicalPageNum, displayPageMaxSize))
                    {
                        pageLayout.ScaleBounds(bmpDisplayPage.Size);

                        g.DrawImage(bmpDisplayPage,
                            new Rectangle(0, screenPageTop, pageLayout.Bounds.Width, pageLayout.Bounds.Height),
                            pageLayout.Bounds, GraphicsUnit.Pixel);

                        // Debug -- top-of-page boundary
                        g.DrawLine(Pens.DarkRed, 0, screenPageTop, screenPage.Width, screenPageTop);
                    }

                    // NextPage
                    screenPageTop += pageLayout.Bounds.Height;
                    topOfPhysicalPage = 0;
                    physicalPageNum++;
                }
            }

            // TODO: adjust topOfPage and return for next screen page

            return screenPage;
        }

    }
}
