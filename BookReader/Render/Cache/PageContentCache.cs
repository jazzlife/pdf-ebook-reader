using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Utils;
using System.Drawing;
using System.IO;

namespace PdfBookReader.Render.Cache
{
    /// <summary>
    /// Wraps the disk and memory caches for PageContent objects.
    /// </summary>
    class PageContentCache : ICache<string, PageContent>
    {
        readonly object MyLock = new object();

        PageContentDiskCache _diskCache;
        PageContentMemoryCache _memoryCache;
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
                if (item == null)
                {
                    item = _diskCache.Get(key);
                    if (item != null)
                    {
                        _memoryCache.Add(key, item);
                    }
                }
                return item;
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

    }
}
