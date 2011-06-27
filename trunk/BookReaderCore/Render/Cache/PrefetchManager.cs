using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using BookReader.Render.Cache;
using BookReader.Utils;

namespace BookReader.Render
{
    class PrefetchManager : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal readonly DW<PageImageCache> Cache;
        readonly IPrefetchPolicy<PageKey, PageCacheContext> Policy;
        readonly IPageCacheContextManager ContextManager;

        Thread _prefetchThread;
        bool _stopLoop = false;
        AutoResetEvent _waitForContextChange = new AutoResetEvent(false);
        PageCacheContext _currentContext;

        public PrefetchManager(DW<PageImageCache> cache, 
            IPageCacheContextManager contextManager)
        {
            ArgCheck.NotNull(cache, "cache");
            ArgCheck.NotNull(contextManager, "contextManager");

            Cache = cache;
            ContextManager = contextManager;

            ContextManager.CacheContextChanged += OnCacheContextChanged;

            Policy = RenderFactory.Default.GetPrefetchPolicy();
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
                else
                {
                    // Short break from prefetching, there's user activity
                    Thread.Sleep(1000);
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

                if (1 <= key.PageNum && key.PageNum <= sb.BookContent.o.PageCount)
                {
                    Size size = new Size(key.ScreenWidth, int.MaxValue);
                    PageImage page;

                    lock (sb.BookContent)
                    {
                        page = sb.BookContent.o.GetPageImage(key.PageNum, size.Width);
                        page.DisposeOnReturn = false;
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
