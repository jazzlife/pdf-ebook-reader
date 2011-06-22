using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using BookReader.Render.Cache;
using BookReader.Model;
using System.IO;
using BookReader.Utils;

namespace BookReaderTest.Render.Cache
{
    [TestFixture]
    public class PageContentPrefetchPolicyTest
    {
        const int ScreenWidth = 100;
        static readonly DateTime BaseTime = new DateTime(2000, 1, 1);

        [Test]
        public void EmptyPolicy()
        {
            var policy = new PagePrefetchAndRetainPolicy();

            var key = new PageKey(Guid.Empty, 1, ScreenWidth);
            var context = new PageCacheContext(ScreenWidth, GetLibrary(0));

            Assert.IsFalse(policy.MustRetain(key, context), "not retain with empty library");

            var dict = GetCacheDict(0);
            CollectionAssert.IsEmpty(policy.KeysToRemove(dict, context), "no keys exist");

            dict = GetCacheDict(5);
            CollectionAssert.AreEquivalent(dict.Keys, policy.KeysToRemove(dict, context), "return all keys");

            CollectionAssert.IsEmpty(policy.PrefetchKeyOrder(context), "prefetch list empty");
        }

        #region Prefetch

        [Test]
        public void Prefetch_InitialPages()
        {
            const int NumBooks = 3;
            var context = new PageCacheContext(ScreenWidth, GetLibrary(NumBooks));

            // Run method 
            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_Initial = 10
            };
            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            // Check for each book
            var booksPos = context.BookPositions;
            for (int bookIdx = 0; bookIdx < NumBooks; bookIdx++)
            {
                // Has the GUID
                CollectionAssert.Contains(keys.Select(x=>x.BookId), booksPos[bookIdx].BookId);

                // Has all the elements
                var bookKeys = LinqExtensions.IntRange(1, 10)
                    .Select(x => new PageKey(booksPos[bookIdx].BookId, x, ScreenWidth));
                
                CollectionAssert.IsSubsetOf(bookKeys, keys);
            }
        }

        [Test]
        public void Prefetch_CurrentBookFirst()
        {
            const int NumBooks = 3;
            
            var library = GetLibrary(NumBooks);
            library.CurrentBook = library.Books.Last();

            var context = new PageCacheContext(ScreenWidth, library);

            // Run method 
            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_Initial = 10
            };
            var keys = policy.PrefetchKeyOrder(context);

            // Has all the elements
            var curBookKeys = LinqExtensions.IntRange(1, 10)
                .Select(x => new PageKey(library.CurrentBook.Id, x, ScreenWidth));

            CollectionAssert.AreEqual(curBookKeys, keys.Take(10), "current book's pages come first");
        }

        [Test]
        public void Prefetch_CurrentPage()
        {
            var library = GetLibrary(1);
            Book book = library.Books[0];
            book.CurrentPosition = PositionInBook.FromPhysicalPage(33, 100);

            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy();
            
            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            Assert.AreEqual(1, keys.Count());
            Assert.AreEqual(33, keys.First().PageNum);
        }

        [Test]
        public void Prefetch_CurrentPageFirst()
        {
            var library = GetLibrary(1);
            Book book = library.Books[0];
            library.CurrentBook = book;
            book.CurrentPosition = PositionInBook.FromPhysicalPage(33, 100);

            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_Initial = 10
            };

            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            Assert.AreEqual(33, keys.First().PageNum);
            Assert.AreEqual(11, keys.Count());
        }

        [Test]
        public void Prefetch_CurrentPageAfter()
        {
            var library = GetLibrary(1);
            Book book = library.Books[0];
            library.CurrentBook = book;
            book.CurrentPosition = PositionInBook.FromPhysicalPage(33, 100);
            
            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_InCurrentBookAfter = 10
            };

            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            // Note that current page is added to the total, so 11 items, not 10
            CollectionAssert.AreEqual(
                LinqExtensions.IntRange(33, 11), 
                keys.Select(x=>x.PageNum));
        }

        [Test]
        public void Prefetch_CurrentPageBefore()
        {
            var library = GetLibrary(1);
            Book book = library.Books[0];
            library.CurrentBook = book;
            book.CurrentPosition = PositionInBook.FromPhysicalPage(33, 100);

            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_InCurrentBookBefore = 10
            };

            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            // Note that current page is added to the total, so 11 items, not 10
            CollectionAssert.AreEqual(
                LinqExtensions.IntRange(33-10, 11).Reverse(), 
                keys.Select(x => x.PageNum));
        }

        [Test]
        public void Prefetch_CurrentPageComplex()
        {
            var library = GetLibrary(1);
            Book book = library.Books[0];
            library.CurrentBook = book;
            book.CurrentPosition = PositionInBook.FromPhysicalPage(33, 100);

            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_InCurrentBookBefore = 2,
                Retain_InCurrentBookAfter = 7,
                Retain_Initial = 3
            };

            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            // current, 2 x after, 1 x before..., initial
            CollectionAssert.AreEqual(
                new int[] { 33, 34, 35, 32, 36, 37, 31, 38, 39, 40, 1, 2, 3 },
                keys.Select(x => x.PageNum));
        }

        [Test]
        public void Prefetch_ThreeBooksComplex()
        {
            var library = GetLibrary(3);
            Book curBook = library.Books.Last();
            library.CurrentBook = curBook;
            curBook.CurrentPosition = PositionInBook.FromPhysicalPage(33, 100);
            library.Books[1].CurrentPosition = PositionInBook.FromPhysicalPage(66, 100);

            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_InCurrentBookAfter = 3,
                Retain_InCurrentBookBefore = 1,
                Retain_InOtherBookAfter = 2,
                Retain_InOtherBookBefore = 1,

                Retain_Initial = 2
            };

            var keys = policy.PrefetchKeyOrder(context);
            AssertMustRetainAllKeys(keys, policy, context);

            CollectionAssert.AreEqual(
                LinqExtensions.RepeatElements(curBook.Id, 7),
                keys.Take(7).Select(x => x.BookId), "First items from current book");

            // Complex pattern, read the rules...
            CollectionAssert.AreEqual(
                new int[] { 
                    33, 34, 35, 32, 36, 1, 2, 
                    1, 2,
                    66, 67, 68, 65, 1, 2
                },
                keys.Select(x => x.PageNum));
        }

        #endregion

        #region remove

        [Test]
        public void Remove_AllNotFetched()
        {
            var library = GetLibrary(3);
            var context = new PageCacheContext(ScreenWidth, library);

            var policy = new PagePrefetchAndRetainPolicy()
            {
                Retain_Initial = 2
            };

            var dict = GetCacheDict(10, library.Books.ToArray());

            var keysToRemove = policy.KeysToRemove(dict, context);

            CollectionAssert.AreEquivalent(
                LinqExtensions.IntRange(3, 8).RepeatSequence(3),
                keysToRemove.Select(x => x.PageNum));
        }

        [Test]
        public void Remove_KeepOtherItems()
        {
            var context = new PageCacheContext(ScreenWidth, GetLibrary(1));

            var policy = new PagePrefetchAndRetainPolicy()
            {
                OtherItemsToKeepCount = 7
            };

            var dict = GetCacheDict(10);
            var keysToRemove = policy.KeysToRemove(dict, context);

            // Keeping 7 items, removing 3
            Assert.AreEqual(3, keysToRemove.Count());
            
            // Makes sure *oldest* ones are removed
            Assert.AreEqual(BaseTime + TimeSpan.FromDays(0), dict[keysToRemove.ElementAt(2)].LastAccessTime);
            Assert.AreEqual(BaseTime + TimeSpan.FromDays(1), dict[keysToRemove.ElementAt(1)].LastAccessTime);
            Assert.AreEqual(BaseTime + TimeSpan.FromDays(2), dict[keysToRemove.ElementAt(0)].LastAccessTime);

            // Must NOT retain those keys
            foreach (var key in keysToRemove)
            {
                Assert.IsFalse(policy.MustRetain(key, context));
            }
        }

        #endregion

        #region Helpers

        void AssertMustRetainAllKeys(IEnumerable<PageKey> keys, IPageRetainPolicy policy, PageCacheContext context)
        {
            foreach (var key in keys)
            {
                Assert.IsTrue(policy.MustRetain(key, context), "Must retain all prefetched keys");
            }
        }

        BookLibrary GetLibrary(int numBooks)
        {
            // Load from non-existing temp file to prevent saving to real library
            String file = Path.GetTempFileName();
            File.Delete(file);

            var lib = BookLibrary.Load(file);

            for (int i = 0; i < numBooks; i++)
            {
                var book = new Book("book-" + i);
                lib.AddBook(book);
            }

            return lib;
        }

        Dictionary<PageKey, CachedItemInfo> GetCacheDict(int numPages)
        {
            var dict = new Dictionary<PageKey, CachedItemInfo>();
            for (int i = 0; i < numPages; i++)
            {
                CachedItemInfo itemInfo = new CachedItemInfo();
                itemInfo.LastAccessTime = BaseTime + TimeSpan.FromDays(i);

                dict.Add(new PageKey(Guid.NewGuid(), ScreenWidth, i+1), itemInfo);
            }
            return dict;
        }

        Dictionary<PageKey, CachedItemInfo> GetCacheDict(int numPages, params Book[] books)
        {
            var dict = new Dictionary<PageKey, CachedItemInfo>();
            foreach(Book book in books)
            {
                for (int i = 0; i < numPages; i++)
                {
                    CachedItemInfo itemInfo = new CachedItemInfo();
                    itemInfo.LastAccessTime = BaseTime + TimeSpan.FromDays(i);

                    dict.Add(new PageKey(book.Id, i + 1, ScreenWidth), itemInfo);
                }
            }
            return dict;
        }


        #endregion
    }
}
