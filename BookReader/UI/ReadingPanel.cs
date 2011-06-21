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

        DW<ScreenRenderManager> _renderManager;
        DW<Bitmap> _currentScreenImage;

        public ReadingPanel()
        {
            InitializeComponent();

            this.Disposed += new EventHandler(ReadingPanel_Disposed);
        }

        public void Initialize(BookLibrary library)
        {
            ArgCheck.NotNull(library, "library");
            _library = library;

            _renderManager = DW.Wrap(new ScreenRenderManager(_library, pbContent.Size));
        }


        void ReadingPanel_Disposed(object sender, EventArgs e)
        {
            if (_renderManager != null)
            {
                _renderManager.DisposeItem();
            }
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
                _renderManager.o.CurrentBook = _book;

                if (_book != null)
                {
                    // Subcribe 
                    _book.CurrentPositionChanged += OnBookPositionChanged;

                    // Set the (no the first "current" page).
                    _renderManager.o.ScreenSize = pbContent.Size;
                    CurrentScreenImage = _renderManager.o.Render(_book.CurrentPosition);

                    bookProgressBar.PageIncrementSize = _book.CurrentPosition.UnitSize;
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
            CurrentScreenImage = _renderManager.o.Render(pi);
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

            _renderManager.o.ScreenSize = pbContent.Size;
            CurrentScreenImage = _renderManager.o.Render(_book.CurrentPosition);
        }

        #region UI update
        void UpdateUIState()
        {
            bNextPage.Enabled = _renderManager.o.HasNextScreen;
            bPrevPage.Enabled = _renderManager.o.HasPreviousScreen;

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
            CurrentScreenImage = _renderManager.o.RenderNext();
        }

        private void bPrevPage_Click(object sender, EventArgs e)
        {
            CurrentScreenImage = _renderManager.o.RenderPrevious();            
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
            if (Book == null) { return; }

            // NOTE: remarkably inefficient, and breaking compatibility -- for debugging only

            PageCache cache = _renderManager.o.Cache.o;
            if (cache != null)
            {
                var diskPages = cache.GetAllKeys();

                this.FindForm().Text = 
                    "Cache:{0} Mem:{1} Bitmaps(A:{2} D:{3})".F(
                    diskPages.Count(), cache.MemoryItemCount, DW<Bitmap>.Active, " D:" + DW<Bitmap>.Disposed);

                bookProgressBar.SetLoadedPages(diskPages.Where(x => x.BookId == Book.Id).Select(x => x.PageNum));
            }
        }


    }
}
