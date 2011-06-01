using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Imaging;
using System.Drawing.Imaging;

namespace PDFViewer.Reader.Render
{
    /// <summary>
    /// Layout info for a page element
    /// </summary>
    public class LayoutInfo 
    {
        public Size PageSize { get; private set; }

        /// <summary>
        /// Bounds based on PageSize
        /// </summary>
        public Rectangle Bounds;

        public LayoutInfo(Size pageSize)
        {
            PageSize = pageSize;
        }

        /// <summary>
        /// Recompute the bounds based on the new page size.
        /// </summary>
        /// <param name="newPageSize"></param>
        public virtual void ScaleBounds(Size newPageSize)
        {
            RectangleF relBounds = BoundsRelative;

            Bounds = new Rectangle(
                (int)(relBounds.X * newPageSize.Width),
                (int)(relBounds.Y * newPageSize.Height),
                (int)(relBounds.Width * newPageSize.Width),
                (int)(relBounds.Height * newPageSize.Height));

            PageSize = newPageSize;
        }

        public bool IsEmpty { get { return Bounds.IsEmpty; } }

        /// <summary>
        /// Bounds in relative 0-1 coordinates
        /// </summary>
        public RectangleF BoundsRelative
        {
            get
            {
                return new RectangleF(
                    (float)Bounds.X / PageSize.Width,
                    (float)Bounds.Y / PageSize.Height,
                    (float)Bounds.Width / PageSize.Width,
                    (float)Bounds.Height / PageSize.Height);
            }
        }

        // Blobs. For internal use, never scaled.
        internal List<Blob> Blobs = new List<Blob>();
    }

}
