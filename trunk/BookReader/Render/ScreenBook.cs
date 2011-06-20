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

        DW<IBookProvider> _bookProvider;

        public ScreenBook(Book book, Size screenPageSize)
        {
            ArgCheck.NotNull(book, "book");

            Book = book;
            ScreenSize = screenPageSize;

            // Slightly hacky but best way to do it
            // -- set the book position info if it's null
            if (Book.CurrentPosition == null)
            {
                Book.CurrentPosition = PositionInBook.FromPhysicalPage(1, BookProvider.o.PageCount);
            }
        }

        #region Public properties

        public Size ScreenSize 
        {
            get { return _screenSize; }
            set 
            {
                if (_screenSize == value) { return; }

                _screenSize = value; 
            }
        }

        #endregion

        public DW<IBookProvider> BookProvider
        {
            get
            {
                if (_bookProvider == null)
                {
                    _bookProvider = DW.Wrap<IBookProvider>(new PdfBookPageProvider(Book.Filename));
                }
                return _bookProvider;
            }
            // TODO: dispose when approrpiate
        }

        PositionInBook GetBookPosition()
        {
            PositionInBook pos = Book.CurrentPosition;
            if (pos == null)
            {
                pos = PositionInBook.FromPhysicalPage(1, BookProvider.o.PageCount);
                Book.CurrentPosition = pos;
            }
            return pos;
        }

        void SetBookPosition(PositionInBook newPosition)
        {
            Book.CurrentPosition = newPosition;
        }

        // Assemble screen
        public List<Page> AssembleCurrentScreen(PositionInBook newPosition, DW<IPageSource> pageContentSource)
        {
            return AssembleScreenHelper(newPosition,
                    new AssembleCurrentScreenAlgorithm(pageContentSource, BookProvider));
        }


        public List<Page> AssembleNextScreen(DW<IPageSource> pageContentSource)
        {
            PositionInBook position = GetBookPosition();
            return AssembleScreenHelper(position,
                new AssembleNextScreenAlgorithm(pageContentSource, BookProvider));
        }

        public List<Page> AssemblePreviousScreen(DW<IPageSource> pageContentSource)
        {
            PositionInBook position = GetBookPosition();
            return AssembleScreenHelper(position,
                new AssemblePreviousScreenAlgorithm(pageContentSource, BookProvider));
        }

        /// <summary>
        /// Assemble the screen and update the position within the book
        /// </summary>
        /// <param name="position"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        List<Page> AssembleScreenHelper(PositionInBook position, AssembleScreenAlgorithm algorithm)
        {
            if (!algorithm.CanApply(position, ScreenSize)) { return null; }

            var rv = algorithm.AssembleScreen(ref position, ScreenSize);
            SetBookPosition(position);
            return rv;
        }

        public bool HasNextScreen(DW<IPageSource> pageContentSource)
        {
            PositionInBook position = GetBookPosition();
            AssembleScreenAlgorithm alg = new AssembleNextScreenAlgorithm(pageContentSource, BookProvider);
            return alg.CanApply(position, ScreenSize);
        }

        public bool HasPreviousScreen(DW<IPageSource> pageContentSource)
        {
            PositionInBook position = GetBookPosition();
            AssembleScreenAlgorithm alg = new AssemblePreviousScreenAlgorithm(pageContentSource, BookProvider);
            return alg.CanApply(position, ScreenSize);
        }

        // Close book -- dispose resources in use
        public void Close()
        {
            // Dispose items we specifically created
            if (_bookProvider != null)
            {
                _bookProvider.DisposeItem();
            }
        }
    }

}
