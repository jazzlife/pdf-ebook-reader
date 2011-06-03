using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfBookReader.Metadata;
using PdfBookReader.Render;
using PdfBookReader.Utils;

namespace PdfBookReader.UI
{
    public partial class ReadingPanel : UserControl
    {
        Book _book;

        IPhysicalPageProvider _physicalPageProvider;
        ScreenPageProvider _screenPageProvider;

        Bitmap _currentScreenImage;

        public ReadingPanel()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Book Book
        {
            get { return _book; }

            set
            {
                ArgCheck.NotNull(value);

                if (value == _book) { return; }

                _book = value;

                // Rendering components
                PhysicalPageProvider = new PdfPhysicalPageProvider(_book.Filename);
                IPageLayoutAnalyzer analyzer = new BlobPageLayoutAnalyzer(); // not disposable
                ScreenProvider = new ScreenPageProvider(PhysicalPageProvider, analyzer, pbContent.Size);

                // TODO: render page at stored position in the book
                // (no the first "current" page).
                CurrentPageImage = ScreenProvider.RenderCurrentPage(pbContent.Size);
                UpdateBookProgressBar();
            }
        }

        private Bitmap CurrentPageImage
        {
            get { return _currentScreenImage; }
            set
            {
                if (value == null) { return; }
                
                pbContent.Image = null;
                value.AssignNewDisposeOld(ref _currentScreenImage);
                pbContent.Image = _currentScreenImage;
            }
        }

        private ScreenPageProvider ScreenProvider
        {
            get { return _screenPageProvider; }
            set { value.AssignNewDisposeOld(ref _screenPageProvider); }
        }

        private IPhysicalPageProvider PhysicalPageProvider
        {
            get { return _physicalPageProvider; }
            set { value.AssignNewDisposeOld(ref _physicalPageProvider); }
        }

        public event EventHandler GoToLibrary;

        private void pbContent_Resize(object sender, EventArgs e)
        {
            // Re-start the timer. Page resize will happen *after*
            // the time fires (timer.Interval after the last 
            // resize event.
            timerResize.Stop();
            timerResize.Start();

            // TODO: sometimes the delay is unnecessary, when we 
            // know a resize was intentional (e.g. setting it programmatically)
        }

        private void timerResize_Tick(object sender, EventArgs e)
        {
            timerResize.Stop();
            if (ScreenProvider == null) { return; }

            CurrentPageImage = ScreenProvider.RenderCurrentPage(pbContent.Size);
            UpdateBookProgressBar();
        }

        private void bLibrary_Click(object sender, EventArgs e)
        {
            if (GoToLibrary != null) { GoToLibrary(this, EventArgs.Empty); }
        }

        private void bNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageImage = ScreenProvider.RenderNextPage();
            if (CurrentPageImage == null) { return; }
            UpdateBookProgressBar();
        }

        private void bPrevPage_Click(object sender, EventArgs e)
        {
            CurrentPageImage = ScreenProvider.RenderPreviousPage();
            if (CurrentPageImage == null) { return; }
            UpdateBookProgressBar();
        }

        const int WidthIncrement = 100;
        private void bWidthPlus_Click(object sender, EventArgs e)
        {
            pMargins.Width += WidthIncrement;
            pMargins.Left -= WidthIncrement / 2;
        }

        private void bWidthMinus_Click(object sender, EventArgs e)
        {
            pMargins.Width -= WidthIncrement;
            pMargins.Left += WidthIncrement / 2;
        }

        void UpdateBookProgressBar()
        {
            float posF = ScreenProvider.Position;
            int pageCount = ScreenProvider.PhysicalPageProvider.PageCount;
            bookProgressBar.Value = (int)(posF * bookProgressBar.Maximum);
            lbPageNum.Text = String.Format("{0:0.0}/{1}", 1 + (posF * pageCount), pageCount);
        }

        private void bookProgressBar_MouseUp(object sender, MouseEventArgs e)
        {
            // Set position
            float pos = (float)e.X / bookProgressBar.Width;
            if (pos > 1) { pos = 1; }
            if (pos < 0) { pos = 0; }

            CurrentPageImage = ScreenProvider.RenderPage(pos);
            UpdateBookProgressBar();
        }


    }
}
