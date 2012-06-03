using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BookReader.Utils
{
    // TODO: refactor 
    public static class ExtensionMethods
    {
        #region String
        public static bool EqualsIC(this String a, String b)
        {
            if (a == null) { return b == null; }
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Same as String.Format(this, args);
        /// Usage: "foo {0}={1}".F(a1, a2)
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string F(this String format, params object[] args)
        {
            return String.Format(format, args);
        }

        #endregion

        #region Numbers

        public static int Round(this float num)
        {
            return (int)Math.Round(num);
        }

        public static int Round(this double num)
        {
            return (int)Math.Round(num);
        }

        public static bool AlmostEquals(this float thisNum, float otherNum, float tolerance = 0.000001f)
        {
            return Math.Abs(thisNum - otherNum) < tolerance;
        }

        public static bool AlmostEquals(this RectangleF thisRect, RectangleF otherRect, float tolerance = 0.000001f)
        {
            return
                thisRect.X.AlmostEquals(otherRect.X) &&
                thisRect.Y.AlmostEquals(otherRect.Y) &&
                thisRect.Width.AlmostEquals(otherRect.Width) &&
                thisRect.Height.AlmostEquals(otherRect.Height);
        }

        #endregion

        #region Drawing

        public static Color NewShade(this Color color, double brightnessMultiplier)
        {
            return NewShade(color, brightnessMultiplier, brightnessMultiplier, brightnessMultiplier);
        }

        public static Color NewShade(this Color color, double rMultiplier, double gMultiplier, double bMultiplier)
        {
            double r = color.R * rMultiplier;
            double g = color.G * gMultiplier;
            double b = color.B * bMultiplier;

            int rInt = (int)Math.Min(255, r);
            int gInt = (int)Math.Min(255, b);
            int bInt = (int)Math.Min(255, b);

            return Color.FromArgb(color.A, rInt, gInt, bInt);
        }


        // Graphics
        public static void DrawStringBoxed(this Graphics g, String text, int x, int y,
            Font font = null, Brush bgBrush = null, Brush fgBrush = null, int offset = 4)
        {
            if (text == null) { text = "[null]"; } 

            if (font == null) { font = SystemFonts.DefaultFont; }
            if (bgBrush == null) { bgBrush = Brushes.DarkRed; }
            if (fgBrush == null) { fgBrush = Brushes.White; }

            // Debug -- draw page number
            SizeF textSize = g.MeasureString(text, font);
            g.FillRectangle(bgBrush, x, y, textSize.Width + offset * 2, textSize.Height + offset * 2);
            g.DrawString(text, font, fgBrush, x + offset, y + offset);
        }

        public static void DrawLineVertical(this Graphics g, Pen pen, int x)
        {
            g.DrawLine(pen, x, 0, x, 1000);
        }

        public static void DrawLineHorizontal(this Graphics g, Pen pen, int y, int width = 1000)
        {
            g.DrawLine(pen, 0, y, width, y);
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

        #region List
        public static void RemoveLast<T>(this IList<T> list)
        {
            list.RemoveAt(list.Count - 1);
        }
        public static void RemoveFirst<T>(this IList<T> list)
        {
            list.RemoveAt(0);
        }
        #endregion
    }
}
