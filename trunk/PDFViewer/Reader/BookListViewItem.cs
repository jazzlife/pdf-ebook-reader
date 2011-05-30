using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PDFViewer.Reader
{
    class BookListViewItem : ListViewItem
    {
        public readonly Book Book;

        public BookListViewItem(Book book) : 
            base(book.Title)
        {
            Book = book;
        }
    }
}
