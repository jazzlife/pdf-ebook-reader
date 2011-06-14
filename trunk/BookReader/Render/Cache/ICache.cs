using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PdfBookReader.Render.Cache
{
    public interface ICache<TKey, TValue>
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
        /// Update item priority, if possible. 
        /// If item is not present in cache, does nothing (does not fetch it).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newPriority"></param>
        void UpdatePriority(TKey key, ItemRetainPriority newPriority);
    }

    public static class CacheUtils
    {
        readonly static object MyStaticLock = new object();

        static String _cacheFolderPath;
        public static String CacheFolderPath
        {

            get
            {
                lock (MyStaticLock)
                {
                    if (_cacheFolderPath == null)
                    {
                        // For testing
                        String basePath = @"E:\temp";
                        if (!Directory.Exists(basePath)) 
                        {
                            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        }

                        String dirName = Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "-cache";
                        _cacheFolderPath = Path.Combine(basePath, dirName);
                    }
                    if (!Directory.Exists(_cacheFolderPath)) { Directory.CreateDirectory(_cacheFolderPath); }
                    return _cacheFolderPath;
                }
            }
        }
    }
}
