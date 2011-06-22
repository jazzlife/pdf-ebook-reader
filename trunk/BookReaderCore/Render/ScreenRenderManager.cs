using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Model;
using BookReader.Utils;
using System.Drawing;
using System.Drawing.Imaging;
using BookReader.Render.Cache;
using AForge.Imaging.Filters;
using AForge;
using BookReader.Render.Filter;

namespace BookReader.Render
{
    /// <summary>
    /// Main class for rendering
    /// </summary>
    public class ScreenRenderManager : IPageCacheContextManager, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        readonly BookLibrary _library;

        Size _screenSize;

        // ScreenBook object for each book in the library
        Dictionary<Guid, ScreenBook> _screenBooks = new Dictionary<Guid, ScreenBook>();
        ScreenBook _curScreenBook = null;

        // Not dependent on book
        readonly DW<IPageSource> _pageSource;

        PaperColorFilter _paperColor;

        public ScreenRenderManager(BookLibrary library, Size screenSize)
        {
            ArgCheck.NotNull(library);

            _library = library;
            ScreenSize = screenSize;
            
            _library.BooksChanged += new EventHandler(_library_BooksChanged);
            _library.CurrentBookChanged += new EventHandler(OnCurrentBookChanged);
            // BUG: need to monotor current page in *all* books, not just one, but this is academic...
            _library.BookPositionChanged += new EventHandler(_library_CurrentBookPositionChanged);

            _pageSource = RenderFactory.ConcreteFactory.GetPageSource(this);

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

        /// <summary>
        /// For debugging cache (showing it in the UI)
        /// </summary>
        internal DW<PageCache> Cache
        {
            get 
            {
                CachedPageSource ps = _pageSource.o as CachedPageSource;
                if (ps != null) { return ps.Cache; }
                return null;
            }
        }

        public PaperColorFilter PaperColor
        {
            get { return _paperColor; }
            set 
            { 
                _paperColor = value;
                if (PaperColorChanged != null) { PaperColorChanged(this, EventArgs.Empty); }
            }
        }

        public event EventHandler PaperColorChanged;

        #region Event handlers

        void OnCurrentBookChanged(object sender, EventArgs e)
        {
            logger.Debug("CurrentBookChanged");

            // No change
            if (_curScreenBook != null &&
                _curScreenBook.Book == _library.CurrentBook) { return; }

            // Set the current screen book field
            _curScreenBook = GetScreenBook(_library.CurrentBook);

            OnCacheContextChanged();
        }

        /// <summary>
        /// Gets the ScreenBook object, or creates it if needed.
        /// 
        /// It's good to call Close() on ScreenBook objects sometimes -- 
        /// they will be re-opened automatically as needed.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        internal ScreenBook GetScreenBook(Book book)
        {
            lock (this)
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
        }

        void _library_CurrentBookPositionChanged(object sender, EventArgs e)
        {
            // QQ: *maybe* should initiate page render (P3), but only if this class
            // is not the one that cased the position to change. Beware of the loop!

            logger.Debug("CurrentBookPositionChanged");
            OnCacheContextChanged();
        }

        void _library_BooksChanged(object sender, EventArgs e)
        {
            logger.Debug("CurrentBookChanged");
            OnCacheContextChanged();
        }

        void OnScreenSizeChanged()
        {
            logger.Debug("CurrentBookChanged");
            OnCacheContextChanged();
        }

        #endregion

        #region Commands 

        public DW<Bitmap> Render()
        {
            if (_curScreenBook == null) { throw new InvalidOperationException("No book"); }
            return Render(_curScreenBook.Book.CurrentPosition);
        }

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
            if (pages == null) { return null; }



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

            foreach (var p in pages)
            {
                p.Return();
            }
            pages.Clear();

            // Apply the paper color filter
            if (_paperColor != null)
            {
                _paperColor.ApplyInPlace(screenBmp.o);
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
            /*
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
             */
            g.DrawLineHorizontal(Pens.LightBlue, curPage.TopOnScreen, 60);
            g.DrawLineHorizontal(Pens.Orange, curPage.BottomOnScreen - 1, 60);

#endif
        }

        #endregion

        #region IPageCacheContextManager

        PageCacheContext _cacheContext;
        event EvHandler<PageCacheContext> _cacheContextChanged;

        void OnCacheContextChanged()
        {
            _cacheContext = new PageCacheContext(ScreenSize.Width, _library);
            if (_cacheContextChanged != null) { _cacheContextChanged(this, EvArgs.Create(_cacheContext)); }
        }

        ScreenBook IPageCacheContextManager.GetScreenBook(Guid bookId)
        {
            // QQ: should we make this thread safe? Access to library is everywhere...
            lock (this)
            {
                Book book = _library.Books.FirstOrDefault(x => x.Id == bookId);
                if (book == null) { return null; }

                return GetScreenBook(book);
            }
        }

        PageCacheContext ICacheContextManager<PageCacheContext>.CacheContext
        {
            get { return _cacheContext; }
        }

        event EvHandler<PageCacheContext> ICacheContextManager<PageCacheContext>.CacheContextChanged
        {
            add { _cacheContextChanged += value; }
            remove { _cacheContextChanged -= value; }
        }

        #endregion

        public void Dispose()
        {
            if (_pageSource != null)
            {
                _pageSource.DisposeItem();
            }
        }

    }
}
