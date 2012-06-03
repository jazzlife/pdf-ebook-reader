using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Model;
using BookReader.Utils;
using BookReader.Base.ViewModel;

namespace BookReader.ViewModel
{
    class BookViewModel : ViewModelBase
    {
        public Book Book { get; private set; }

        public BookViewModel(Book book)
        {
            ArgCheck.NotNull(book);
            Book = book;
            // TODO: handle position changed
        }

        // Only expose properties required for the view
        
        public String Title { get { return Book.Title; } }

        // Current position
        // - if it changed, then length indicator also changed

        public String LengthIndicator 
        { 
            get { return GetLengthIndicator(); }
        }

        string GetLengthIndicator()
        {
            var pos = Book.CurrentPosition;
            if (pos == null) { return "[?]"; }

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            // How long is the book
            int adjLen = GetAdjustedLength(pos.PageCount);
            int adjPos = (int)Math.Round(pos.PositionUnit * adjLen);

            sb.Append('#', adjPos);
            sb.Append('-', adjLen - adjPos);

            sb.Append("]");

            return sb.ToString();
        }

        int GetAdjustedLength(int pageCount)
        {
            if (pageCount <= 1) { return 1; }
            if (pageCount < 3) { return 3; }
            if (pageCount < 10) { return 4; }
            if (pageCount < 100) { return 5; }
            if (pageCount < 300) { return 7; }
            if (pageCount < 500) { return 9; }
            else return 10;
        }

        
    }
}
