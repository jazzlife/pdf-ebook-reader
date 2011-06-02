using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfBookReader.Metadata;

namespace PdfBookReader.UI
{
    public partial class ReadingPanel : UserControl
    {
        Book _book;

        public ReadingPanel()
        {
            InitializeComponent();
        }

        public Book Book
        {
            get { return _book; }

            set
            {
                if (value == _book) { return; }

                // TODO: load book
                _book = value;

                //pdfView.OpenFile(_book.Filename);
            }
        }

        public event EventHandler GoToLibrary;

        private void bLibrary_Click(object sender, EventArgs e)
        {
            if (GoToLibrary != null)
            {
                GoToLibrary(this, EventArgs.Empty);
            }
        }
    }
}
