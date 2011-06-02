using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PdfBookReader.Utils
{
    public static class ExtensionMethods
    {
        #region String
        public static bool EqualsIC(this String a, String b)
        {
            if (a == null) { return b == null; }
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion


        #region IDisposable

        /// <summary>
        /// Assign a new value to a disposable field, and dispose the old value (if any).
        /// 
        /// NOTE: recommended to use only when value is created within a class (private setter).
        /// For values that are created externally, the creator should dispose them.
        /// 
        /// Usage: value.AssignNewDisposeOld(ref field)
        /// 
        /// Would be better to have an extension method on *field*, but C# doesn't support "this ref".
        /// </summary>
        /// <typeparam name="T">Type (IDisposable)</typeparam>
        /// <param name="value">New value to assign</param>
        /// <param name="targetField">Field to assign the value to</param>
        /// <param name="otherFieldValues">All other fields which may contain the old value (prevent disposing if still in use)</param>
        /// <remarks></remarks>
        public static void AssignNewDisposeOld<T>(this T value, ref T targetField, params T[] otherFieldValues)
            where T: class, IDisposable
        {
            
            if (value == targetField) { return; }

            // Dispose value if needed
            T fieldValue = targetField;
            if (fieldValue != null && 
                // Check if value is still in use
                otherFieldValues.TrueForAll(ofv => fieldValue != ofv))
            {
                targetField.Dispose();
                targetField = null;
            }

            targetField = value;
        }



        #endregion



        #region Drawing

        // Graphics
        public static void DrawStringBoxed(this Graphics g, String text, int x, int y,
            Font font = null, Brush bgBrush = null, Brush fgBrush = null)
        {
            if (text == null) { text = "[null]"; } 

            if (font == null) { font = SystemFonts.DefaultFont; }
            if (bgBrush == null) { bgBrush = Brushes.DarkRed; }
            if (fgBrush == null) { fgBrush = Brushes.White; }

            const int Off = 4;

            // Debug -- draw page number
            SizeF textSize = g.MeasureString(text, font);
            g.FillRectangle(bgBrush, x, y, textSize.Width + Off * 2, textSize.Height + Off * 2);
            g.DrawString(text, font, fgBrush, x + Off, y + Off);
        }

        public static void DrawLineVertical(this Graphics g, Pen pen, int x)
        {
            g.DrawLine(pen, x, 0, x, 1000);
        }

        public static void DrawLineHorizontal(this Graphics g, Pen pen, int y)
        {
            g.DrawLine(pen, 0, y, 1000, y);
        }

        /// <summary>
        /// Proportionally scale the size to fit within the bounds given by maxSize.
        /// </summary>
        /// <param name="sourceSize"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static Size ScaleToFitBounds(this Size sourceSize, Size maxSize)
        {
            // Fit-to-width
            int width = maxSize.Width;
            double scale = (double)maxSize.Width / sourceSize.Width;
            int height = (int)(sourceSize.Height * scale);

            if (height > maxSize.Height)
            {
                // Fit-to-height
                height = maxSize.Height;
                scale = (double)maxSize.Height / sourceSize.Height;
                width = (int)(sourceSize.Width * scale);
            }

            return new Size(width, height);
        }
        #endregion

        #region LINQ-like
        // LINQ-like
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

        #endregion


    }
}
