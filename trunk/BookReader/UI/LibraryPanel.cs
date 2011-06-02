using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using PdfBookReader.Utils;
using PdfBookReader.Metadata;

namespace PdfBookReader.UI
{
    public partial class LibraryPanel : UserControl
    {
        BookLibrary _library;

        public LibraryPanel()
        {
            InitializeComponent();

            LoadBookLibrary();
        }

        void LoadBookLibrary()
        {
            if (File.Exists(BookLibrary.DefaultFilename))
            {
                try
                {
                    _library = BookLibrary.Load(BookLibrary.DefaultFilename);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError("Failed loading library file. " + e);
                    // TODO: tracing

                    _library = new BookLibrary();
                }
            }
            else
            {
                _library = new BookLibrary();
            }

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
            UpdateListViewItems();
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
                    _library.Books.Remove(bookItem.Book);
                    lbBooks.Items.Remove(bookItem);
                }
                _library.Save();
            }
        }

        private void lbBooks_DoubleClick(object sender, EventArgs e)
        {
            if (lbBooks.SelectedItems.Count == 0) { return; }

            Book bookToOpen = (lbBooks.SelectedItems[0] as BookListViewItem).Book;
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
