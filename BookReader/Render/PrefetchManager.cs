using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using PdfBookReader.Render.Cache;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    // DRAFT, REVISE HEAVILY
    class PrefetchManager
    {
        // TODO: should be a singleton
        readonly DW<PageContentCache> Cache;

        DW<ScreenPageProvider> _currentBook;

        Thread _prefetchThread;
        bool _stopLoop = false;

        readonly object MyLock = new object();

        public PrefetchManager(DW<PageContentCache> cache)
        {
            Cache = cache;
        }

        public DW<ScreenPageProvider> CurrentBook
        {
            get { return _currentBook; }
            set
            {
                lock (MyLock)
                {
                    if (_currentBook == value) { return; }

                    if (_currentBook != null)
                    {
                        _currentBook.o.PositionChanged -= OnPositionChanged;
                    }

                    _currentBook = value;

                    if (_currentBook != null)
                    {
                        _currentBook.o.PositionChanged += OnPositionChanged;
                        OnPositionChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        void OnPositionChanged(object sender, EventArgs e)
        {
            // TODO -- what exactly?
            //throw new NotImplementedException();
        }

        // Must be notified of changes in ScreenPageProvider...
        int PhysicalPageNum
        {
            get
            {
                lock (MyLock)
                {
                    return (int)(CurrentBook.o.Position * CurrentBook.o.PhysicalPageProvider.PageCount) + 1;
                }
            }
        }

        const int FetchForward = 20;
        const int FetchBack = 5;

        void DoLoop()
        {
            while (!_stopLoop)
            {

                if (PrefetchStartingAtCurrentPage())
                {
                    // TODO: something more sensible than just sleeping.
                    // e.g. Wait and pulse when book changes
                    Thread.Sleep(1000);

                    // TODO: render the start of other books if this one
                    // is done
                }
            }
        }

        // Returns true if all pages done, false otherwise
        bool PrefetchStartingAtCurrentPage()
        {
            DW<ScreenPageProvider> currentBook = CurrentBook;
            if (currentBook == null) { return true; }

            int currentPageNum = PhysicalPageNum;
            int pageCount = CurrentBook.o.PhysicalPageProvider.PageCount;
            Size currentScreenSize = CurrentBook.o.ScreenSize;

            // Next page until end
            for (int pageNum = currentPageNum; pageNum <= pageCount; pageNum++)
            {
                PrefetchPage(currentBook, pageNum);

                // Quite if current page changed
                if (ShouldRestartFetch(currentBook, currentPageNum, currentScreenSize)) { return false; }
            }

            // Fill back from start
            for (int pageNum = 1; pageNum < currentPageNum; pageNum++)
            {
                PrefetchPage(currentBook, pageNum);

                // Quite if current page changed
                if (ShouldRestartFetch(currentBook, currentPageNum, currentScreenSize)) { return false; }
            }

            // TODO: maybe fill a bit backwards from current (after filling sufficiently forward).

            return true;
        }

        bool ShouldRestartFetch(DW<ScreenPageProvider> currentBook, int currentPageNum, Size currentSize)
        {
            if (_stopLoop) { return true; }
            if (currentBook != CurrentBook) { return true; }
            if (currentPageNum != PhysicalPageNum) { return true; }
            if (currentSize.Width != CurrentBook.o.ScreenSize.Width) { return true; }

            return false;

        }

        public void Start()
        {
            _stopLoop = false;
            if (_prefetchThread == null)
            {
                _prefetchThread = new Thread(DoLoop);
                _prefetchThread.Name = "Prefetch thread";
                _prefetchThread.Start();
            }
        }

        public void Stop()
        {
            _stopLoop = true;
            
            // Wait until done working with all relevant objects
            if (_prefetchThread != null) 
            { 
                _prefetchThread.Join(); 
            }
        }

        void PrefetchPage(DW<ScreenPageProvider> screenProvider, int pageNum)
        {
            lock (MyLock)
            {
                if (pageNum < 1 || 
                    pageNum > screenProvider.o.PhysicalPageProvider.PageCount ||
                    screenProvider.IsDisposed)
                {
                    return;
                }

                if (!Cache.o.Contains(
                        CurrentBook.o.PhysicalPageProvider.FullPath,
                        pageNum,
                        CurrentBook.o.ScreenSize.Width))
                {
                    CurrentBook.o.ContentProvider.RenderPhysicalPage(
                        pageNum,
                        CurrentBook.o.ScreenSize,
                        CurrentBook.o.PhysicalPageProvider);
                }

                // set priority
                ItemRetainPriority priority = ItemRetainPriority.Normal;
                int currentPageNum = PhysicalPageNum;
                if (pageNum < 5 || 
                    (pageNum - 3 < pageNum && pageNum < currentPageNum + 5))
                { 
                    priority = ItemRetainPriority.AlwaysRetain; 
                }

                Cache.o.UpdatePriority(
                        CurrentBook.o.PhysicalPageProvider.FullPath,
                        pageNum,
                        CurrentBook.o.ScreenSize.Width, priority);


            }
        }

    }
}
