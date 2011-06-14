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

        IPhysicalPageProvider _physicalPageProvider;
        ScreenPageProvider _screenPageProvider;

        PrefetchManager _prefetchManager;

        Bitmap _currentScreenImage;
        PageContentCache _pageCache;

        public ReadingPanel()
        {
            InitializeComponent();

            _pageCache = new PageContentCache();
            //_pageCache.PageCached += OnPageCached;

            _prefetchManager = new PrefetchManager(null, _pageCache);
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

                // Rendering components
                PhysicalPageProvider = new PdfPhysicalPageProvider(_book.Filename);
                IPageContentProvider contentProvider = new DefaultPageContentProvider(_pageCache); // not disposable
                ScreenProvider = new ScreenPageProvider(PhysicalPageProvider, contentProvider, pbContent.Size);

                _prefetchManager.CurrentBook = ScreenProvider;

                // TODO: render page at stored position in the book
                // (no the first "current" page).
                CurrentPageImage = ScreenProvider.RenderCurrentPage(pbContent.Size);
                UpdateUIState();
                FillProgressBarWithCacheInfo();
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

            CurrentPageImage = ScreenProvider.RenderPage(pos);

            FillProgressBarWithCacheInfo();

            UpdateUIState();
        }

        // Display progress at start
        void FillProgressBarWithCacheInfo()
        {
            /*
            if (PhysicalPageProvider == null || _pageCache == null) { return; }

            bookProgressBar.ClearLoadedPages();
            bookProgressBar.PageIncrementSize = 1.0f / PhysicalPageProvider.PageCount;

            List<int> cachedPages = new List<int>();
            for(int pageCount = 0; pageCount <= PhysicalPageProvider.PageCount; pageCount++)
            {
                if (_pageCache.ContainsPage(PhysicalPageProvider.FullPath, pageCount, ScreenProvider.ScreenSize.Width))
                {
                    cachedPages.Add(pageCount);
                }
            }
            bookProgressBar.AddLoadedPages(cachedPages);
             */
        }

        #endregion

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

            FillProgressBarWithCacheInfo();
            UpdateUIState();
        }

        #region UI update
        void UpdateUIState()
        {
            if (ScreenProvider == null) { return; }

            bNextPage.Enabled = ScreenProvider.HasNextPage();
            bPrevPage.Enabled = ScreenProvider.HasPreviousPage();

            UpdateBookProgressBar();
        }

        void UpdateBookProgressBar()
        {
            float posF = ScreenProvider.Position;
            int pageCount = ScreenProvider.PhysicalPageProvider.PageCount;
            bookProgressBar.Value = posF;
            lbPageNum.Text = String.Format("{0:0.0}/{1}", 1 + (posF * pageCount), pageCount);
        }
        #endregion


        private void bLibrary_Click(object sender, EventArgs e)
        {
            if (GoToLibrary != null) { GoToLibrary(this, EventArgs.Empty); }
        }

        private void bNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageImage = ScreenProvider.RenderNextPage();
            
            UpdateUIState();
        }

        private void bPrevPage_Click(object sender, EventArgs e)
        {
            CurrentPageImage = ScreenProvider.RenderPreviousPage();            

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


    }
}
