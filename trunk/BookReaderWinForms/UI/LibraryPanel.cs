using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using BookReader.Model;
using BookReader.Utils;
using BookReaderUI.Properties;

namespace PdfBookReader.UI
{
    public partial class LibraryPanel : UserControl
    {
        BookLibrary _library;

        public LibraryPanel()
        {
            InitializeComponent();
        }

        public void Initialize(BookLibrary library)
        {
            ArgCheck.NotNull(library, "library");
            _library = library;

            _library.BooksChanged += _library_BooksChanged;
            _library.BookPositionChanged += _library_BookPositionChanged;
            UpdateListViewItems();
        }

        void _library_BookPositionChanged(object sender, EventArgs e)
        {
            // Update the progress bars
            UpdateListViewItems();
        }

        void _library_BooksChanged(object sender, EventArgs e)
        {
            UpdateListViewItems();
        }

        public event EventHandler<OpenBookEventArgs> OpenBook;

        private void LibraryPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (GetPdfFiles(e.Data) != null)
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }           
        }

        IEnumerable<String> GetPdfFiles(IDataObject dataObject)
        {
            String[] files = (String[])dataObject.GetData("FileDrop");
            if (files == null) { return null; }

            // TODO: instead of ".pdf", get supported formats from BookLibrary
            var pdfs = files.Where(x => Path.GetExtension(x).EqualsIC(".pdf"));
            if (pdfs.FirstOrDefault() == null) { return null; }
            return pdfs;
        }

        private void LibraryPanel_DragDrop(object sender, DragEventArgs e)
        {
            var files = GetPdfFiles(e.Data);
            if (files == null) { return; }

            _library.AddFiles(files);
            _library.Save();
        }

        void UpdateListViewItems()
        {
            lbBooks.SuspendLayout();
            
            lbBooks.Clear();
            foreach (Book book in _library.Books)
            {
                lbBooks.Items.Add(new BookListViewItem(book));
            }
            
            lbBooks.ResumeLayout();
        }

        private void lbBooks_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (BookListViewItem bookItem in lbBooks.SelectedItems)
                {
                    _library.RemoveBook(bookItem.Book);
                }
                _library.Save();
            }
        }

        private void lbBooks_DoubleClick(object sender, EventArgs e)
        {
            if (lbBooks.SelectedItems.Count == 0) { return; }

            Book bookToOpen = (lbBooks.SelectedItems[0] as BookListViewItem).Book;

            if (!File.Exists(bookToOpen.Filename))
            {
                MessageBox.Show(Resources.errBookFileNotFound.F(bookToOpen.Title), 
                    Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (OpenBook != null)
            {
                OpenBook(this, new OpenBookEventArgs(bookToOpen));
            }
        }
    }

    public class OpenBookEventArgs : EventArgs
    {
        public readonly Book Book;
        public OpenBookEventArgs(Book book)
        {
            if (book == null) { throw new ArgumentNullException("book"); }
            Book = book;
        }

    }
}
