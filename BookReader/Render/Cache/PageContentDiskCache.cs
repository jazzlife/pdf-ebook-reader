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
    class PageContentDiskCache : SimpleCache<string, PageContent>        
    {
        public readonly string Prefix;
        public readonly string Extension;

        public PageContentDiskCache(string filePrefix = "page-", string fileExtension = "png")
            : base (filePrefix, new DefaultExpirationPolicy(600, 800) )
        {
            ArgCheck.FilenameCharsValid(filePrefix, "filePrefix");
            ArgCheck.IsNot(fileExtension.Contains('.'), "File extension should not contain a dot (.)");

            Prefix = filePrefix;
            Extension = fileExtension;
        }

        public override void Add(string key, PageContent value)
        {
            ArgCheck.FilenameCharsValid(key, "key");

            // Make a copy, we do not store the bitmap pointer.
            PageContent actualPc = new PageContent(value.PageNum, null, value.Layout);

            base.Add(key, value);

            String filename = GetFullPath(key);
            value.Image.Save(filename);
        }

        protected override void RemoveItem(string key)
        {
            // Do NOT dispose the bitmap here, that will be done by the memory cache
            // Items will always go through memory cache, it has the ultimate reposibility

            base.RemoveItem(key);

            String filename = GetFullPath(key);
            if (File.Exists(filename)) { File.Delete(filename); }
        }

        public override PageContent Get(string key)
        {
            PageContent tempPc = base.Get(key); // temporary value, no image
            if (tempPc == null) { return null; }

            // TODO: try/catch around file access
            String filename = GetFullPath(key);
            Bitmap b = new Bitmap(filename);

            return new PageContent(tempPc.PageNum, b, tempPc.Layout);
        }

        string GetFullPath(String key)
        {
            return Path.Combine(CacheUtils.CacheFolderPath, Prefix + key + "." + Extension);
        }
    }
}
