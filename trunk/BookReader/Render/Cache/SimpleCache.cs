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
    /// Caches items in one xml file and infos in another.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>    
    class SimpleCache<TKey, TValue, TContext> : ICache<TKey, TValue> where TValue : class
    {
        public readonly String Name;

        Dictionary<TKey, CachedItemInfo> _cacheInfos;
        Dictionary<TKey, TValue> _cache;

        protected ICacheContextManager<TContext> ContextManager;
        protected ICacheRetainPolicy<TKey, TContext> RetainPolicy;

        public SimpleCache(String name, 
            ICacheContextManager<TContext> contextManager,
            ICacheRetainPolicy<TKey, TContext> retainPolicy)
        {
            ArgCheck.NotEmpty(name, "name");

            Name = name;
            
            RetainPolicy = retainPolicy;
            ContextManager = contextManager;

            _cache = LoadItems();
            _cacheInfos = LoadCacheInfos();
        }

        #region Load/Save 

        string CacheInfosFileName
        {
            get
            {
                return Path.Combine(AppPaths.CacheFolderPath, Name + "CacheInfos.xml");
            }
        }

        string ItemsFileName
        {
            get
            {
                return Path.Combine(AppPaths.CacheFolderPath, Name + "Items.xml");
            }
        }

        protected virtual Dictionary<TKey, CachedItemInfo> LoadCacheInfos()
        {
            Dictionary<TKey, CachedItemInfo> infos = XmlHelper.DeserializeOrDefault(
                CacheInfosFileName, new Dictionary<TKey, CachedItemInfo>());

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
            return XmlHelper.DeserializeOrDefault(ItemsFileName, new Dictionary<TKey, TValue>());
        }

        public virtual void SaveCache()
        {
            XmlHelper.Serialize(_cacheInfos, CacheInfosFileName);
            XmlHelper.Serialize(_cache, ItemsFileName);
        }

        #endregion

        #region Item access

        public virtual bool Contains(TKey key)
        {
            return _cacheInfos.ContainsKey(key);
        }

        public virtual void Add(TKey key, TValue value)
        {
            // remove if it exists
            _cacheInfos.Remove(key); 
            _cache.Remove(key);

            CachedItemInfo itemInfo = new CachedItemInfo();
            itemInfo.LastAccessTime = DateTime.Now;
            _cacheInfos.Add(key, itemInfo);

            _cache.Add(key, value);

            RemoveExpiredItems();
        }

        protected virtual void RemoveExpiredItems()
        {
            if (RetainPolicy == null) { return; }

            TContext context = ContextManager.CacheContext;
            RetainPolicy.KeysToRemove(_cacheInfos, context);

            var toRemove = RetainPolicy.KeysToRemove(_cacheInfos, ContextManager.CacheContext).ToArray();
            foreach (var key in toRemove)
            {
                Remove(key);
            }
        }

        public virtual void Remove(TKey key)
        {
            _cache.Remove(key);
            _cacheInfos.Remove(key);
        }

        public virtual TValue Get(TKey key)
        {
            TValue item = null;
            if (_cache.TryGetValue(key, out item))
            {
                // Update access info. 
                // NOTE: Entry *must* exist, or there is a bug
                CachedItemInfo info = _cacheInfos[key];
                info.LastAccessTime = DateTime.Now;
                _cacheInfos[key] = info;
            }
            return item;
        }

        protected TValue GetItemNoInfoUpdate(TKey key)
        {
            return _cache[key];
        }

        #endregion

        public IEnumerable<TKey> GetAllKeys()
        {
            return new List<TKey>(_cache.Keys);
        }

        public virtual void Dispose()
        {
            if (_cache == null) { return; }

            foreach(TValue val in _cache.Values)
            {
                if (val is IDisposable) { ((IDisposable)val).Dispose(); }
            }

            _cache = null;
            _cacheInfos = null;
        }
    }

}
