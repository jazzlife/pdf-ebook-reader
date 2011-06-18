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

            _library = BookLibrary.Load(BookLibrary.DefaultFilename);

            pLibrary.Initialize(_library);
            pReading.Initialize(_library);

            pLibrary.Dock = DockStyle.Fill;
            pReading.Dock = DockStyle.Fill;
            pReading.Visible = false;
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
            pLibrary.Visible = true;
            pReading.Visible = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _library.Save();
        }
    }
}
