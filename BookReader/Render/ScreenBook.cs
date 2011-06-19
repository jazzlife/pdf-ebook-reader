using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PdfBookReader.Utils;
using System.Diagnostics;
using PdfBookReader.Model;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Renders screen pages based on physical pages.
    /// Keeps track of current page with ability to request next/previous.
    /// </summary>
    partial class ScreenBook 
    {
        public readonly Book Book;

        Size _screenSize;

        DW<IBookPageProvider> _bookPageProvider;

        public ScreenBook(Book book, Size screenPageSize)
        {
            ArgCheck.NotNull(book, "book");

            Book = book;
            ScreenSize = screenPageSize;
        }

        #region Public properties

        public Size ScreenSize 
        {
            get { return _screenSize; }
            set 
            {
                if (_screenSize == value) { return; }

                _screenSize = value; 

                // Size changed, so top and bottom page are no longer relevant,
                // We should re-render based on Book.Position instead
                TopPage = null;
                BottomPage = null;
            }
        }

        #endregion

        public DW<IBookPageProvider> BookPageProvider
        {
            get
            {
                if (_bookPageProvider == null)
                {
                    _bookPageProvider = DW.Wrap<IBookPageProvider>(new PdfBookPageProvider(Book.Filename));
                }
                return _bookPageProvider;
            }
            // TODO: dispose when approrpiate
        }


        void UpdateBookPosition()
        {
            Book.CurrentPosition = GetCurrentPosition();
        }

        // The public one is Position property of Book
        PositionInBook GetCurrentPosition()
        {
            if (BookPageProvider == null) { throw new InvalidOperationException(); }

            if (TopPage == null)
            {
                return PositionInBook.FromPhysicalPage(1, BookPageProvider.o.PageCount);
            }

            return PositionInBook.FromPhysicalPage(
                TopPage.PageNum,
                BookPageProvider.o.PageCount,
                TopPage.TopOnScreen, TopPage.Layout.Bounds.Height);
        }

        /// <summary>
        /// Render the page at given position, and set it as current page
        /// </summary>
        /// <param name="positionInBook"></param>
        /// <returns></returns>
        public DW<Bitmap> RenderScreen(PositionInBook position, DW<IPageContentSource> pageContentSource)
        {
            // Initialize position if unknown
            if (position == null)
            {
                position = PositionInBook.FromPhysicalPage(1, BookPageProvider.o.PageCount);
            }

            ArgCheck.Equals(position.PageCount == BookPageProvider.o.PageCount, "position page count not same as current book");

            // Fix for showing the full last page
            TopPage = GetPageContent(position.PageNum, pageContentSource);

            TopPage.TopOnScreen = position.GetTopOnScreen(TopPage.Layout.Bounds.Height);

            // Render "current" page based on new TopPage. No change in size.
            RenderCurrent r = new RenderCurrent(this, pageContentSource, ScreenSize);
            return r.Run();
        }

        /// <summary>
        /// Render the first screen page, and set it as current page
        /// </summary>
        /// <returns></returns>
        public DW<Bitmap> RenderFirstPage(DW<IPageContentSource> pageContentSource) 
        {
            TopPage = null;
            BottomPage = null;
            return RenderNextScreen(pageContentSource);
        }

        /// <summary>
        /// Render the last screen page, and set it as current page
        /// </summary>
        /// <returns></returns>
        public DW<Bitmap> RenderLastPage(DW<IPageContentSource> pageContentSource)
        {
            TopPage = null;
            BottomPage = null;
            return RenderPreviousScreen(pageContentSource);
        }

        /// <summary>
        /// Render the current screen page at a different screen size.
        /// </summary>
        /// <param name="newScreenPageSize"></param>
        /// <returns></returns>
        public DW<Bitmap> RenderCurrentScreen(Size newScreenPageSize, DW<IPageContentSource> pageContentSource)
        {
            Size oldSize = ScreenSize;
            ScreenSize = newScreenPageSize;
            RenderCurrent r = new RenderCurrent(this, pageContentSource, oldSize);
            return r.Run();
        }

        /// <summary>
        /// Render the next screen, set it as current page.
        /// If there is no current page, renders the first page.
        /// If currently at last page, returns null.
        /// </summary>
        /// <returns></returns>
        public DW<Bitmap> RenderNextScreen(DW<IPageContentSource> pageContentSource)
        {
            if (!HasNextScreen) { throw new InvalidOperationException("no next screen."); }

            RenderDown r = new RenderDown(this, pageContentSource);
            return r.Run();
        }

        /// <summary>
        /// Render the previous screen, set it as current page.
        /// If there is no current page, renders the last page.
        /// If currently at first page, returns null.
        /// </summary>
        /// <returns></returns>
        public DW<Bitmap> RenderPreviousScreen(DW<IPageContentSource> pageContentSource)
        {
            if (!HasPreviousScreen) { throw new InvalidOperationException("no previous screen."); }

            RenderUp r = new RenderUp(this, pageContentSource);
            return r.Run();
        }

        /// <summary>
        /// True if the next page exists.
        /// </summary>
        /// <returns></returns>
        public bool HasNextScreen
        {
            get
            {
                if (BottomPage == null) { return true; }

                bool v = BottomPage.BottomOnScreen > ScreenSize.Height ||
                    (BottomPage.BottomOnScreen == ScreenSize.Height && BottomPage.PageNum < BookPageProvider.o.PageCount);

                return v;
            }
        }

        /// <summary>
        /// True if the previous page exists
        /// </summary>
        /// <returns></returns>
        public bool HasPreviousScreen
        {
            get 
            {
                if (TopPage == null) { return true; }
            
                bool v = TopPage.TopOnScreen < 0 ||
                    (TopPage.TopOnScreen == 0 && TopPage.PageNum > 1);

                return v;
            }
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
                value.AssignNewReturnOld(ref _topPage, _bottomPage);
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
                value.AssignNewReturnOld(ref _bottomPage, _topPage);
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

        }

        void DrawPhysicalPage(Graphics g, PageContent curPage)
        {
            Trace.WriteLine("DrawPage: " + curPage);

            // Render current page
            Rectangle destRect = new Rectangle(0, curPage.TopOnScreen,
                    curPage.Layout.Bounds.Width, curPage.Layout.Bounds.Height);
            Rectangle srcRect = curPage.Layout.Bounds;

            g.DrawImage(curPage.Image.o, destRect, srcRect, GraphicsUnit.Pixel);

#if DEBUG
            // Debug drawing of page numbers / boundaries
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
#endif
        }

        #endregion

        /// <summary>
        /// Get physical page info from the provider.
        /// Null if pageNum is out of range.
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        PageContent GetPageContent(int pageNum, DW<IPageContentSource> pageSource)
        {
            // No physical page
            if (pageNum < 1 || pageNum > BookPageProvider.o.PageCount)
            {
                Trace.WriteLine("GetPhysicalPage: null, pageNum out of range: " + pageNum);
                return null;
            }

            // Render actual page (may take long)
            return pageSource.o.GetPage(pageNum, ScreenSize, BookPageProvider);
        }

        // Close book -- dispose resources in use
        public void Close()
        {
            // Dispose items we specifically created
            TopPage = null;
            BottomPage = null;

            if (_bookPageProvider != null)
            {
                _bookPageProvider.DisposeItem();
            }
        }
    }

}
