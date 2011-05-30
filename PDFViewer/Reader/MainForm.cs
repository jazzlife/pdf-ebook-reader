using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PDFViewer.Reader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            pLibrary.Dock = DockStyle.Fill;
            pReading.Dock = DockStyle.Fill;
            pReading.Visible = false;
        }

        private void pLibrary_OpenBook(object sender, OpenBookEventArgs e)
        {
            this.Text = "eBook - " + e.Book.Title;
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
    }
}
