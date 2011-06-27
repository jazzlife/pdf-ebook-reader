using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using BookReader.Utils;

namespace BookReader.Render.Layout
{

    /// <summary>
    /// Information about a physical page content layout:
    /// rows, header, footer
    /// </summary>
    [DataContract]
    class PageLayout : LayoutElement
    {
        public Size PageSize { get; private set; }

        public PageLayout(Size pageSize) 
        {
            ArgCheck.NotEmpty(pageSize, "pageSize");
            PageSize = pageSize;
        }

        public PageLayout ScaleToScreen(Size screenSize)
        {
            float mult = (float)screenSize.Width / Bounds.Width;

            Size newPageSize = new Size((PageSize.Width * mult).Round(), (PageSize.Height * mult).Round());

            PageLayout newLayout = new PageLayout(newPageSize);
            newLayout.Bounds = GetScaledBounds(PageSize, newPageSize);

            // TODO: scale all elements down recursively, if needed

            return newLayout;
        }

        public RectangleF UnitBounds
        {
            get { return GetUnitBounds(PageSize); } 
        }

        #region Debug

        Brush _backShadeBrush = new SolidBrush(Color.FromArgb(100, Color.Black));
        Font _numberFont = new Font("Arial", 6);

        /// <summary>
        /// Renders the layout info to a bitmap for debugging purposes.
        /// </summary>
        /// <param name="originalBitmap"></param>
        /// <returns></returns>
        [DebugOnly]
        public DW<Bitmap> Debug_DrawLayout(DW<Bitmap> originalBitmap = null)
        {
            DW<Bitmap> bmp = DW.Wrap(new Bitmap(PageSize.Width, PageSize.Height, PixelFormat.Format24bppRgb));
            if (IsEmpty) { return bmp; }

            using (Graphics g = Graphics.FromImage(bmp.o))
            {
                if (originalBitmap != null)
                {
                    g.DrawImageUnscaled(originalBitmap.o, 0, 0);
                }

                // Content bounds (most important)
                g.FillRectangle(_backShadeBrush, 0, 0, Bounds.Left, PageSize.Height);
                g.FillRectangle(_backShadeBrush, Bounds.Right, 0, PageSize.Width - Bounds.Right, PageSize.Height);
                g.FillRectangle(_backShadeBrush, Bounds.Left, 0, Bounds.Width, Bounds.Top);
                g.FillRectangle(_backShadeBrush, Bounds.Left, Bounds.Bottom, Bounds.Width, PageSize.Height - Bounds.Bottom);

                Debug_DrawLayout(g);
            }
            return bmp;
        }
        #endregion

    }
}
