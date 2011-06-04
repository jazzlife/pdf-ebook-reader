﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace PdfBookReader.Render
{
    // DRAFT, REVISE HEAVILY
    class PrefetchManager
    {
        // TODO: should be a singleton
        readonly PageContentCache Cache;

        ScreenPageProvider _currentBook;

        Thread _prefetchThread;
        bool _stopLoop = false;

        readonly object MyLock = new object();

        public PrefetchManager(ScreenPageProvider book, PageContentCache cache)
        {
            Cache = cache;
        }

        public ScreenPageProvider CurrentBook
        {
            get { return _currentBook; }
            set
            {
                lock (MyLock)
                {
                    if (_currentBook == value) { return; }

                    if (_currentBook != null)
                    {
                        _currentBook.PositionChanged -= OnPositionChanged;
                    }

                    _currentBook = value;

                    if (_currentBook != null)
                    {
                        _currentBook.PositionChanged += OnPositionChanged;
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
                    return (int)(CurrentBook.Position * CurrentBook.PhysicalPageProvider.PageCount) + 1;
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
                    Thread.Sleep(2000);

                    // TODO: render the start of other books if this one
                    // is done
                }
            }
        }

        // Returns true if all pages done, false otherwise
        bool PrefetchStartingAtCurrentPage()
        {
            ScreenPageProvider currentBook = CurrentBook;
            if (currentBook == null) { return true; }

            int currentPageNum = PhysicalPageNum;
            int pageCount = CurrentBook.PhysicalPageProvider.PageCount;
            Size currentScreenSize = CurrentBook.ScreenSize;

            // Next page until end
            for (int pageNume = currentPageNum; pageNume <= pageCount; pageNume++)
            {
                PrefetchPage(currentBook, pageNume);

                // Quite if current page changed
                if (ShouldRestartFetch(currentBook, currentPageNum, currentScreenSize)) { return false; }
            }

            // Fill back from start
            for (int pageNume = 1; pageNume <= currentPageNum; pageNume++)
            {
                PrefetchPage(currentBook, pageNume);

                // Quite if current page changed
                if (ShouldRestartFetch(currentBook, currentPageNum, currentScreenSize)) { return false; }
            }

            // TODO: maybe fill a bit backwards from current (after filling sufficiently forward).

            return true;
        }

        bool ShouldRestartFetch(ScreenPageProvider currentBook, int currentPageNum, Size currentSize)
        {
            if (currentBook != CurrentBook) { return true; }
            if (currentPageNum != PhysicalPageNum) { return true; }
            if (currentSize.Width != CurrentBook.ScreenSize.Width) { return true; }

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

        void PrefetchPage(ScreenPageProvider screenProvider, int physicalPageNum)
        {
            lock (MyLock)
            {
                if (physicalPageNum < 1 || physicalPageNum > screenProvider.PhysicalPageProvider.PageCount)
                {
                    return;
                }

                if (Cache.ContainsPage(
                        CurrentBook.PhysicalPageProvider.FullPath,
                        physicalPageNum,
                        CurrentBook.ScreenSize.Width))
                {
                    return;
                }

                CurrentBook.ContentProvider.RenderPhysicalPage(
                    physicalPageNum,
                    CurrentBook.ScreenSize,
                    CurrentBook.PhysicalPageProvider);
            }
        }

    }
}