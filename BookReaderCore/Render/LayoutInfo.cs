using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Imaging;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace BookReader.Render
{
    /// <summary>
    /// Layout info for a page element
    /// </summary>
    [DataContract]
    public class LayoutInfo 
    {
        // For serialization - cut down on ridiculous amount of XML crap
        [DataMember(Name = "S")]
        string StringRep
        {
            get
            {
                return PageSize.Width + "x" + PageSize.Height + " "
                    + Bounds.X + "," + Bounds.Y + "," + Bounds.Width + "," + Bounds.Height;
            }
            set
            {
                String[] parts = value.Split('x', ' ', ',');
                int i=0;
                PageSize = new Size(int.Parse(parts[i++]), int.Parse(parts[i++]));
                Bounds = new Rectangle(int.Parse(parts[i++]), int.Parse(parts[i++]), int.Parse(parts[i++]), int.Parse(parts[i++]));
            }
        }

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
            RectangleF relBounds = BoundsUnit;

            Bounds = new Rectangle(
                (int)(relBounds.X * newPageSize.Width),
                (int)(relBounds.Y * newPageSize.Height),
                (int)(relBounds.Width * newPageSize.Width),
                (int)(relBounds.Height * newPageSize.Height));

            PageSize = newPageSize;
        }

        public bool IsEmpty { get { return Bounds.IsEmpty; } }

        /// <summary>
        /// Bounds in unit interval [0, 1] coordinates 
        /// </summary>
        public RectangleF BoundsUnit
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
        internal List<PDFLibNet.PDFTextWord> Words = new List<PDFLibNet.PDFTextWord>();
    }

}
