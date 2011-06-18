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
    class PageContentCache : ICache<string, PageContent>
    {
        readonly static Logger Log = LogManager.GetCurrentClassLogger();

        readonly object MyLock = new object();

        PageContentDiskCache _diskCache;
        PageContentMemoryCache _memoryCache;

        // Unnecessary, saved within the book
        [Obsolete]
        Dictionary<string, Guid> _bookIds = new Dictionary<string,Guid>();

        public PageContentCache(string filePrefix = "page-", string fileExtension = "png")
        {
            _memoryCache = new PageContentMemoryCache();
            _diskCache = new PageContentDiskCache(filePrefix, fileExtension);

            _bookIds = XmlHelper.DeserializeOrDefault(BookIdsFilename, new Dictionary<string, Guid>());
        }

        public bool Contains(string key)
        {
            lock (MyLock)
            {
                if (_memoryCache.Contains(key)) { return true; }
                if (_diskCache.Contains(key)) { return true; }
                return false;
            }
        }

        public bool Contains(String fullFilePath, int pageNum, int width)
        {
            lock (MyLock)
            {
                return Contains(GetKey(fullFilePath, pageNum, width));
            }
        }

        public bool MemoryCacheContains(string key)
        {
            lock (MyLock)
            {
                return _memoryCache.Contains(key);
            }
        }
        public bool MemoryCacheContains(String fullFilePath, int pageNum, int width)
        {
            lock (MyLock)
            {
                return MemoryCacheContains(GetKey(fullFilePath, pageNum, width));
            }
        }

        public void Add(string key, PageContent value)
        {
            lock (MyLock)
            {
                // Add to both memory and disk
                _memoryCache.Add(key, value);
                _diskCache.Add(key, value);
            }

            // Removing exipired items handled within lower-level caches
            // Memory cache eventually removes each item from memory
            // Disk cache removes files from disk
        }

        public void Add(String fullFilePath, int pageNum, int width, PageContent value)
        {
            lock (MyLock)
            {
                Add(GetKey(fullFilePath, pageNum, width), value);
            }
        }

        public PageContent Get(string key)
        {
            lock (MyLock)
            {
                PageContent item = _memoryCache.Get(key);
                if (item != null)
                {
                    Log.Debug("Get: Page in memory cache: " + key);
                    return item;
                }

                item = _diskCache.Get(key);
                if (item != null)
                {
                    Log.Debug("Get: Page in disk cache: " + key);
                    _memoryCache.Add(key, item);
                    return item;
                }

                return null;
            }
        }

        public PageContent Get(String fullFilePath, int pageNum, int width)
        {
            lock (MyLock)
            {
                return Get(GetKey(fullFilePath, pageNum, width));
            }
        }

        public void UpdatePriority(string key, ItemRetainPriority newPriority)
        {
            lock (MyLock)
            {
                _memoryCache.UpdatePriority(key, newPriority);
                _diskCache.UpdatePriority(key, newPriority);
            }
        }

        public void UpdatePriority(String fullFilePath, int pageNum, int width, ItemRetainPriority newPriority)
        {
            lock (MyLock)
            {
                UpdatePriority(GetKey(fullFilePath, pageNum, width), newPriority);
            }
        }

        public void SaveCache()
        {
            lock (MyLock)
            {
                _memoryCache.SaveCache();
                _diskCache.SaveCache();
                XmlHelper.Serialize(_bookIds, BookIdsFilename);
            }
        }

        /// <summary>
        /// Get the cache lookup key based on item information.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="pageNum"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        string GetKey(String fullFilePath, int pageNum, int width)
        {
            return width + "_" + GetBookId(fullFilePath) + "_p" + pageNum;
        }


        string BookIdsFilename { get { return Path.Combine(CacheUtils.CacheFolderPath, "BookIds.xml"); } }

        Guid GetBookId(String filename)
        {
            Guid id;
            if (!_bookIds.TryGetValue(filename, out id))
            {
                id = Guid.NewGuid();
                _bookIds.Add(filename, id);
            }
            return id;
        }

        // Test
        #region Debug / Test

        public IEnumerable<string> GetAllKeys() { throw new NotImplementedException("Not implemented"); }

        int GetPageNum(String key) 
        { 
            return int.Parse(key.Substring(key.LastIndexOf("_p") + 2)); 
        }
        
        public IEnumerable<int> GetMemoryPageNums(String fullFilePath, int width)
        {
            lock (MyLock)
            {
                String item = width + "_" + GetBookId(fullFilePath);

                return _memoryCache.GetAllKeys()
                    .Where(x => x.StartsWith(item))
                    .Select(x => GetPageNum(x)); 
            }
        }
        public IEnumerable<int> GetDiskPageNums(String fullFilePath, int width)
        {
            lock (MyLock)
            {
                String item = width + "_" + GetBookId(fullFilePath);

                return _diskCache.GetAllKeys()
                    .Where(x => x.StartsWith(item))
                    .Select(x => GetPageNum(x));
            }
        }

        #endregion


        public void Dispose()
        {
            if (_memoryCache == null) { return; }

            _memoryCache.Dispose();
            _diskCache.Dispose();

            _memoryCache = null;
            _diskCache = null;
        }
    }
}
