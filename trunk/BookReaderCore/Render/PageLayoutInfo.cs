using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using BookReader.Utils;

namespace BookReader.Render
{

    /// <summary>
    /// Information about a physical page content layout:
    /// rows, header, footer
    /// </summary>
    [DataContract]
    class PageLayoutInfo : LayoutInfo
    {
        [DataMember(Name = "Rows")]
        List<LayoutInfo> _rows;

        [DataMember]
        public LayoutInfo Header;

        [DataMember]
        public LayoutInfo Footer;

        public PageLayoutInfo(Size pageSize) : base(pageSize) { }

        public override void ScaleBounds(Size newPageSize)
        {
            base.ScaleBounds(newPageSize);

            Rows.ForEach(x => x.ScaleBounds(newPageSize));
            if (Header != null) { Header.ScaleBounds(newPageSize); }
            if (Footer != null) { Footer.ScaleBounds(newPageSize); }
        }

        public List<LayoutInfo> Rows 
        {
            get 
            { 
                if (_rows == null) { _rows = new List<LayoutInfo>(); }
                return _rows;
            }            
        }


        #region Debug

        Brush _backShadeBrush = new SolidBrush(Color.FromArgb(100, Color.Black));

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

                // Order of drawing -- less important items first
                if (!Blobs.IsEmpty()) { Blobs.ForEach(x => g.DrawRectangle(Pens.Orange, x.Rectangle)); }
                if (!Words.IsEmpty()) 
                {
                    int i = 0;
                    foreach(var w in Words)
                    {
                        i++;
                        //g.DrawStringBoxed("" + i, w.Bounds.X, w.Bounds.Y, SystemFonts.DefaultFont, Brushes.Gray, Brushes.White, 1);
                        g.DrawRectangle(Pens.OrangeRed, w.Bounds);
                    }
                }



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

                // Content bounds (most important)
                g.FillRectangle(_backShadeBrush, 0, 0, Bounds.Left, PageSize.Height);
                g.FillRectangle(_backShadeBrush, Bounds.Right, 0, PageSize.Width - Bounds.Right, PageSize.Height);
                g.FillRectangle(_backShadeBrush, Bounds.Left, 0, Bounds.Width, Bounds.Top);
                g.FillRectangle(_backShadeBrush, Bounds.Left, Bounds.Bottom, Bounds.Width, PageSize.Height - Bounds.Bottom);
                g.DrawRectangle(Pens.LightBlue, Bounds);
            }
            return bmp;
        }
        #endregion
    }
}
