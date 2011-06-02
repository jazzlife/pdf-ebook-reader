using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PdfBookReader.Utils;
using System.Diagnostics;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Renders screen pages based on physical pages.
    /// Keeps track of current page with ability to request next/previous.
    /// </summary>
    public partial class ScreenPageProvider : IDisposable
    {
        public bool DrawDebugMarks = true;

        Size _screenSize;
        IPhysicalPageProvider _physicalPageProvider;
        IPageLayoutAnalyzer _pageLayoutAnalyzer;

        public ScreenPageProvider(
            IPhysicalPageProvider physicalPageProvider,
            IPageLayoutAnalyzer layoutAnalyzer,
            Size screenPageSize)
        {
            ScreenSize = screenPageSize;
            PhysicalPageProvider = physicalPageProvider;
            LayoutAnalyzer = layoutAnalyzer;
        }

        #region Config Properties

        public Size ScreenSize 
        {
            get { return _screenSize; }
            private set
            {
                ArgCheck.NotEmpty(value);

                _screenSize = value;
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

        public Bitmap RenderFirstPage() 
        {
            TopPage = null;
            BottomPage = null;
            return RenderNextPage();
        }

        public Bitmap RenderLastPage()
        {
            TopPage = null;
            BottomPage = null;
            return RenderLastPage();
        }

        public Bitmap RenderCurrentPage(Size newScreenPageSize)
        {
            Size oldSize = ScreenSize;
            ScreenSize = newScreenPageSize;
            RenderCurrent r = new RenderCurrent(this, oldSize);
            return r.Run();
        }

        public Bitmap RenderNextPage()
        {
            RenderDown r = new RenderDown(this);
            return r.Run();
        }

        public Bitmap RenderPreviousPage()
        {
            RenderUp r = new RenderUp(this);
            return r.Run();
        }

        #region PhysicalPageInfo fields

        // NOTE: careful when disposing these. Generally, should be disposed on assignment, but
        // NOT if TopPage == BottomPage
        PhysicalPageInfo _topPage = null;
        PhysicalPageInfo _bottomPage = null;
             
        /// <summary>
        /// Top physical page of the current screen page.
        /// Previous page needs this when rendering (RenderUp)
        /// </summary>
        PhysicalPageInfo TopPage 
        { 
            get { return _topPage; }
            set 
            {
                Trace.WriteLine("TopPage set to: " + value);
                value.AssignNewDisposeOld(ref _topPage, _bottomPage);
            }
        }

        /// <summary>
        /// Bottom physical page of current screen page
        /// Next page needs this when rendering (RenderDown)
        /// </summary>
        PhysicalPageInfo BottomPage 
        { 
            get { return _bottomPage; }
            set 
            {
                Trace.WriteLine("BottomPage set to: " + value);
                value.AssignNewDisposeOld(ref _bottomPage, _topPage);
            }
        }

        #endregion

        #region Drawing
        void DrawScreenBefore(Graphics g)
        {
            g.FillRectangle(Brushes.White, 0, 0, ScreenSize.Width, ScreenSize.Height);
        }

        void DrawScreenAfter(Graphics g)
        {
            if (DrawDebugMarks)
            {
                // g.DrawRectangle(Pens.Blue, 0, 0, ScreenSize.Width -1, ScreenSize.Height - 1);
            }
        }

        void DrawPhysicalPage(Graphics g, PhysicalPageInfo curPage)
        {
            Trace.WriteLine("DrawPage: " + curPage);

            // Special case: empty page
            if (curPage.Layout.IsEmpty)
            {
                // TODO: do something prettier
                g.FillEllipse(Brushes.DarkSlateGray, 10, 10, 30, 30);

                return;
            }

            // Render current page
            Rectangle destRect = new Rectangle(0, curPage.TopOnScreen,
                    curPage.ContentBounds.Width, curPage.ContentBounds.Height);
            Rectangle srcRect = curPage.ContentBounds;

            g.DrawImage(curPage.Image, destRect, srcRect, GraphicsUnit.Pixel);

            // Debug drawing
            if (DrawDebugMarks)
            {
                if (curPage.TopOnScreen >= 0)
                {
                    g.DrawStringBoxed("Page #" + curPage.PageNum, 0, curPage.TopOnScreen);
                }
                else
                {
                    g.DrawStringBoxed("Page #" + curPage.PageNum, ScreenSize.Width / 3, 0, bgBrush: Brushes.Gray);
                }
                g.DrawLineHorizontal(Pens.LightGray, curPage.TopOnScreen);
                g.DrawLineHorizontal(Pens.LightBlue, curPage.BottomOnScreen - 1);
            }
        }

        #endregion


        // Smaller probably renders faster, but rows are hard to distinguish.
        // experiment later
        readonly Size LayoutRenderSize = new Size(1000, 1000);

        /// <summary>
        /// Get physical page info. Null if pageNum is out of range.
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        PhysicalPageInfo GetPhysicalPage(int pageNum)
        {
            // No physical page
            if (pageNum < 1 || pageNum > PhysicalPageProvider.PageCount)
            {
                Trace.WriteLine("GetPhysicalPage: null, pageNum out of range: " + pageNum);
                return null;
            }

            Trace.WriteLine("GetPhysicalPage: pageNum = " + pageNum);

            // NOTE: rendering the page twice -- we need the layout in order to figure out
            // the best dimensions for the final render.

            PageLayoutInfo layout;
            using (Bitmap bmpLayoutPage = PhysicalPageProvider.RenderPage(pageNum, LayoutRenderSize))
            {
                layout = LayoutAnalyzer.DetectPageLayout(bmpLayoutPage);
            }

            // Render actual page. Bounded by width, but not height.
            int maxWidth = (int)((float)ScreenSize.Width / layout.BoundsRelative.Width);
            Size displayPageMaxSize = new Size(maxWidth, int.MaxValue);

            Bitmap image = PhysicalPageProvider.RenderPage(pageNum, displayPageMaxSize);

            layout.ScaleBounds(image.Size);

            return new PhysicalPageInfo(pageNum, image, layout);
        }

        public void Dispose()
        {
            // Only dispose disposable items we specifically created
            TopPage = null;
            BottomPage = null;
        }
    }
}
