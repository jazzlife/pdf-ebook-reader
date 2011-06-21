using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PdfBookReader.Model;

namespace PdfBookReader.UI
{
    public partial class MainForm : Form
    {
        BookLibrary _library;

        public MainForm()
        {
            InitializeComponent();

            pLibrary.Dock = DockStyle.Fill;
            pReading.Dock = DockStyle.Fill;
            pReading.Visible = false;

            _library = BookLibrary.Load(BookLibrary.DefaultFilename);

            if (!DesignMode)
            {
                pLibrary.Initialize(_library);
                pReading.Initialize(_library);
            }

        }

        private void pLibrary_Load(object sender, EventArgs e)
        {
            // Open last book at start
            if (_library.CurrentBook != null)
            {
                pLibrary_OpenBook(this, new OpenBookEventArgs(_library.CurrentBook));
            }

        }

        private void pLibrary_OpenBook(object sender, OpenBookEventArgs e)
        {
            this.Text = "eBook - " + e.Book.Title;

            _library.CurrentBook = e.Book;

            pReading.Book = e.Book;
            pReading.Visible = true;
            pLibrary.Visible = false;
        }

        private void pReading_GoToLibrary(object sender, EventArgs e)
        {
            this.Text = "eBook - Library";

            _library.CurrentBook = null;

            pReading.Book = null;
            pLibrary.Visible = true;
            pReading.Visible = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _library.Save();
        }

    }
}
