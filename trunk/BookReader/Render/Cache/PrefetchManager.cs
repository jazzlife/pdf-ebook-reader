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
    class PrefetchManager : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal readonly DW<PageCache> Cache;
        readonly IPageSource PhysicalSource;
        readonly IPrefetchPolicy<PageKey, PageCacheContext> Policy;
        readonly IPageCacheContextManager ContextManager;

        Thread _prefetchThread;
        bool _stopLoop = false;
        AutoResetEvent _waitForContextChange = new AutoResetEvent(false);
        PageCacheContext _currentContext;

        public PrefetchManager(DW<PageCache> cache, 
            IPageSource pageSource,
            IPageCacheContextManager contextManager)
        {
            ArgCheck.NotNull(cache, "cache");
            ArgCheck.NotNull(pageSource, "pageSource");
            ArgCheck.NotNull(contextManager, "contextManager");

            Cache = cache;
            PhysicalSource = pageSource;
            ContextManager = contextManager;

            ContextManager.CacheContextChanged += OnCacheContextChanged;

            Policy = RenderFactory.ConcreteFactory.GetPrefetchPolicy();
        }

        #region prefetch thread

        void PrefetchLoop()
        {
            while (!_stopLoop)
            {
                if (PrefetchNeededKeys())
                {
                    logger.Debug("Done with all, waiting for context change.");
                    _waitForContextChange.WaitOne();
                }
            }
        }

        // returns true if prefetch is complete, 
        // false if context changed before it was done
        bool PrefetchNeededKeys()
        {
            PageCacheContext context = _currentContext;
            if (context == null) { return true; }
            
            var pageKeys = Policy.PrefetchKeyOrder(context);
            logger.Debug("Starting to prefetch: " + pageKeys);

            foreach (var key in pageKeys)
            {
                if (Cache.o.Contains(key)) { continue; }

                // Render and add to cache
                ScreenBook sb = ContextManager.GetScreenBook(key.BookId);

                if (1 <= key.PageNum && key.PageNum <= sb.BookProvider.o.PageCount)
                {
                    Size size = new Size(key.ScreenWidth, int.MaxValue);
                    Page page;
                    lock (this)
                    {
                        page = PhysicalSource.GetPage(key.PageNum, size, sb);
                    }

                    Cache.o.Add(key, page);
                    page.Return();
                }


                if (_stopLoop) { return false; }

                if (context != _currentContext)
                {
                    logger.Debug("Context changed");
                    return false;
                }
            }

            return true;
        }

        #endregion


        void OnCacheContextChanged(object sender, EvArgs<PageCacheContext> e)
        {
            _currentContext = e.Value;
            _waitForContextChange.Set();
        }

        public void Start()
        {
            _stopLoop = false;
            if (_prefetchThread == null)
            {
                _prefetchThread = new Thread(PrefetchLoop);
                _prefetchThread.Name = "Prefetch thread";
                _prefetchThread.Start();
            }
        }

        public void Stop() 
        {
            _stopLoop = true;
            _currentContext = null;
            _waitForContextChange.Set();

            // Wait for the thread to end
            _prefetchThread.Join();
        }

        public void Dispose()
        {
            Stop();
        }
    }

}
