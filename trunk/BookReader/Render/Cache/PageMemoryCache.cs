using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PdfBookReader.Render.Cache
{
    // Not thread-safe, lock externally
    class PageMemoryCache : SimpleCache<string, Page>
    {
        public PageMemoryCache() :
            base("BitmapMemoryCache", new DefaultExpirationPolicy(20, 30))
        { }

        protected override Dictionary<string, Page> LoadItems()
        {
            // Always use a fresh empty collection
            return new Dictionary<string, Page>();
        }

        protected override Dictionary<string, CachedItemInfo> LoadCacheInfos()
        {
            // Always use a fresh empty collection
            return new Dictionary<string, CachedItemInfo>();
        }

        protected override void SaveCacheInfos()
        {
            // Not saving to disk
        }

        protected override void SaveItems()
        {
            // Not saving to disk
        }

        protected override bool CanRemoveItem(string key)
        {
            Page cdItem = GetItemNoInfoUpdate(key);
            // Remove item only if it is not in use
            return !cdItem.InUse;
        }

        protected override void RemoveItem(string key)
        {
            Page cdItem = GetItemNoInfoUpdate(key);
            if (cdItem.InUse) { throw new InvalidOperationException("Item in use, cannot remove: " + cdItem); }

            base.RemoveItem(key);

            // Dispose the bitmap
            cdItem.Image.DisposeItem();
        }
    }
}
