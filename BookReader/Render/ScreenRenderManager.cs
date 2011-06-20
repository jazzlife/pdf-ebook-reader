using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Model;
using PdfBookReader.Utils;
using System.Drawing;
using System.Drawing.Imaging;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Main class for rendering
    /// </summary>
    class ScreenRenderManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        readonly BookLibrary _library;

        Size _screenSize;

        // ScreenBook object for each book in the library
        Dictionary<Guid, ScreenBook> _screenBooks = new Dictionary<Guid, ScreenBook>();
        ScreenBook _curScreenBook = null;

        // Not dependent on book
        readonly DW<IPageSource> _pageSource;

        public ScreenRenderManager(BookLibrary library, Size screenSize)
        {
            ArgCheck.NotNull(library);

            _library = library;
            ScreenSize = screenSize;
            
            _library.BooksChanged += new EventHandler(_library_BooksChanged);
            _library.CurrentBookChanged += new EventHandler(OnCurrentBookChanged);
            // BUG: need to monotor current page in *all* books, not just one, but this is academic...
            _library.CurrentBookPositionChanged += new EventHandler(_library_CurrentBookPositionChanged);

            _pageSource = DW.Wrap<IPageSource>(new CachedPageSource());

            OnCurrentBookChanged(this, EventArgs.Empty);
        }

        public Book CurrentBook
        {
            get { return _library.CurrentBook; }
            set { _library.CurrentBook = value; }
        }

        public Size ScreenSize
        {
            get { return _screenSize; }
            set
            {
                _screenSize = value;

                // Set size for all existing screen books
                foreach (ScreenBook sb in _screenBooks.Values)
                {
                    sb.ScreenSize = value;
                }

                OnScreenSizeChanged();
            }
        }


        #region Event handlers

        void OnCurrentBookChanged(object sender, EventArgs e)
        {
            logger.Debug("CurrentBookChanged");

            // No change
            if (_curScreenBook != null &&
                _curScreenBook.Book == _library.CurrentBook) { return; }

            // Set the current screen book field
            _curScreenBook = GetScreenBook(_library.CurrentBook);

            // TODO: not sure what do do about ScrenBook closing
            // They're fairly light on resources, apart from the PDF file...
            // Prefetch manager may use them at unpredictable times, not sure when
            // it's appropriate to dispose of them.
        }

        /// <summary>
        /// Gets the ScreenBook object, or creates it if needed.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public ScreenBook GetScreenBook(Book book)
        {
            if (book == null) { return null; }

            ScreenBook sb;
            if (!_screenBooks.TryGetValue(book.Id, out sb))
            {
                sb = new ScreenBook(book, _screenSize);
                _screenBooks.Add(sb.Book.Id, sb);
            }
            return sb;
        }

        void _library_CurrentBookPositionChanged(object sender, EventArgs e)
        {
            // QQ: *maybe* should initiate page render (P3), but only if this class
            // is not the one that cased the position to change. Beware of the loop!

            logger.Debug("CurrentBookPositionChanged"); 
            // TODO: update cache policies
        }

        void _library_BooksChanged(object sender, EventArgs e)
        {
            logger.Debug("CurrentBookChanged");
            // TODO: update cache policies
        }

        void OnScreenSizeChanged()
        {
            logger.Debug("CurrentBookChanged");
            // TODO: update cache policies
        }

        #endregion

        #region Commands and UI properties

        public DW<Bitmap> Render(PositionInBook newPosition)
        {
            if (_curScreenBook == null) { throw new InvalidOperationException("No book"); }

            var pages = _curScreenBook.AssembleCurrentScreen(newPosition, _pageSource);
            return GetScreenBitmap(pages);
        }

        public DW<Bitmap> RenderNext()
        {
            if (_curScreenBook == null) { throw new InvalidOperationException("No book"); }

            var pages = _curScreenBook.AssembleNextScreen(_pageSource);
            return GetScreenBitmap(pages);
        }

        public DW<Bitmap> RenderPrevious()
        {
            if (_curScreenBook == null) { throw new InvalidOperationException("No book"); }

            var pages = _curScreenBook.AssemblePreviousScreen(_pageSource);
            return GetScreenBitmap(pages);
        }

        public bool HasNextScreen
        {
            get
            {
                if (_curScreenBook == null) { return false; }
                return _curScreenBook.HasNextScreen(_pageSource);
            }
        }

        public bool HasPreviousScreen
        {
            get
            {
                if (_curScreenBook == null) { return false; }
                return _curScreenBook.HasPreviousScreen(_pageSource);
            }
        }

        #endregion


        #region Drawing

        DW<Bitmap> GetScreenBitmap(List<Page> pages)
        {
            DW<Bitmap> screenBmp = DW.Wrap(new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format24bppRgb));
            using (Graphics g = Graphics.FromImage(screenBmp.o))
            {
                DrawScreenBefore(g);

                foreach (Page page in pages)
                {
                    DrawPhysicalPage(g, page);
                }

                DrawScreenAfter(g);
            }

            return screenBmp;
        }

        void DrawScreenBefore(Graphics g)
        {
            g.FillRectangle(Brushes.White, 0, 0, ScreenSize.Width, ScreenSize.Height);
        }

        void DrawScreenAfter(Graphics g)
        {

        }

        void DrawPhysicalPage(Graphics g, Page curPage)
        {
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


    }
}
