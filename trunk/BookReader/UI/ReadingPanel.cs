using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfBookReader.Model;
using PdfBookReader.Render;
using PdfBookReader.Utils;
using PdfBookReader.Render.Cache;

namespace PdfBookReader.UI
{
    public partial class ReadingPanel : UserControl
    {
        BookLibrary _library;
        Book _book;

        ScreenRenderManager _renderManager;
        DW<Bitmap> _currentScreenImage;

        // read-only
        DW<IPageSource> _contentSource;

        public ReadingPanel()
        {
            InitializeComponent();
        }

        public void Initialize(BookLibrary library)
        {
            ArgCheck.NotNull(library, "library");
            _library = library;

            _renderManager = new ScreenRenderManager(_library, pbContent.Size);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Book Book
        {
            get { return _book; }
            set
            {
                if (value == _book) { return; }

                if (_book != null)
                {
                    // Unsubsribe
                    _book.CurrentPositionChanged -= OnBookPositionChanged;
                }

                _book = value;
                _renderManager.CurrentBook = _book;

                if (_book != null)
                {
                    // Subcribe 
                    _book.CurrentPositionChanged += OnBookPositionChanged;

                    // Set the (no the first "current" page).
                    _renderManager.ScreenSize = pbContent.Size;
                    CurrentScreenImage = _renderManager.Render(_book.CurrentPosition);

                    bookProgressBar.PageIncrementSize = _book.CurrentPosition.UnitSize;

                    UpdateUIState();
                }
            }
        }

        void OnBookPositionChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        #region Progress bar handling

        // Navigate to the given page
        private void bookProgressBar_MouseUp(object sender, MouseEventArgs e)
        {
            // Set position
            float pos = (float)e.X / bookProgressBar.Width;
            if (pos > 1) { pos = 1; }
            if (pos < 0) { pos = 0; }

            PositionInBook pi = PositionInBook.FromPositionUnit(pos, Book.CurrentPosition.PageCount);
            CurrentScreenImage = _renderManager.Render(pi);
        }

        #endregion

        private DW<Bitmap> CurrentScreenImage
        {
            get { return _currentScreenImage; }
            set
            {
                if (value == null) { return; }
                
                pbContent.Image = null;
                if (_currentScreenImage != null)
                {
                    _currentScreenImage.DisposeItem();
                }

                _currentScreenImage = value;

                pbContent.Image = _currentScreenImage.o;
            }
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
            if (Book == null) { return; }

            _renderManager.ScreenSize = pbContent.Size;
            CurrentScreenImage = _renderManager.Render(_book.CurrentPosition);
        }

        #region UI update
        void UpdateUIState()
        {
            bNextPage.Enabled = _renderManager.HasNextScreen;
            bPrevPage.Enabled = _renderManager.HasPreviousScreen;

            UpdateBookProgressBar();
        }

        void UpdateBookProgressBar()
        {
            if (Book == null) { return; }

            // TODO: shift progress so it's full at last page
            PositionInBook pos = Book.CurrentPosition;
            bookProgressBar.Value = pos.PositionUnit;

            lbPageNum.Text = String.Format("{0:0.0}/{1}", 
                pos.Position, pos.PageCount);
        }
        #endregion


        private void bLibrary_Click(object sender, EventArgs e)
        {
            if (GoToLibrary != null) { GoToLibrary(this, EventArgs.Empty); }
        }

        private void bNextPage_Click(object sender, EventArgs e)
        {
            CurrentScreenImage = _renderManager.RenderNext();
        }

        private void bPrevPage_Click(object sender, EventArgs e)
        {
            CurrentScreenImage = _renderManager.RenderPrevious();            
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

        private void timerCacheDisplay_Tick(object sender, EventArgs e)
        {
            // NOTE: remarkably inefficient, for debugging only

            /*
            if (_pageCache != null)
            {
                if (Book == null)
                {
                    bookProgressBar.SetLoadedPages(null, null);
                }
                else
                {
                    var memPages = _pageCache.o.GetMemoryPageNums(Book.Filename, ScreenProvider.o.ScreenSize.Width);
                    var diskPages = _pageCache.o.GetDiskPageNums(Book.Filename, ScreenProvider.o.ScreenSize.Width);
                    bookProgressBar.SetLoadedPages(memPages, diskPages);
                }

            }
             */
        }

        protected override void OnLoad(EventArgs e)
        {
            this.FindForm().FormClosed += OnUnload;
        }

        void OnUnload(object sender, FormClosedEventArgs e)
        {
            if (_contentSource != null)
            {
                _contentSource.DisposeItem();
            }
        }

    }
}
