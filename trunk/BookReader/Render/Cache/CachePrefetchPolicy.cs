using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfBookReader.Render.Cache
{
    interface ICacheRetainPolicy<TKey, TContext>
    {
        bool MustRetain(TKey key, TContext context);
        IEnumerable<TKey> KeysToRemove(IDictionary<TKey, CachedItemInfo> cacheInfos, TContext context);
    }

    interface IPrefetchPolicy<TKey, TContext>
    {
        /// <summary>
        /// Returns the order of prefetch keys based on the context. 
        /// Keys may be already fetched, and pages may not be in the valid range,
        /// prefetch algorithm is responsible for hendling invalid items.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<TKey> PrefetchKeyOrder(TContext context);
    }
    
}
