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
using BookReader.Properties;

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
        Dictionary<Guid, DW<IBookContent>> _bookContents = new Dictionary<Guid, DW<IBookContent>>();
        DW<IBookContent> _curBookContent = null;

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
                OnScreenSizeChanged();
            }
        }

        /// <summary>
        /// For debugging cache (showing it in the UI)
        /// </summary>
        internal DW<PageImageCache> Cache
        {
            get 
            {
                // TODO
                //CachedPageSource ps = _pageSource.o as CachedPageSource;
                //if (ps != null) { return ps.Cache; }
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
            if (_curBookContent != null &&
                _curBookContent.o.Book == _library.CurrentBook) { return; }

            // Set the current screen book field
            _curBookContent = GetBookContent(_library.CurrentBook);

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
        internal DW<IBookContent> GetBookContent(Book book)
        {
            lock (this)
            {
                if (book == null) { return null; }

                DW<IBookContent> bc;
                if (!_bookContents.TryGetValue(book.Id, out bc))
                {
                    bc = RenderFactory.Default.NewBookContent(book);
                    _bookContents.Add(bc.o.Book.Id, bc);
                }

                return bc;
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
            if (_curBookContent == null) { throw new InvalidOperationException("No book"); }
            return Render(_curBookContent.o.Book.CurrentPosition);
        }

        public DW<Bitmap> Render(PositionInBook newPosition)
        {
            if (_curBookContent == null) { throw new InvalidOperationException("No book"); }

            return RenderScreenHelper(newPosition,
                new AssembleCurrentScreenAlgorithm(_curBookContent));
        }

        public DW<Bitmap> RenderNext()
        {
            if (_curBookContent == null) { throw new InvalidOperationException("No book"); }

            PositionInBook position = _curBookContent.o.Position;
            return RenderScreenHelper(position,
                new AssembleNextScreenAlgorithm(_curBookContent));
        }

        public DW<Bitmap> RenderPrevious()
        {
            if (_curBookContent == null) { throw new InvalidOperationException("No book"); }

            PositionInBook position = _curBookContent.o.Position;
            return RenderScreenHelper(position,
                new AssemblePreviousScreenAlgorithm(_curBookContent));
        }

        public bool HasNextScreen
        {
            get
            {
                if (_curBookContent == null) { return false; }

                PositionInBook position = _curBookContent.o.Position;
                AssembleScreenAlgorithm alg = new AssembleNextScreenAlgorithm(_curBookContent);
                return alg.CanApply(position, ScreenSize);
            }
        }

        public bool HasPreviousScreen
        {
            get
            {
                if (_curBookContent == null) { return false; }

                PositionInBook position = _curBookContent.o.Position;
                AssembleScreenAlgorithm alg = new AssemblePreviousScreenAlgorithm(_curBookContent);
                return alg.CanApply(position, ScreenSize);
            }
        }

        /// <summary>
        /// Assemble the screen and update the position within the book
        /// </summary>
        /// <param name="position"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        DW<Bitmap> RenderScreenHelper(PositionInBook position, AssembleScreenAlgorithm algorithm)
        {
            if (_curBookContent == null) { throw new InvalidOperationException("No book"); }

            if (!algorithm.CanApply(position, ScreenSize)) { return null; }

            var pages = algorithm.AssembleScreen(ref position, ScreenSize);

            // Set the changed position
            _curBookContent.o.Book.CurrentPosition = position;

            return GetScreenBitmap(pages);
        }

        #endregion

        #region Drawing

        DW<Bitmap> GetScreenBitmap(List<PageOnScreen> pages)
        {
            if (pages == null) { return null; }

            DW<IBookContent> bc = GetBookContent(CurrentBook);

            DW<Bitmap> screenBmp = DW.Wrap(new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format24bppRgb));
            using (Graphics g = Graphics.FromImage(screenBmp.o))
            {
                DrawScreenBefore(g);

                foreach (PageOnScreen page in pages)
                {
                    PageImage image = bc.o.GetPageImage(page.PageNum, ScreenSize.Width);
                    
                    DrawPhysicalPage(g, page, image);

                    // Return to cache / for disposal
                    image.Return();
                }

                DrawScreenAfter(g);
            }

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

        void DrawPhysicalPage(Graphics g, PageOnScreen curPage, PageImage image)
        {
            // Render current page
            Rectangle destRect = new Rectangle(0, curPage.TopOnScreen,
                    curPage.Layout.Bounds.Width, curPage.Layout.Bounds.Height);
            Rectangle srcRect = curPage.Layout.Bounds;

            g.DrawImage(image.Image, destRect, srcRect, GraphicsUnit.Pixel);

            if (Settings.Default.Debug_DrawPageNumbers)
            {
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
            }
            if (Settings.Default.Debug_DrawPageEnd)
            {
                g.DrawLineHorizontal(Pens.LightBlue, curPage.TopOnScreen, 60);
                g.DrawLineHorizontal(Pens.Orange, curPage.BottomOnScreen - 1, 60);
            }

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

        DW<IBookContent> IPageCacheContextManager.GetBookContent(Guid bookId)
        {
            // QQ: should we make this thread safe? Access to library is everywhere...
            lock (this)
            {
                Book book = _library.Books.FirstOrDefault(x => x.Id == bookId);
                if (book == null) { return null; }

                return GetBookContent(book);
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
            if (_bookContents != null)
            {
                foreach (var bc in _bookContents.Values)
                {
                    bc.DisposeItem();
                }
                _bookContents.Clear();
                _bookContents = null;
            }
        }

    }
}
