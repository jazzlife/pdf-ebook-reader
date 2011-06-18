﻿using System;
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

        DW<IBookPageProvider> _bookPageProvider;
        DW<ScreenProvider> _screenPageProvider;
        DW<Bitmap> _currentScreenImage;

        // read-only
        DW<IPageContentSource> _contentSource;

        public ReadingPanel()
        {
            InitializeComponent();
        }

        public void Initialize(BookLibrary library)
        {
            ArgCheck.NotNull(library, "library");
            _library = library;

            _contentSource = DW.Wrap<IPageContentSource>(new CachedPageContentSource());
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

                if (_book != null)
                {
                    // Rendering components
                    BookPageProvider = DW.Wrap<IBookPageProvider>(new PdfBookPageProvider(_book.Filename));
                    ScreenProvider = DW.Wrap(new ScreenProvider(BookPageProvider, _contentSource, pbContent.Size));

                    // Subcribe 
                    _book.CurrentPositionChanged += OnBookPositionChanged;

                    // Initialize position if unknown
                    if (_book.CurrentPosition == null)
                    {
                        _book.CurrentPosition = PositionInBook.FromPhysicalPage(1, BookPageProvider.o.PageCount);
                    }

                    // Set the (no the first "current" page).
                    CurrentScreenImage = ScreenProvider.o.RenderPage(_book.CurrentPosition);

                    bookProgressBar.PageIncrementSize = ScreenProvider.o.CurrentPosition.UnitSize;

                    UpdateUIState();
                }
                else
                {
                    ScreenProvider = null;
                    BookPageProvider = null;
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
            CurrentScreenImage = ScreenProvider.o.RenderPage(pi);
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

        private DW<ScreenProvider> ScreenProvider
        {
            get { return _screenPageProvider; }
            set 
            {
                if (_screenPageProvider != null)
                {
                    _screenPageProvider.o.PositionChanged -= o_PositionChanged;
                    _screenPageProvider.DisposeItem();
                }

                _screenPageProvider = value;

                if (_screenPageProvider != null)
                {
                    _screenPageProvider.o.PositionChanged += o_PositionChanged;
                }
            }
        }

        void o_PositionChanged(object sender, EventArgs e)
        {
            // IMPORTANT: Screen provider event updats Book.CurrentPosition.
            // (since we don't want to pass book inside it). 

            // However, UI elements are driven by BOOK position change 
            // event, not one from the provider.

            if (Book != null &&
                Book.Filename == ScreenProvider.o.PageProvider.o.BookFilename)
            {
                Book.CurrentPosition = ScreenProvider.o.CurrentPosition;
            }
        }

        private DW<IBookPageProvider> BookPageProvider
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

            CurrentScreenImage = ScreenProvider.o.RenderCurrentScreen(pbContent.Size);
        }

        #region UI update
        void UpdateUIState()
        {
            if (ScreenProvider == null) { return; }

            bNextPage.Enabled = ScreenProvider.o.HasNextScreen;
            bPrevPage.Enabled = ScreenProvider.o.HasPreviousScreen;

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
            CurrentScreenImage = ScreenProvider.o.RenderNextScreen();
        }

        private void bPrevPage_Click(object sender, EventArgs e)
        {
            CurrentScreenImage = ScreenProvider.o.RenderPreviousScreen();            
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
