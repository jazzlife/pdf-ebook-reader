using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Model;

namespace PdfBookReader.Render.Cache
{
    interface IPageRetainPolicy : ICacheRetainPolicy<PageKey, PageCacheContext> { }
    interface IPagePrefetchPolicy : IPrefetchPolicy<PageKey, PageCacheContext> { }

    class PagePrefetchAndRetainPolicy : IPageRetainPolicy, IPagePrefetchPolicy
    {
        #region pre-configured items




        #endregion

        #region Options

        /// <summary>
        /// Number of pages before current page of current book
        /// </summary>
        public int Retain_InCurrentBookAfter = 0;

        /// <summary>
        /// Number of pages before current page of current book
        /// </summary>
        public int Retain_InCurrentBookBefore = 0;

        /// <summary>
        /// Number of pages after current page of non-current books
        /// </summary>
        public int Retain_InOtherBookAfter = 0;

        /// <summary>
        /// Number of pages before current page of non-current books
        /// </summary>
        public int Retain_InOtherBookBefore = 0;

        /// <summary>
        /// Number of pages at the beginning of each book
        /// </summary>
        public int Retain_Initial = 0;

        /// <summary>
        /// Number of other items (!MustRetain) to keep
        /// </summary>
        public int OtherItemsToKeepCount = 0;

        #endregion

        public bool MustRetain(PageKey key, PageCacheContext context)
        {
            // Do not retain items where screen sizes don't match
            if (key.ScreenWidth != context.ScreenWidth) { return false; }

            // Retain the initial few pages
            if (key.PageNum <= Retain_Initial) { return true; }

            foreach (var bookPos in context.BookPositions)
            {
                if (bookPos.Position == null) { continue; }

                // Retain the range of pages around the current page                
                int currentPage = bookPos.Position.PageNum;

                if (context.CurrentBookPosition != null &&
                    key.BookId == context.CurrentBookPosition.BookId)
                {
                    // Current book
                    if (currentPage - Retain_InCurrentBookBefore <= key.PageNum
                        && key.PageNum <= currentPage + Retain_InCurrentBookAfter)
                    {
                        return true;
                    }
                }
                else
                {
                    // Other books
                    if (currentPage - Retain_InOtherBookBefore <= key.PageNum
                        && key.PageNum <= currentPage + Retain_InOtherBookAfter)
                    {
                        return true;
                    }
                }
            }

            // All others are optional
            return false;
        }

        public IEnumerable<PageKey> KeysToRemove(
            IDictionary<PageKey, CachedItemInfo> cacheInfos, PageCacheContext context)
        {
            var toRemove = cacheInfos.Where(x => !MustRetain(x.Key, context));

            // Optionally keep some of the other items (by recency)
            if (OtherItemsToKeepCount > 0)
            {
                int toRemoveCount = toRemove.Count() - OtherItemsToKeepCount;
                if (toRemoveCount > 0)
                {
                    toRemove = toRemove
                        .OrderBy(x => cacheInfos[x.Key].LastAccessTime)
                        .Take(toRemoveCount);
                }
            }

            return toRemove.Select(x => x.Key);
        }

        // Needs to be in sync with MustRetain method
        // All items we prefetch should pass MustRetain,
        // otherwise we'll keep prefetching and discarding 
        // items indefinitely.
        public IEnumerable<PageKey> PrefetchKeyOrder(PageCacheContext context)
        {
            // Current book 
            if (context.CurrentBookPosition != null)
            {
                var keys = PrefetchKeysForBook(
                    context.CurrentBookPosition,
                    Retain_InCurrentBookBefore,
                    Retain_InCurrentBookAfter,
                    context.ScreenWidth);

                foreach (var key in keys) { yield return key; }
            }

            // Other books
            foreach (var bookPos in context.BookPositions)
            {
                // Skip current book, already done
                if (context.CurrentBookPosition != null &&
                    bookPos.BookId == context.CurrentBookPosition.BookId) { continue; }

                var keys = PrefetchKeysForBook(
                    bookPos,
                    Retain_InOtherBookBefore,
                    Retain_InOtherBookAfter,
                    context.ScreenWidth);

                foreach (var key in keys) { yield return key; }
            }
        }

        IEnumerable<PageKey> PrefetchKeysForBook(PageCacheContext.BookPosition bookPos, int keepBefore, int keepAfter, int screenWidth)
        {
            IEnumerable<int> pageNums = PrefetchPagesForBook(bookPos, keepBefore, keepAfter);
            foreach (int page in pageNums)
            {
                yield return new PageKey(bookPos.BookId, page, screenWidth);
            }
        }

        IEnumerable<int> PrefetchPagesForBook(PageCacheContext.BookPosition bookPos, int keepBefore, int keepAfter)
        {
            // items around the current page
            if (bookPos.Position != null)
            {
                int currentPage = bookPos.Position.PageNum;

                yield return currentPage;

                int beforeMin = currentPage - keepBefore;
                int afterMax = currentPage + keepAfter;

                int beforeNum = currentPage;
                int afterNum = currentPage;

                // Two steps forward, one step back
                while (true)
                {
                    ++afterNum;
                    if (afterNum <= afterMax) { yield return afterNum; }

                    ++afterNum;
                    if (afterNum <= afterMax) { yield return afterNum; }

                    --beforeNum;
                    if (beforeNum >= beforeMin) { yield return beforeNum; }

                    if (beforeMin > beforeNum && afterNum > afterMax) { break; }
                }
            }

            // first few pages (overlap possible)
            for (int i = 1; i <= Retain_Initial; i++)
            {
                yield return i;
            }
        }
    }


}
