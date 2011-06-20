using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Model;
using PdfBookReader.Utils;

namespace PdfBookReader.Render.Cache
{
    [Immutable]
    class PageCacheContext
    {
        public readonly int ScreenWidth;

        public readonly BookPosition CurrentBookPosition;
        public readonly BookPosition[] BookPositions;

        public PageCacheContext(int screenWidth, BookLibrary library)
        {
            ScreenWidth = screenWidth;

            if (library.CurrentBook != null)
            {
                CurrentBookPosition = new BookPosition(library.CurrentBook);
            }
            else
            {
                CurrentBookPosition = null;
            }

            BookPositions = library.Books.Select(x => new BookPosition(x)).ToArray();
        }

        public class BookPosition
        {
            public Guid BookId;
            public PositionInBook Position;
            public BookPosition(Book b)
            {
                BookId = b.Id;
                Position = b.CurrentPosition;
            }
        }
    }

    interface IPageCacheContextManager
    {
        PageCacheContext CacheContext { get; }
        event EvHandler<PageCacheContext> CacheContextChanged;
    }

}
