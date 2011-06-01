using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PDFViewer.Reader.Utils
{
    public static class ArgCheck
    {
        public static void NotNull<T>(T arg, String argName = null) where T : class
        {
            if (arg == null) { throw new ArgumentNullException(argName); }
        }

        public static void Is(Func<bool> trueCondition, String argName = null)
        {
            if (!trueCondition()) { throw new ArgumentException(argName); }
        }

        public static void IsNot(Func<bool> falseCondition, String argName = null)
        {
            if (falseCondition()) { throw new ArgumentException(argName); }
        }

        // Specific types

        public static void InRange(int arg, int minInclusive, int maxInclusive, String argName = null)
        {
            if (arg < minInclusive || arg > maxInclusive)
            {
                throw new ArgumentOutOfRangeException(
                    String.Format("{0}: {1}, not in range [{2}-{3}]", argName, arg, minInclusive, maxInclusive));
            }
        }

        public static void GreaterThan(int arg, int val, String argName)
        {
            if (!(arg > val)) { throw new ArgumentOutOfRangeException(String.Format("{0}: {1} not > {2}", argName, arg, val)); }
        }
        public static void LessThan(int arg, int val, String argName)
        {
            if (!(arg < val)) { throw new ArgumentOutOfRangeException(String.Format("{0}: {1} not < {2}", argName, arg, val)); }
        }
        public static void GreaterThanOrEqual(int arg, int val, String argName)
        {
            if (!(arg >= val)) { throw new ArgumentOutOfRangeException(String.Format("{0}: {1} not >= {2}", argName, arg, val)); }
        }
        public static void LessThanOrEqual(int arg, int val, String argName)
        {
            if (!(arg <= val)) { throw new ArgumentOutOfRangeException(String.Format("{0}: {1} not <= {2}", argName, arg, val)); }
        }

        public static void IsRatio(float arg, String argName = null)
        {
            if (arg < 0 || arg > 1)
            {
                throw new ArgumentOutOfRangeException(
                    String.Format("{0}={1}, not a ratio [0-1]", argName, arg));
            }
        }

        public static void NotEmpty(Size s, String argName = null)
        {
            if (s.IsEmpty) { throw new ArgumentException(argName + " is empty."); }
        }
    }

}
