using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace PdfBookReader.Render
{

    /// <summary>
    /// Information about a physical page content layout:
    /// rows, header, footer
    /// </summary>
    public class PageLayoutInfo : LayoutInfo
    {
        public List<LayoutInfo> Rows = new List<LayoutInfo>();
        public LayoutInfo Header;
        public LayoutInfo Footer;

        public PageLayoutInfo(Size pageSize) : base(pageSize) { }

        public override void ScaleBounds(Size newPageSize)
        {
            base.ScaleBounds(newPageSize);

            Rows.ForEach(x => x.ScaleBounds(newPageSize));
            if (Header != null) { Header.ScaleBounds(newPageSize); }
            if (Footer != null) { Footer.ScaleBounds(newPageSize); }
        }

        #region Debug
        /// <summary>
        /// Renders the layout info to a bitmap for debugging purposes.
        /// </summary>
        /// <param name="originalBitmap"></param>
        /// <returns></returns>
        [DebugOnly]
        public Bitmap Debug_RenderLayout(Bitmap originalBitmap = null)
        {
            Bitmap bmp = new Bitmap(PageSize.Width, PageSize.Height, PixelFormat.Format24bppRgb);
            if (IsEmpty) { return bmp; }

            using (Graphics g = Graphics.FromImage(bmp))
            {
                if (originalBitmap != null)
                {
                    g.DrawImageUnscaled(originalBitmap, 0, 0);
                }

                // Order of drawing -- less important items first
                Blobs.ForEach(x => g.DrawRectangle(Pens.Orange, x.Rectangle));

                // Rows
                Rows.ForEach(x => g.DrawRectangle(Pens.Pink, x.Bounds));

                // Header and footer
                if (Header != null)
                {
                    g.DrawRectangle(Pens.Yellow, Header.Bounds);
                    g.FillRectangle(Brushes.Yellow, Header.Bounds.X, Header.Bounds.Y, 10, Header.Bounds.Height);
                }
                if (Footer != null)
                {
                    g.DrawRectangle(Pens.Yellow, Footer.Bounds);
                    g.FillRectangle(Brushes.Yellow, Footer.Bounds.X, Footer.Bounds.Y, 10, Footer.Bounds.Height);
                }

                // Content bounds (most important
                g.DrawRectangle(Pens.LightBlue, Bounds);
            }
            return bmp;
        }
        #endregion
    }
}
