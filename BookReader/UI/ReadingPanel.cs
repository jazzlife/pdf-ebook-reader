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
using PdfBookReader.Render.Cache;

namespace PdfBookReader.UI
{
    public partial class ReadingPanel : UserControl
    {
        Book _book;

        IBookPageProvider _bookPageProvider;
        DW<ScreenProvider> _screenPageProvider;

        PrefetchManager _prefetchManager;

        DW<Bitmap> _currentScreenImage;
        DW<PageContentCache> _pageCache;

        public ReadingPanel()
        {
            InitializeComponent();

            _pageCache = DW.Wrap(new PageContentCache());
            //_pageCache.PageCached += OnPageCached;

            _prefetchManager = new PrefetchManager(_pageCache);
            _prefetchManager.Start();
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

                _prefetchManager.ScreenProvider = null;

                // Rendering components
                BookPageProvider = new PdfBookPageProvider(_book.Filename);
                IPageContentProvider contentProvider = new DefaultPageContentProvider(_pageCache); // not disposable
                ScreenProvider = DW.Wrap(new ScreenProvider(BookPageProvider, contentProvider, pbContent.Size));

                _prefetchManager.ScreenProvider = ScreenProvider;

                // TODO: render page at stored position in the book
                // (no the first "current" page).
                CurrentPageImage = ScreenProvider.o.RenderCurrentPage(pbContent.Size);

                bookProgressBar.PageIncrementSize = ScreenProvider.o.CurrentPosition.UnitSize;
                UpdateUIState();
            }
        }

        #region Progress bar handling

        // Navigate to the given page
        private void bookProgressBar_MouseUp(object sender, MouseEventArgs e)
        {
            // Set position
            float pos = (float)e.X / bookProgressBar.Width;
            if (pos > 1) { pos = 1; }
            if (pos < 0) { pos = 0; }

            PositionInBook pi = PositionInBook.FromPositionUnit(pos, ScreenProvider.o.PageProvider.PageCount);
            CurrentPageImage = ScreenProvider.o.RenderPage(pi);
            UpdateUIState();
        }

        #endregion

        private DW<Bitmap> CurrentPageImage
        {
            get { return _currentScreenImage; }
            set
            {
                if (value == null) { return; }
                
                pbContent.Image = null;
                value.AssignNewDisposeOld(ref _currentScreenImage);
                pbContent.Image = _currentScreenImage.o;
            }
        }

        private DW<ScreenProvider> ScreenProvider
        {
            get { return _screenPageProvider; }
            set { value.AssignNewDisposeOld(ref _screenPageProvider); }
        }

        private IBookPageProvider BookPageProvider
        {
            get { return _bookPageProvider; }
            set { value.AssignNewDisposeOld(ref _bookPageProvider); }
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

            CurrentPageImage = ScreenProvider.o.RenderCurrentPage(pbContent.Size);

            UpdateUIState();
        }

        #region UI update
        void UpdateUIState()
        {
            if (ScreenProvider == null) { return; }

            bNextPage.Enabled = ScreenProvider.o.HasNextPage();
            bPrevPage.Enabled = ScreenProvider.o.HasPreviousPage();

            UpdateBookProgressBar();
        }

        void UpdateBookProgressBar()
        {
            // TODO: shift progress so it's full at last page
            PositionInBook pos = ScreenProvider.o.CurrentPosition;
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
            CurrentPageImage = ScreenProvider.o.RenderNextPage();
            
            UpdateUIState();
        }

        private void bPrevPage_Click(object sender, EventArgs e)
        {
            CurrentPageImage = ScreenProvider.o.RenderPreviousPage();            

            UpdateUIState();
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
        }


    }
}
