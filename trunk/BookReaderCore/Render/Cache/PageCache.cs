using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;
using System.Drawing;
using System.IO;

namespace BookReader.Render.Cache
{
    // Not thread-safe, lock externally
    class PageCache : SimpleCache<PageKey, Page, PageCacheContext>        
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        readonly object MyLock = new object();

        public readonly string Prefix = "page";
        public readonly string Extension = "png";

        Dictionary<PageKey, Page> _memoryBuffer = new Dictionary<PageKey,Page>();

        public PageCache(IPageCacheContextManager contextManager)
            : base("PageCache", contextManager,
                   RenderFactory.ConcreteFactory.GetPageCachePolicy())
        { }

        public override bool Contains(PageKey key)
        {
            lock (MyLock)
            {
                return base.Contains(key);
            }
        }

        public override void Add(PageKey key, Page value)
        {
            lock (MyLock)
            {
                ArgCheck.NotNull(key, "key");
                ArgCheck.NotNull(key, "value");
                ArgCheck.Is(value.InUse, "page not in use");

                if (Contains(key)) 
                {
                    logger.Debug("Add: already contains key: " + key);
                    return; 
                }

                // Make a copy, we do not store the bitmap pointer.
                Page actualPc = new Page(value.PageNum, null, value.Layout);
                base.Add(key, actualPc);

                String filename = GetFullPath(key);
                value.Image.o.Save(filename);

                // Save cache xml, since we're saving the bitmap 
                //SaveCache();

                // Store to memory
                _memoryBuffer.Add(key, value);

                // Remove unused items from memory
                var keys = _memoryBuffer.Keys.ToArray();
                keys.ForEach(x => DisposeUnused(x));
            }
        }

        void DisposeUnused(PageKey key)
        {
            Page memPage;
            if (!_memoryBuffer.TryGetValue(key, out memPage)) { return; }

            if (!memPage.InUse)
            {
                memPage.Image.DisposeItem();
                _memoryBuffer.Remove(key);
            }
        }

        public override void Remove(PageKey key)
        {
            lock (MyLock)
            {
                DisposeUnused(key);

                String filename = GetFullPath(key);
                File.Delete(filename);
                base.Remove(key);
            }
        }
        
        public override Page Get(PageKey key)
        {
            lock (MyLock)
            {
                // Try getting from memory
                if (_memoryBuffer.ContainsKey(key))
                {
                    Page memPage = _memoryBuffer[key];
                    memPage.Reuse();
                    return memPage;
                }
                else
                {
                    // Try getting from disk

                    // Get info from base cache
                    Page tempPage = base.Get(key); // temporary value, no image
                    if (tempPage == null) { return null; }

                    // Load bitmap from disk
                    String filename = GetFullPath(key);
                    if (!File.Exists(filename)) { return null; }

                    // TODO: try/catch around file access
                    // Note: new Bitmap(filename) locks the file
                    DW<Bitmap> bmp;
                    using (var fs = new FileStream(filename, FileMode.Open))
                    {
                        bmp = DW.Wrap(new Bitmap(fs));
                    }

                    Page diskPage = new Page(tempPage.PageNum, bmp, tempPage.Layout);

                    // Add to memory, to be disposed later
                    if (!diskPage.InUse) { throw new ApplicationException("BUG: page not in use"); }
                    _memoryBuffer.Add(key, diskPage);

                    return diskPage;
                }
            }
        }

        string GetFullPath(PageKey key)
        {
            String filename = "{0}_{1}_p{2}_w{3}.{4}".F(Prefix, key.BookId, key.PageNum, key.ScreenWidth, Extension);
            return Path.Combine(AppPaths.CacheFolderPath, filename);
        }

        public int MemoryItemCount 
        { 
            get 
            {
                lock (MyLock)
                {
                    return _memoryBuffer.Count;
                }
            } 
        }
    }
}
