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
        IEnumerable<TKey> PrefetchKeyOrder(TContext context);
    }
    
}
