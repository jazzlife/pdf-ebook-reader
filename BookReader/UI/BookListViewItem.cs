using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfBookReader.Metadata;

namespace PdfBookReader
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
