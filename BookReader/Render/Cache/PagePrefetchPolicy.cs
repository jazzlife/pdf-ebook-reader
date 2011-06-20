using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Model;

namespace PdfBookReader.Render.Cache
{
    class PagePrefetchPolicy :
        ICacheRetainPolicy<PageKey, PageCacheContext>,
        IPrefetchPolicy<PageKey, PageCacheContext>
    {
        #region pre-configured items

        static PagePrefetchPolicy _diskPolicy;
        public static PagePrefetchPolicy GetGeneralPolicy()
        {

            if (_diskPolicy == null)
            {
                _diskPolicy = new PagePrefetchPolicy()
                {
                    Retain_InCurrentBookAfter = 30,
                    Retain_InCurrentBookBefore = 10,
                    Retain_InOtherBookAfter = 20,
                    Retain_InOtherBookBefore = 0,
                    Retain_Initial = 10,
                    OtherItemsToKeepCount = 100
                };
            }
            return _diskPolicy;
        }

        static PagePrefetchPolicy _memoryPolicy;
        public static PagePrefetchPolicy GetMemoryPolicy()
        {
            if (_memoryPolicy == null)
            {
                _memoryPolicy = new PagePrefetchPolicy()
                {
                    Retain_InCurrentBookAfter = 10,
                    Retain_InCurrentBookBefore = 3,
                    Retain_InOtherBookAfter = 0,
                    Retain_InOtherBookBefore = 0,
                    Retain_Initial = 0,
                    OtherItemsToKeepCount = 0
                };
            }
            return _memoryPolicy;
        }



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

            foreach (Book book in context.Library.Books)
            {
                if (book.CurrentPosition == null) { continue; }

                // Retain the range of pages around the current page                
                int currentPage = book.CurrentPosition.PageNum;

                if (context.Library.CurrentBook != null &&
                    key.BookId == context.Library.CurrentBook.Id)
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
            if (context.Library.CurrentBook != null)
            {
                var keys = PrefetchKeysForBook(
                    context.Library.CurrentBook,
                    Retain_InCurrentBookBefore,
                    Retain_InCurrentBookAfter,
                    context.ScreenWidth);

                foreach (var key in keys) { yield return key; }
            }

            // Other books
            foreach (Book book in context.Library.Books)
            {
                // Skip current book, already done
                if (book == context.Library.CurrentBook) { continue; }

                var keys = PrefetchKeysForBook(
                    book,
                    Retain_InOtherBookBefore,
                    Retain_InOtherBookAfter,
                    context.ScreenWidth);

                foreach (var key in keys) { yield return key; }
            }
        }

        IEnumerable<PageKey> PrefetchKeysForBook(Book book, int keepBefore, int keepAfter, int screenWidth)
        {
            IEnumerable<int> pageNums = PrefetchPagesForBook(book, keepBefore, keepAfter);
            foreach (int page in pageNums)
            {
                yield return new PageKey(book.Id, page, screenWidth);
            }
        }

        IEnumerable<int> PrefetchPagesForBook(Book book, int keepBefore, int keepAfter)
        {
            // items around the current page
            if (book.CurrentPosition != null)
            {
                int currentPage = book.CurrentPosition.PageNum;

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

    class PageCacheContext
    {
        public readonly int ScreenWidth;
        public readonly BookLibrary Library;
        public PageCacheContext(int screenWidth, BookLibrary library)
        {
            ScreenWidth = screenWidth;
            Library = library;
        }
    }

}
