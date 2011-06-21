using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using PdfBookReader.Render.Cache;
using PdfBookReader.Utils;
using NLog;

namespace PdfBookReader.Render
{
    class CachedPageSource : IPageSource
    {
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        // Cache
        internal readonly DW<PageCache> Cache;
        readonly PrefetchManager PrefetchManager;
        readonly IPageSource PhysicalSource;

        object MyLock = new object();

        public CachedPageSource(IPageCacheContextManager contextManager)
        {
            PhysicalSource = new SimplePageSource();
            LayoutStrategy = RenderFactory.ConcreteFactory.GetLayoutStrategy();

            Cache = DW.Wrap(new PageCache(contextManager));
            PrefetchManager = new PrefetchManager(Cache, PhysicalSource, contextManager);
            PrefetchManager.Start();

        }

        public IPageLayoutStrategy LayoutStrategy 
        { 
            get { return PhysicalSource.LayoutStrategy; }
            set { PhysicalSource.LayoutStrategy = value; } 
        }

        public Page GetPage(int pageNum, Size screenSize, ScreenBook screenBook)
        {
            // Try to get from cache
            PageKey key = new PageKey(screenBook.Book.Id, pageNum, screenSize.Width);
            Page page = Cache.o.Get(key);
            if (page != null) 
            {
                return page; 
            }

            // Render and add to cache
            lock (PrefetchManager)
            {
                page = PhysicalSource.GetPage(pageNum, screenSize, screenBook);
            }

            Cache.o.Add(key, page);
            return page;
        }

        public void Dispose()
        {
            PrefetchManager.Stop();

            Cache.o.SaveCache();
            Cache.o.Dispose();
        }
    }
}
