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
    partial class ScreenPageProvider : IDisposable
    {
        public bool DrawDebugMarks = true;

        Size _screenSize;
        IPhysicalPageProvider _physicalPageProvider;
        IPageContentProvider _contentProvider;

        public ScreenPageProvider(
            IPhysicalPageProvider physicalPageProvider,
            IPageContentProvider contentProvider,
            Size screenPageSize)
        {
            ScreenSize = screenPageSize;
            PhysicalPageProvider = physicalPageProvider;
            ContentProvider = contentProvider;
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

        public IPageContentProvider ContentProvider
        {
            get { return _contentProvider; }
            set
            {
                ArgCheck.NotNull(value);
                _contentProvider = value;
            }
        }

        #endregion

        /// <summary>
        /// Position in the document changed
        /// </summary>
        public event EventHandler PositionChanged;

        /// <summary>
        /// Position of currnet screen within the book, 0-1
        /// 
        /// Notion of screen page number does not exist.
        /// We do not render the whole document at once. Current 
        /// page number depends on screen page size, whitespace 
        /// in physical pages, etc.
        /// </summary>
        public float Position
        {
            get
            {
                float pageIndex = (TopPage == null) ? 0 : TopPage.PageNum - 1;

                // Position within physical page (since screen breaks != physical page breaks)
                float positionWithinPage = 0;
                if (TopPage != null &&
                    TopPage.BottomOnScreen > 0 &&
                    TopPage.Layout.Bounds.Height > 0)
                {
                    positionWithinPage = -(float)TopPage.TopOnScreen / TopPage.Layout.Bounds.Height;
                }

                pageIndex += positionWithinPage;

                float position = pageIndex / PhysicalPageProvider.PageCount;

                // This can happen (e.g. when page top is positive 
                // on first page after iterating backwards)
                if (position < 0) { position = 0; }
                if (position > 1) { position = 1; }

                return position;
            }
        }

        /// <summary>
        /// Render the page at given position, and set it as current page
        /// </summary>
        /// <param name="positionInBook"></param>
        /// <returns></returns>
        public Bitmap RenderPage(float positionInBook)
        {
            ArgCheck.IsRatio(positionInBook, "positionInBook");

            // Fix for showing the full last page
            // 1.0 position corresponds to the END of last page, we want the start
            float onePageIncrement = 1.0f / PhysicalPageProvider.PageCount;
            if (positionInBook > 1 - onePageIncrement) { positionInBook = 1 - onePageIncrement; }

            // Find and set TopPage
            float pageIndex = positionInBook * PhysicalPageProvider.PageCount;

            int pageNum = (int)pageIndex + 1;
            TopPage = GetPhysicalPage(pageNum);

            if (TopPage.Layout.Bounds.Height > 0)
            {
                // Fractional part of pageIndex
                float topOnScreenRelative = pageIndex - ((int)pageIndex);
                TopPage.TopOnScreen = -(int)(topOnScreenRelative * TopPage.Layout.Bounds.Height);
            }

            // Render "current" page based on new TopPage. No change in size.
            RenderCurrent r = new RenderCurrent(this, ScreenSize);
            return r.Run();
        }

        /// <summary>
        /// Render the first screen page, and set it as current page
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderFirstPage() 
        {
            TopPage = null;
            BottomPage = null;
            return RenderNextPage();
        }

        /// <summary>
        /// Render the last screen page, and set it as current page
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderLastPage()
        {
            TopPage = null;
            BottomPage = null;
            return RenderPreviousPage();
        }

        /// <summary>
        /// Render the current screen page at a different screen size.
        /// </summary>
        /// <param name="newScreenPageSize"></param>
        /// <returns></returns>
        public Bitmap RenderCurrentPage(Size newScreenPageSize)
        {
            Size oldSize = ScreenSize;
            ScreenSize = newScreenPageSize;
            RenderCurrent r = new RenderCurrent(this, oldSize);
            return r.Run();
        }

        /// <summary>
        /// Render the next screen, set it as current page.
        /// If there is no current page, renders the first page.
        /// If currently at last page, returns null.
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderNextPage()
        {
            RenderDown r = new RenderDown(this);
            return r.Run();
        }

        /// <summary>
        /// Checks if there next page exists.
        /// </summary>
        /// <returns></returns>
        public bool HasNextPage()
        {
            float onePage = 1.0f / PhysicalPageProvider.PageCount;
            return Position < (1 - onePage);
        }

        /// <summary>
        /// Render the previous screen, set it as current page.
        /// If there is no current page, renders the last page.
        /// If currently at first page, returns null.
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderPreviousPage()
        {
            RenderUp r = new RenderUp(this);
            return r.Run();
        }

        public bool HasPreviousPage()
        {
            return Position > 0;
        }

        #region PhysicalPageInfo fields

        // NOTE: careful when disposing these. Generally, should be disposed on assignment, but
        // NOT if TopPage == BottomPage
        PageContent _topPage = null;
        PageContent _bottomPage = null;
             
        /// <summary>
        /// Top physical page of the current screen page.
        /// Previous page needs this when rendering (RenderUp)
        /// </summary>
        PageContent TopPage 
        { 
            get { return _topPage; }
            set 
            {
                Trace.WriteLine("TopPage set to: " + value);
                value.AssignNewDisposeOld(ref _topPage, _bottomPage);

                if (PositionChanged != null)
                {
                    PositionChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Bottom physical page of current screen page
        /// Next page needs this when rendering (RenderDown)
        /// </summary>
        PageContent BottomPage 
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

        void DrawPhysicalPage(Graphics g, PageContent curPage)
        {
            Trace.WriteLine("DrawPage: " + curPage);

            // Render current page
            Rectangle destRect = new Rectangle(0, curPage.TopOnScreen,
                    curPage.Layout.Bounds.Width, curPage.Layout.Bounds.Height);
            Rectangle srcRect = curPage.Layout.Bounds;

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

        /// <summary>
        /// Get physical page info from the provider.
        /// Null if pageNum is out of range.
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        PageContent GetPhysicalPage(int pageNum)
        {
            // No physical page
            if (pageNum < 1 || pageNum > PhysicalPageProvider.PageCount)
            {
                Trace.WriteLine("GetPhysicalPage: null, pageNum out of range: " + pageNum);
                return null;
            }

            Trace.WriteLine("GetPhysicalPage: pageNum = " + pageNum);

            // Render actual page (may take long)
            return ContentProvider.RenderPhysicalPage(pageNum, ScreenSize, PhysicalPageProvider);
        }        


        public void Dispose()
        {
            // Only dispose disposable items we specifically created
            TopPage = null;
            BottomPage = null;
        }
    }

}
