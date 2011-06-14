using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Utils;

namespace PdfBookReader.Render.Cache
{
    interface IExpirationPolicy
    {
        bool ShouldCheck<TKey, TValue>(IDictionary<TKey, TValue> cache);
        IEnumerable<TKey> GetExpiredItemKeys<TKey, T>(Dictionary<TKey, CachedItemInfo> cache);
    }

    class NoExpirationPolicy : IExpirationPolicy
    {
        private NoExpirationPolicy() { }

        static NoExpirationPolicy _instance = new NoExpirationPolicy();
        public static NoExpirationPolicy Instance { get { return _instance; } }

        public bool ShouldCheck<TKey, TValue>(IDictionary<TKey, TValue> cache)
        {
            return false;
        }

        public IEnumerable<TKey> GetExpiredItemKeys<TKey, T>(Dictionary<TKey, CachedItemInfo> cache)
        {
            return Enumerable.Empty<TKey>();
        }
    }

    class DefaultExpirationPolicy : IExpirationPolicy
    {
        int _optimalItemCount;
        int _maxItemCount;

        public DefaultExpirationPolicy(int optimalItemCount, int maxItemCount)
        {
            ArgCheck.Is(optimalItemCount <= maxItemCount, "optimal count > max count");

            _optimalItemCount = optimalItemCount;
            _maxItemCount = maxItemCount;
        }

        public bool ShouldCheck<TKey, TValue>(IDictionary<TKey, TValue> cache)
        {
            return cache.Count > _optimalItemCount;
        }

        public IEnumerable<TKey> GetExpiredItemKeys<TKey, T>(Dictionary<TKey, CachedItemInfo> cache)
        {
            if (cache.Count <= _optimalItemCount) { return Enumerable.Empty<TKey>(); }

            // Choose items not in use
            // Order by last use date
            // Take items over the optimal count
            var keysToRemove = cache
                .OrderBy(x => x.Value.LastAccessTime)
                .Select(x => x.Key)
                .Take(cache.Count - _optimalItemCount);

            return keysToRemove;
        }
    }

}
