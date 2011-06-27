using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;
using System.Drawing;
using System.IO;

namespace BookReader.Render.Cache
{
    class PageImageCache : SimpleCache<PageKey, PageImage, PageCacheContext>        
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        readonly object MyLock = new object();

        public readonly string Prefix = "page";
        public readonly string Extension = "png";

        Dictionary<PageKey, PageImage> _memoryBuffer = new Dictionary<PageKey, PageImage>();

        public PageImageCache(IPageCacheContextManager contextManager)
            : base("PageCache", contextManager,
                   RenderFactory.Default.GetPageCachePolicy())
        { }

        protected override Dictionary<PageKey, PageImage> LoadItems()
        {
            var dict = new Dictionary<PageKey, PageImage>();

            var files = Directory.GetFiles(AppPaths.CacheFolderPath, Prefix + "*." + Extension);

            foreach (String filename in files)
            {
                PageKey key = KeyFromFilename(Path.GetFileName(filename));
                
                // Only store keys, actaual items are null
                if (key != null) { dict.Add(key, null); }
            }

            return dict;
        }

        protected override void SaveItems()
        {
            // Do nothing, files are already saved
        }


        public override bool Contains(PageKey key)
        {
            lock (MyLock)
            {
                return base.Contains(key);
            }
        }

        public override void Add(PageKey key, PageImage value)
        {
            lock (MyLock)
            {
                ArgCheck.NotNull(key, "key");
                ArgCheck.NotNull(key, "value");
                ArgCheck.Is(value.InUse, "page not in use");

                if (Contains(key)) 
                {
                    throw new InvalidOperationException("Add: already contains key: " + key);
                }

                String filename = GetFullPath(key);
                value.Image.Save(filename);

                base.Add(key, null);
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
            PageImage item;
            if (!_memoryBuffer.TryGetValue(key, out item)) { return; }

            if (!item.InUse)
            {
                item.DisposeItem();
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
        
        public override PageImage Get(PageKey key)
        {
            lock (MyLock)
            {
                // Try getting from memory
                if (_memoryBuffer.ContainsKey(key))
                {
                    PageImage memImage = _memoryBuffer[key];
                    memImage.Reuse();
                    return memImage;
                }
                else
                {
                    // Try getting from disk
                    String filename = GetFullPath(key);
                    if (!File.Exists(filename)) { return null; }

                    // TODO: try/catch around file access
                    // Note: new Bitmap(filename) locks the file
                    PageImage diskImage;
                    using (var fs = new FileStream(filename, FileMode.Open))
                    {
                        diskImage = new PageImage(key, new Bitmap(fs));
                    }

                    // Add to memory, to be disposed later
                    if (!diskImage.InUse) { throw new ApplicationException("BUG: page not in use"); }
                    _memoryBuffer.Add(key, diskImage);

                    return diskImage;
                }
            }
        }

        string GetFullPath(PageKey key)
        {
            return Path.Combine(AppPaths.CacheFolderPath, FilenameFromKey(key));
        }

        string FilenameFromKey(PageKey key)
        {
            return "{0}_{1}_p{2}_w{3}.{4}".F(Prefix, key.BookId, key.PageNum, key.ScreenWidth, Extension);
        }
        PageKey KeyFromFilename(string filename)
        {
            String[] parts = filename.Split('_','.');

            try 
            {
                Guid id = new Guid(parts[1]);
                int pageNum = int.Parse(parts[2].Substring(1));
                int screenWidth = int.Parse(parts[3].Substring(1));

                return new PageKey(id, pageNum, screenWidth);
            }
            catch(Exception e)
            {
                logger.Debug("Cannot get key from filename: " + filename + " " + e.Message);
                return null;
            }
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
