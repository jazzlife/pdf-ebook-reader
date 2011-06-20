using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Utils;
using System.Drawing;
using System.IO;

namespace PdfBookReader.Render.Cache
{
    // Not thread-safe, lock externally
    class PageDiskCache : SimpleCache<PageKey, Page, PageCacheContext>        
    {
        public readonly string Prefix = "page";
        public readonly string Extension = "png";

        public PageDiskCache(IPageCacheContextManager contextManager)
            : base("PageCache", contextManager,
                   RenderFactory.ConcreteFactory.GetPageCachePolicyDisk())
        { }

        public override void Add(PageKey key, Page value)
        {
            // Make a copy, we do not store the bitmap pointer.
            Page actualPc = new Page(value.PageNum, null, value.Layout);

            base.Add(key, value);

            String filename = GetFullPath(key);
            value.Image.o.Save(filename);
        }

        public override void Remove(PageKey key)
        {
            String filename = GetFullPath(key);
            File.Delete(filename);

            base.Remove(key);
        }
        
        public override Page Get(PageKey key)
        {
            Page tempPc = base.Get(key); // temporary value, no image
            if (tempPc == null) { return null; }

            String filename = GetFullPath(key);
            if (!File.Exists(filename)) { return null; }

            // TODO: try/catch around file access
            DW<Bitmap> b = DW.Wrap(new Bitmap(filename));

            return new Page(tempPc.PageNum, b, tempPc.Layout);
        }

        string GetFullPath(PageKey key)
        {
            String filename = "{0}_{1}_p{2}_w{3}.{4}".F(Prefix, key.BookId, key.PageNum, key.ScreenWidth, Extension);
            return Path.Combine(AppPaths.CacheFolderPath, filename);
        }
    }
}
