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
    /// Caches items in one xml file and infos (expiration data) in another.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>    
    class SimpleCache<TKey, TValue> : ICache<TKey, TValue> where TValue : class
    {
        public readonly String Name;

        Dictionary<TKey, CachedItemInfo> _cacheInfos;
        Dictionary<TKey, TValue> _cache;
        
        IExpirationPolicy _expirationPolicy;

        public SimpleCache(String name, IExpirationPolicy expirationPolicy)
        {
            ArgCheck.NotEmpty(name, "name");
            ArgCheck.NotNull(expirationPolicy, "expirationPolicy");

            Name = name;
            _expirationPolicy = expirationPolicy;

            _cache = LoadItems();
            _cacheInfos = LoadCacheInfos();
        }

        #region Load/Save 

        string CacheInfosFileName
        {
            get
            {
                return Path.Combine(CacheUtils.CacheFolderPath, Name + "CacheInfos.xml");
            }
        }

        string ItemsFileName
        {
            get
            {
                return Path.Combine(CacheUtils.CacheFolderPath, Name + "Items.xml");
            }
        }

        protected virtual Dictionary<TKey, CachedItemInfo> LoadCacheInfos()
        {
            Dictionary<TKey, CachedItemInfo> infos;

            if (_expirationPolicy is NoExpirationPolicy ||
                !File.Exists(CacheInfosFileName))
            {
                infos = new Dictionary<TKey, CachedItemInfo>();
            }
            else
            {
                // TODO: try/catch around file access
                infos = XmlHelper.Deserialize<Dictionary<TKey, CachedItemInfo>>(CacheInfosFileName);
            }

            // Special case: no items stored
            if (_cache == null) { return infos; }

            // Only keep cached infos corresponding to existing items,
            // create new infos as necessary
            Dictionary<TKey, CachedItemInfo> infosHavingItems = new Dictionary<TKey,CachedItemInfo>();

            foreach (TKey key in _cache.Keys)
            {
                // Create info if it doesn't exist
                if (!infos.ContainsKey(key))
                {
                    CachedItemInfo ci = new CachedItemInfo();
                    ci.LastAccessTime = DateTime.MinValue;
                    infos.Add(key, ci);
                }

                // Add to the real info collection
                infosHavingItems .Add(key, infos[key]);
            }

            return infosHavingItems;
        }

        protected virtual Dictionary<TKey, TValue> LoadItems()
        {
            if (File.Exists(ItemsFileName))
            {
                return XmlHelper.Deserialize<Dictionary<TKey, TValue>>(ItemsFileName);
            }
            else
            {
                return new Dictionary<TKey, TValue>();
            }
        }

        public virtual void SaveCache()
        {
            SaveCacheInfos();
            if (_cache != null) { SaveItems(); }
        }

        protected virtual void SaveCacheInfos()
        {
            // Optimization: no need to save
            if (_expirationPolicy is NoExpirationPolicy) { return; }


            XmlHelper.Serialize(_cacheInfos, CacheInfosFileName);
        }
        protected virtual void SaveItems()
        {
            XmlHelper.Serialize(_cache, CacheInfosFileName);
        }

        #endregion

        #region Item access

        public virtual bool Contains(TKey key)
        {
            return _cacheInfos.ContainsKey(key);
        }

        public virtual void Add(TKey key, TValue value)
        {
            // if it exists
            _cacheInfos.Remove(key); 
            _cache.Remove(key);

            CachedItemInfo itemInfo = new CachedItemInfo();
            itemInfo.LastAccessTime = DateTime.Now;
            _cacheInfos.Add(key, itemInfo);

            _cache.Add(key, value);

            // Remove old items
            if (_expirationPolicy.ShouldCheck(_cacheInfos))
            {
                var keys = _expirationPolicy.GetExpiredItemKeys<TKey, CachedItemInfo>(_cacheInfos);

                foreach (TKey expKey in keys)
                {
                    Remove(expKey);
                }
            }
        }

        public void Remove(TKey key)
        {
            if (CanRemoveItem(key))
            {
                RemoveItem(key);
                _cacheInfos.Remove(key);
            }
        }

        protected virtual bool CanRemoveItem(TKey key) { return true; }

        protected virtual void RemoveItem(TKey key)
        {
            if (_cache == null) { return; }

            TValue item = null;
            if (_cache != null)
            {
                _cache.TryGetValue(key, out item);
                _cache.Remove(key);
            }
        }

        public virtual TValue Get(TKey key)
        {
            TValue item = null;
            if (_cache != null)
            {
                if (_cache.TryGetValue(key, out item))
                {
                    // Update access info. 
                    // NOTE: Entry *must* exist, or there is a bug
                    CachedItemInfo info = _cacheInfos[key];
                    info.LastAccessTime = DateTime.Now;
                    _cacheInfos[key] = info;
                }
            }
            return item;
        }

        protected TValue GetItemNoInfoUpdate(TKey key)
        {
            return _cache[key];
        }

        #endregion

        public void UpdatePriority(TKey key, ItemRetainPriority newPriority)
        {
            CachedItemInfo info;
            if (_cacheInfos.TryGetValue(key, out info))
            {
                info.Priority = newPriority;
                _cacheInfos[key] = info;                
            }
        }
    }

}
