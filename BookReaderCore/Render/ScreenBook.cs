using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using BookReader.Utils;
using System.Diagnostics;
using BookReader.Model;

namespace BookReader.Render
{
    /// <summary>
    /// Renders screen pages based on physical pages.
    /// Keeps track of current page with ability to request next/previous.
    /// </summary>
    class ScreenBook 
    {
        public readonly Book Book;

        Size _screenSize;

        DW<IBookContent> _bookContent;

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
            }
        }

        #endregion

        public DW<IBookContent> BookContent
        {
            get
            {
                if (_bookContent == null)
                {
                    _bookContent = RenderFactory.Default.GetBookContent(Book);
                }
                return _bookContent;
            }
            // TODO: dispose when approrpiate
        }

        void SetBookPosition(PositionInBook newPosition)
        {
            Book.CurrentPosition = newPosition;
        }

        // Assemble screen
        public List<PageOnScreen> AssembleCurrentScreen(PositionInBook newPosition)
        {
            return AssembleScreenHelper(newPosition,
                    new AssembleCurrentScreenAlgorithm(this));
        }

        public List<PageOnScreen> AssembleNextScreen()
        {
            PositionInBook position = BookContent.o.Position;
            return AssembleScreenHelper(position,
                new AssembleNextScreenAlgorithm(this));
        }

        public List<PageOnScreen> AssemblePreviousScreen()
        {
            PositionInBook position = BookContent.o.Position;
            return AssembleScreenHelper(position,
                new AssemblePreviousScreenAlgorithm(this));
        }

        /// <summary>
        /// Assemble the screen and update the position within the book
        /// </summary>
        /// <param name="position"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        List<PageOnScreen> AssembleScreenHelper(PositionInBook position, AssembleScreenAlgorithm algorithm)
        {
            if (!algorithm.CanApply(position, ScreenSize)) { return null; }

            var rv = algorithm.AssembleScreen(ref position, ScreenSize);
            SetBookPosition(position);
            return rv;
        }

        public bool HasNextScreen()
        {
            PositionInBook position = BookContent.o.Position;
            AssembleScreenAlgorithm alg = new AssembleNextScreenAlgorithm(this);
            return alg.CanApply(position, ScreenSize);
        }

        public bool HasPreviousScreen()
        {
            PositionInBook position = BookContent.o.Position;
            AssembleScreenAlgorithm alg = new AssemblePreviousScreenAlgorithm(this);
            return alg.CanApply(position, ScreenSize);
        }

        // Close book -- dispose resources in use
        public void Close()
        {
            // Dispose items we specifically created
            if (_bookContent != null)
            {
                _bookContent.DisposeItem();
            }
        }
    }

}
