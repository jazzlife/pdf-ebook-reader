using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Utils;
using System.Drawing;
using System.IO;
using NLog;

namespace PdfBookReader.Render.Cache
{
    /// <summary>
    /// Wraps the disk and memory caches for PageContent objects.
    /// </summary>
    class PageCache : SimpleCache<PageKey, Page, PageCacheContext>
    {
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        readonly object MyLock = new object();

        PageDiskCache _diskCache;
        
        public PageCache(IPageCacheContextManager contextManager)
            : base("PageMemoryCache", contextManager, RenderFactory.ConcreteFactory.GetPageCachePolicyMemory())
        {
            _diskCache = new PageDiskCache(contextManager);
        }

        #region load/save
        protected override Dictionary<PageKey, Page> LoadItems()
        {
            // Memory cache - always use a fresh empty collection
            return new Dictionary<PageKey, Page>();
        }

        protected override Dictionary<PageKey, CachedItemInfo> LoadCacheInfos()
        {
            // Memory cache - always use a fresh empty collection
            return new Dictionary<PageKey, CachedItemInfo>();
        }

        public override void SaveCache()
        {
            // No need to call base.SaveCache, it's memory-only
            _diskCache.SaveCache();
        }
        #endregion

        public override bool Contains(PageKey key)
        {
            lock (MyLock)
            {

                if (base.Contains(key)) { return true; }
                if (_diskCache.Contains(key)) 
                {
                    // Add to memory if policy approves
                    if (RetainPolicy.MustRetain(key, ContextManager.CacheContext))
                    {
                        Page page = _diskCache.Get(key);
                        base.Add(key, page);
                    }

                    // TODO: add to memory if policy approves?
                    return true; 
                }
                return false;
            }
        }

        public override void Add(PageKey key, Page value)
        {
            lock (MyLock)
            {
                // Always add to disk
                _diskCache.Add(key, value);

                // Add to memory if policy approves
                if (value.InUse || RetainPolicy.MustRetain(key, ContextManager.CacheContext))
                {
                    base.Add(key, value);
                }
            }
        }

        public override void Remove(PageKey key)
        {
            Page item = base.Get(key);
            if (item == null) { return; }

            if (item.InUse) { return; }
            item.Image.DisposeItem();

            base.Remove(key);
        }

        public override Page Get(PageKey key)
        {
            lock (MyLock)
            {
                Page item = base.Get(key);
                if (item != null)
                {
                    logger.Debug("Get: Page in memory: " + key);
                    return item;
                }

                item = _diskCache.Get(key);
                if (item != null)
                {
                    logger.Debug("Get: Page on disk: " + key);
                    
                    // TODO: only add to memory if policy is happy
                    base.Add(key, item);
                    
                    
                    return item;
                }

                return null;
            }
        }


        string BookIdsFilename { get { return Path.Combine(AppPaths.CacheFolderPath, "BookIds.xml"); } }

        public override void Dispose()
        {
            lock (MyLock)
            {
                base.Dispose();

                _diskCache.Dispose();
                _diskCache = null;
            }
        }

        public IEnumerable<PageKey> GetMemoryKeys()
        {
            lock (MyLock)
            {
                return base.GetAllKeys().ToArray();
            }
        }

        public IEnumerable<PageKey> GetDiskKeys()
        {
            lock (MyLock)
            {
                return _diskCache.GetAllKeys().ToArray();
            }
        }

    }
}
