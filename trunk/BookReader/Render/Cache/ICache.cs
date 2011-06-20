using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PdfBookReader.Render.Cache
{
    public interface ICache<TKey, TValue> : IDisposable
    {
        /// <summary>
        /// True if cache contains the item.
        /// Lookup should be *quick* (in-memory).
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(TKey key);

        /// <summary>
        /// Add an item to cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Get an item, null if it doesn't exist.
        /// </summary>
        /// <param name="key"></param>
        TValue Get(TKey key);

        /// <summary>
        /// Save the internal cache information to disk.
        /// (e.g. last usage times).
        /// </summary>
        void SaveCache();

        // Optional methods

        /// <summary>
        /// For testing purposes. Returns a copy, may be slow.
        /// </summary>
        IEnumerable<TKey> GetAllKeys();
    }

    /// <summary>
    /// Cached item info, used by expiration algorithm.
    /// Should be lightweight -- for disk-based cache items 
    /// it may be kept in memory for speed.
    /// </summary>
    public struct CachedItemInfo
    {
        public DateTime LastAccessTime;
    }

}
