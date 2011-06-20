﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using PdfBookReader.Render.Cache;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    class PrefetchManager 
    {
        readonly object MyLock = new object();
        readonly DW<PageCache> Cache;

        public PrefetchManager(DW<PageCache> cache)
        {
            Cache = cache;
        }

        public void Start() { }
        public void Stop() { }
    }

    /*
    // DRAFT, REVISE HEAVILY
    class PrefetchManager
    {
        // TODO: should be a singleton
        readonly DW<PageContentCache> Cache;

        DW<ScreenBook> _screenProvider;

        Thread _prefetchThread;
        bool _stopLoop = false;

        readonly object MyLock = new object();

        public PrefetchManager(DW<PageContentCache> cache)
        {
            Cache = cache;
        }

        AutoResetEvent _currentBookDoneWait = new AutoResetEvent(false);

        public DW<ScreenBook> ScreenProvider
        {
            get { return _screenProvider; }
            set
            {
                lock (MyLock)
                {
                    if (_screenProvider == value) { return; }

                    if (_screenProvider != null)
                    {
                        _screenProvider.o.PositionChanged -= OnPositionChanged;
                    }

                    _screenProvider = value;

                    if (_screenProvider != null)
                    {
                        _screenProvider.o.PositionChanged += OnPositionChanged;
                        OnPositionChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        void OnPositionChanged(object sender, EventArgs e)
        {
            _currentBookDoneWait.Set();
        }

        void DoLoop()
        {
            while (!_stopLoop)
            {

                if (PrefetchStartingAtCurrentPage())
                {
                    _currentBookDoneWait.WaitOne();
                }
            }
        }

        // Returns true if all pages done, false otherwise
        bool PrefetchStartingAtCurrentPage()
        {
            DW<ScreenBook> currentBook = ScreenProvider;
            if (currentBook == null) { return true; }

            int currentPageNum = ScreenProvider.o.CurrentPosition.PageNum;
            int pageCount = ScreenProvider.o.PageProvider.o.PageCount;
            Size currentScreenSize = ScreenProvider.o.ScreenSize;

            foreach(int pageNum in GetPrefretchPageNumbers(currentPageNum, pageCount))
            {
                // Quit if current page changed
                if (ShouldRestartFetch(currentBook, currentPageNum, currentScreenSize)) { return false; }

                PrefetchPage(currentBook, pageNum);
            }
            return true;
        }

        /// <summary>
        /// Get the order in which to prefetch
        /// </summary>
        /// <param name="currentPageNum"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        IEnumerable<int> GetPrefretchPageNumbers(int currentPageNum, int pageCount)
        {
            // current page is fetched by normal channels

            const int PrefetchForward = 30;
            const int PrefetchBack = 10;

            int forwardMax = Math.Min(currentPageNum + PrefetchForward, pageCount);
            int backwardMin = Math.Max(currentPageNum - PrefetchBack, 1);

            int forwardNum = currentPageNum + 1;
            int backwardNum = currentPageNum - 1;

            // Two steps forward, one step back
            while (true)
            {
                if (forwardNum <= forwardMax) { yield return forwardNum; }
                ++forwardNum;
                if (forwardNum <= forwardMax) { yield return forwardNum; }
                ++forwardNum;

                if (backwardNum >= backwardMin) { yield return backwardNum; }
                --backwardNum;

                if (backwardMin > backwardNum && forwardNum > forwardMax) { break; }
            }
        }

        bool ShouldRestartFetch(DW<ScreenBook> currentBook, int currentPageNum, Size currentSize)
        {
            if (_stopLoop) { return true; }
            if (currentBook != ScreenProvider) { return true; }
            if (currentPageNum != ScreenProvider.o.CurrentPosition.PageNum) { return true; }
            if (currentSize.Width != ScreenProvider.o.ScreenSize.Width) { return true; }

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
            _currentBookDoneWait.Set();

            // Wait until done working with all relevant objects
            if (_prefetchThread != null) 
            { 
                _prefetchThread.Join(); 
            }
        }

        void PrefetchPage(DW<ScreenBook> screenProvider, int pageNum)
        {
            lock (MyLock)
            {
                if (pageNum < 1 || 
                    pageNum > screenProvider.o.PageProvider.o.PageCount ||
                    ScreenProvider == null || 
                    ScreenProvider.IsDisposed)
                {
                    return;
                }

                if (!Cache.o.MemoryCacheContains(
                        ScreenProvider.o.PageProvider.o.BookFilename,
                        pageNum,
                        ScreenProvider.o.ScreenSize.Width))
                {
                    PageContent pc = ScreenProvider.o.ContentProvider.o.GetPage(
                        pageNum,
                        ScreenProvider.o.ScreenSize,
                        ScreenProvider.o.PageProvider);
                    
                    // BUG: leaking memory since PC is not set to InUse
                    // and would never be disposed. Cannot return it here,
                    // as the main thead could still have this item. 

                    // Major architectural change is necessary to fix this in a nice way
                }


            }
        }

    }
     */
}