using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Model;
using PdfBookReader.Utils;
using System.Drawing;

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
        ScreenBook _currentScreenBook = null;

        // Not dependent on book
        readonly DW<IPageContentSource> _pageSource;

        public ScreenRenderManager(BookLibrary library, Size screenSize)
        {
            ArgCheck.NotNull(library);

            _library = library;
            ScreenSize = screenSize;
            
            _library.BooksChanged += new EventHandler(_library_BooksChanged);
            _library.CurrentBookChanged += new EventHandler(OnCurrentBookChanged);
            // BUG: need to monotor current page in *all* books, not just one, but this is academic...
            _library.CurrentBookPositionChanged += new EventHandler(_library_CurrentBookPositionChanged);

            _pageSource = DW.Wrap<IPageContentSource>(new CachedPageContentSource());

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
            if (_currentScreenBook != null &&
                _currentScreenBook.Book == _library.CurrentBook) { return; }

            // Set the current screen book field
            _currentScreenBook = GetScreenBook(_library.CurrentBook);

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

        // TODO: global recoloring should be done here, not within each book
        public DW<Bitmap> Render(PositionInBook newPosition)
        {
            if (_currentScreenBook == null) { throw new InvalidOperationException("No book"); }
            return _currentScreenBook.RenderScreen(newPosition, _pageSource); 
        }

        public DW<Bitmap> RenderNext()
        {
            if (_currentScreenBook == null) { throw new InvalidOperationException("No book"); }
            return _currentScreenBook.RenderNextScreen(_pageSource); 
        }

        public DW<Bitmap> RenderPrevious()
        {
            if (_currentScreenBook == null) { throw new InvalidOperationException("No book"); }
            return _currentScreenBook.RenderPreviousScreen(_pageSource); 
        }

        public DW<Bitmap> RenderCurrent(Size newSize)
        {
            ScreenSize = newSize;
            if (_currentScreenBook == null) { throw new InvalidOperationException("No book"); }
            return _currentScreenBook.RenderCurrentScreen(newSize, _pageSource);
        }

        public bool HasNextScreen
        {
            get
            {
                if (_currentScreenBook == null) { return false; }
                return _currentScreenBook.HasNextScreen;
            }
        }

        public bool HasPreviousScreen
        {
            get
            {
                if (_currentScreenBook == null) { return false; }
                return _currentScreenBook.HasPreviousScreen;
            }
        }

        #endregion
    }
}
