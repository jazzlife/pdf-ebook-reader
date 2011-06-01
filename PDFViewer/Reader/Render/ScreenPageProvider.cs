using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PDFViewer.Reader.Utils;
using System.Diagnostics;

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
            ScreenSize = screenPageSize;
            PhysicalPageProvider = physicalPageProvider;
            LayoutAnalyzer = layoutAnalyzer;
        }

        #region Config Properties

        public Size ScreenSize 
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
            return RenderDown();
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


        #region PhysicalPageInfo class and fields

        class PhysicalPageInfo : IDisposable
        {
            public readonly int PageNum; // page number in document, 1-n

            Bitmap _image; // physical page image
            PageLayoutInfo _layout; // content layout

            // Distance between top of screen page and content bounds content bounds and 

            /// <summary>
            /// Distance between content bounds top and creen page top 
            /// screen.Top - countentBounds.Top
            /// </summary>
            public int TopOnScreen = 0;

            private Bitmap bitmap;

            public PhysicalPageInfo(int pageNum, Bitmap image, PageLayoutInfo layout)
            {
                ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");
                ArgCheck.NotNull(image);

                PageNum = pageNum;
                _image = image;
                _layout = layout;

                ContentBounds = Layout.Bounds;
            }

            public Bitmap Image { get { return _image; } }
            public PageLayoutInfo Layout { get { return _layout; } }

            // For convenience
            public int BottomOnScreen { get { return TopOnScreen + ContentBounds.Height; } }

            /// <summary>
            /// Usually same as Layout.Bounds, but can be set differently in some
            /// tweaking scenarios (e.g. to avoid splitting a row)
            /// </summary>
            public Rectangle ContentBounds { get; set; }
        
            public void  Dispose()
            {
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                    _layout = null;
                    ContentBounds = Rectangle.Empty;
                }
            }

            /*
            public PhysicalPageInfo Clone()
            {
                return new PhysicalPageInfo(this.PageNum, this.Image, this.Layout);
            }
            */

            public override string ToString()
            {
                return "PhysicalPage #" + PageNum + " TopOnScreen = " + TopOnScreen;
            }
        }

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

                if (_topPage == value) { return; }
                if (_topPage != null &&
                    _topPage != _bottomPage) 
                { 
                    _topPage.Dispose(); 
                }
                _topPage = value;
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

                if (_bottomPage == value) { return; }
                if (_bottomPage != null &&
                    _bottomPage != _topPage) 
                { 
                    _bottomPage.Dispose(); 
                }
                _bottomPage = value;
            }
        }

        #endregion

        Bitmap RenderDown()
        {

            // Use bottom page from previous screen, or get an appropriate one
            PhysicalPageInfo curPage = RenderDown_GetStartingPage();

            // LastPage
            if (curPage == null || curPage.BottomOnScreen <= 0) { return null; }

            // 24bpp format for compatibility with AForge
            Bitmap screenBmp = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(screenBmp))
            {
                // Render other pages
                while (curPage != null)
                {
                    curPage = RenderDown_OnePage(curPage, g);
                }
            }

            Trace.WriteLine("RenderDown done: TopPage = " + TopPage);
            Trace.WriteLine("RenderDown done: BottomPage = " + BottomPage);
            Trace.WriteLine("");

            return screenBmp;
        }

        PhysicalPageInfo RenderDown_GetStartingPage()
        {
            PhysicalPageInfo curPage;
            if (BottomPage == null)
            {
                Trace.WriteLine("RenderDown_GetStarting: no BottomPage, getting first page in doc");
                curPage = GetPhysicalPage(1);
            }
            else if (BottomPage.TopOnScreen < ScreenSize.Height)
            {
                Trace.WriteLine("RenderDown_GetStarting: using BottomPage");

                // Bottom page included on screen. Adjust offset for next screen.
                curPage = BottomPage;
                curPage.TopOnScreen = BottomPage.TopOnScreen - ScreenSize.Height;
            }
            else
            {
                Trace.WriteLine("RenderDown_GetStarting: bottomPage is above screen, getting next page");

                // Render a new page
                // NOTE: null if past-the-last page
                curPage = GetPhysicalPage(BottomPage.PageNum + 1);
            }
            return curPage;
        }

        /// <summary>
        /// Draw current page.
        /// Set TopPage/BottomPage if appropriate.
        /// Return next page (or return null if none)
        /// </summary>
        /// <param name="curPage"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        PhysicalPageInfo RenderDown_OnePage(PhysicalPageInfo curPage, Graphics g)
        {
            Trace.WriteLine("RenderDownOnePage: curPage = " + curPage);

            DrawPage(g, curPage);

            // Special case: Empty page. 
            // Keep the page number at both top and bottom.
            if (curPage.Layout.IsEmpty)
            {
                TopPage = curPage;
                BottomPage = curPage;
                return null;
            }

            // Save new top page
            if (curPage.TopOnScreen <= 0)
            {
                TopPage = curPage;
            }

            // Save new bottom page
            if (curPage.BottomOnScreen >= ScreenSize.Height)
            {
                BottomPage = curPage;
            }

            // Special case -- last page
            if (curPage.PageNum == PhysicalPageProvider.PageCount)
            {
                // Last page, not filled
                BottomPage = curPage;
                return null;
            }

            if (curPage.BottomOnScreen >= ScreenSize.Height)
            {
                return null;
            }

            int newTop = curPage.BottomOnScreen;
            curPage = GetPhysicalPage(curPage.PageNum + 1);

            Debug.Assert(curPage != null, "curPage null, should not be, we checked earlier");

            curPage.TopOnScreen = newTop; 
            return curPage;
        }

        void DrawPage(Graphics g, PhysicalPageInfo curPage)
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
            if (curPage.TopOnScreen >= 0)
            {
                g.DrawStringBoxed("Page #" + curPage.PageNum, 0, curPage.TopOnScreen);
            }
            else
            {
                g.DrawStringBoxed("Page #" + curPage.PageNum, ScreenSize.Width/3, 0, bgBrush: Brushes.Gray);
            }
            g.DrawLineHorizontal(Pens.DarkRed, curPage.TopOnScreen);
            g.DrawLineHorizontal(Pens.DarkBlue, curPage.BottomOnScreen);
        }


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


    }
}
