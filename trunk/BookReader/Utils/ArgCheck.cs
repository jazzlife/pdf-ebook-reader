﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace PdfBookReader.Utils
{
    public static class ArgCheck
    {
        public static void NotNull<T>(T arg, String argName = null) where T : class
        {
            if (arg == null) { throw new ArgumentNullException(argName); }
        }

        public static void Is(bool trueCondition, String message = null)
        {
            if (!trueCondition) { throw new ArgumentException(message); }
        }

        public static void IsNot(bool falseCondition, String message = null)
        {
            if (falseCondition) { throw new ArgumentException(message); }
        }

        // Files
        public static void FileExists(String filename)
        {
            if (!File.Exists(filename))
            {
                throw new ArgumentException("File does not exits: " + Path.GetFullPath(filename));
            }
        }

        public static void FilenameCharsValid(String filename, String argName = null)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (filename.Contains(c)) { throw new ArgumentException("Invalid filename character in: " + filename, argName); }
            }
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
        public static void InRange(float arg, float minInclusive, float maxInclusive, String argName = null)
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

        public static void IsUnit(float arg, String argName = null)
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

        internal static void NotEmpty(string name, string argName = null)
        {
            if (name == null || String.IsNullOrEmpty(name.Trim()))
            {
                throw new ArgumentException("Empty arg " + argName);
            }
        }
    }

}