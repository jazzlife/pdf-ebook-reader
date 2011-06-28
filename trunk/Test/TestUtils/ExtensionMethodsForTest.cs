using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;
using NUnit.Framework;
using System.Collections;
using System.Diagnostics;

namespace BookReaderTest.TestUtils
{
    static class ExtensionMethodsForTest
    {


        // Order of expected/actual inverted on purpose.
        [DebuggerHidden]
        public static void AssertEqualsTo<T>(this IEnumerable<T> actual, params T[] expected)
        {
            AssertEquals<T>(actual, expected);
        }

        // Order of expected/actual inverted on purpose.
        [DebuggerHidden]
        public static void AssertEquals<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var expIt = expected.GetEnumerator();
            var actIt = actual.GetEnumerator();

            int index = 0;
            while (true)
            {
                bool expHasNext = expIt.MoveNext();
                bool actHasNext = actIt.MoveNext();

                // Done, success
                if (!expHasNext && !actHasNext) { return; }

                if (!expHasNext) { ThrowCollectionDifferent(expected, actual, "Longer than expected"); }
                if (!actHasNext) { ThrowCollectionDifferent(expected, actual, "Shorter than expected"); }

                T expItem = expIt.Current;
                T actItem = actIt.Current;

                if ((expItem == null && actItem != null) || 
                    (expItem != null && actItem == null) ||
                    (expItem != null && !expItem.Equals(actItem)))
                {
                    ThrowItemsDifferentAt(expected, actual, index, expItem, actItem);
                }

                index++;
            }
        }

        [DebuggerHidden]
        static void ThrowCollectionDifferent<T>(IEnumerable<T> expected, IEnumerable<T> actual, String message)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine("Expected: " + expected.ElementsToStringS());
            sb.AppendLine("  Actual: " + actual.ElementsToStringS());
            throw new AssertionException(sb.ToString());
        }

        [DebuggerHidden]
        static void ThrowItemsDifferentAt<T>(IEnumerable<T> expected, IEnumerable<T> actual, int index, T expItem, T actItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Items different at index: " + index);
            sb.AppendLine("Expected: " + expItem);
            sb.AppendLine("  Actual: " + actItem);
            sb.Append("Collections:");

            ThrowCollectionDifferent(expected, actual, sb.ToString());
        }

    }
}
