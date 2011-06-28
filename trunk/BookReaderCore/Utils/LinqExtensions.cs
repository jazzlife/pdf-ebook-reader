using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace BookReader.Utils
{
    static class LinqExtensions
    {
        /// <summary>
        /// Split the group by comparing all pairs, e.g.
        /// 
        /// {1,1,2,1}.Split((a,b) => a != b) // returns {{1,1},{2},{1}}
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="shouldSplit"></param>
        /// <returns></returns>

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, Func<T, T, bool> shouldSplit)
        {
            List<T> sublist = new List<T>();

            T prevItem = default(T);
            bool isFirst = true;
            foreach (T item in list)
            {
                if (!isFirst)
                {
                    if (shouldSplit(prevItem, item))
                    {
                        yield return sublist;
                        // start new sublist
                        sublist = new List<T>();
                    }
                }
                sublist.Add(item);
                prevItem = item;
                isFirst = false;
            }
            yield return sublist;
        }

        /// <summary>
        /// Create an enumerable with the range of ints.
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

        /// <summary>
        /// Create an enumerable with n repeated items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> RepeatElements<T>(T item, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Create an enumerable repeating the sequence n times.
        /// {a,b}.RepeatSequence(3) => {a,b,a,b,a,b}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> RepeatSequence<T>(this IEnumerable<T> list, int count)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (T item in list) { yield return item; }
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> list)
        {
            return !list.Any();
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

        public static String ElementsToStringS<T>(this IEnumerable<T> list)
        {
            return ElementsToString(list, "{2}:[{0}]", "{0}", ",");
        }


        public static String ElementsToString<T>(this IEnumerable<T> list, 
            String listFormat = "Count: {2} in {1}\r\n{0}", String elemFormat = "  [{1}] {0}", String delim = "\r\n")
        {
            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach(T x in list)
            {
                if (i != 0) { sb.Append(delim); } 
                sb.AppendFormat(elemFormat, x, i); 
                i++;
            }

            return listFormat.F(sb.ToString(), list, list.Count());
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> list, T newItem)
        {
            foreach (T x in list) { yield return x; }
            yield return newItem;
        }
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> list, T newItem)
        {
            yield return newItem;
            foreach (T x in list) { yield return x; }
        }
        
        
    }
}
