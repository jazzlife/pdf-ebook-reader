﻿using System;
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
        DW<ScreenPageProvider> _screenPageProvider;

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

                _prefetchManager.CurrentBook = null;

                // Rendering components
                PhysicalPageProvider = new PdfPhysicalPageProvider(_book.Filename);
                IPageContentProvider contentProvider = new DefaultPageContentProvider(_pageCache); // not disposable
                ScreenProvider = DW.Wrap(new ScreenPageProvider(PhysicalPageProvider, contentProvider, pbContent.Size));

                _prefetchManager.CurrentBook = ScreenProvider;

                // TODO: render page at stored position in the book
                // (no the first "current" page).
                CurrentPageImage = ScreenProvider.o.RenderCurrentPage(pbContent.Size);
                UpdateUIState();

                bookProgressBar.PageIncrementSize = 1.0f / PhysicalPageProvider.PageCount;
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

            CurrentPageImage = ScreenProvider.o.RenderPage(pos);
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

        private DW<ScreenPageProvider> ScreenProvider
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
            float posF = ScreenProvider.o.Position;
            int pageCount = ScreenProvider.o.PhysicalPageProvider.PageCount;
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