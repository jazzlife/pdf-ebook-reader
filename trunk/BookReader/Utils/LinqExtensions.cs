using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfBookReader.Utils
{
    static class LinqExtensions
    {

        /// <summary>
        /// Enumerable with the range of ints.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<int> IntRange(int start, int count)
        {
            for (int i = start; i < start + count; i++)
            {
                yield return i;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T, int> action)
        {
            int index = 0;
            foreach (T item in enumeration)
            {
                action(item, index++);
            }
        }

        public static bool TrueForAny<T>(this IEnumerable<T> enumeration, Func<T, bool> predicate)
        {
            foreach (T item in enumeration)
            {
                if (predicate(item)) { return true; }
            }
            return false;
        }

        public static bool TrueForAll<T>(this IEnumerable<T> enumeration, Func<T, bool> predicate)
        {
            foreach (T item in enumeration)
            {
                if (!predicate(item)) { return false; }
            }
            return true;
        }

    }
}
