using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Imaging;
using PDFViewer.Reader.Utils;
using AForge.Imaging.Filters;

namespace PDFViewer.Reader.GraphicsUtils
{
    public class ContentBoundsInfo
    {
        public Blob[] Blobs;
        public Rectangle Bounds;

        public List<Rectangle> Rows;

        public Rectangle Header;
        public Rectangle Footer;
    }

    /// <summary>
    /// Detect real bounds of the content on a physical page
    /// (main text, header, footer).
    /// </summary>
    public class ContentBoundsDetector
    {
        internal ContentBoundsInfo DetectBlobs(Bitmap bmp, Graphics g)
        {
            ContentBoundsInfo cbi = new ContentBoundsInfo();

            Invert filter = new Invert();
            filter.ApplyInPlace(bmp);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.FromArgb(8, 8, 8);

            bc.BlobsFilter = new BlobsFilter(bmp.Size);
            bc.FilterBlobs = true;

            bc.ProcessImage(bmp);

            cbi.Blobs = bc.GetObjectsInformation();
            
            // Debug
            if (bc.ObjectsCount == 0)
            {
                g.FillEllipse(Brushes.Red, 0, 0, 10, 10);
            }
            else
            {
                foreach (var blob in cbi.Blobs)
                {
                    g.DrawRectangle(Pens.Orange, blob.Rectangle);
                }
            }
            return cbi;
        }

        internal void DetectMainContentBounds(ref ContentBoundsInfo cbi, Graphics g)
        {
            if (cbi.Blobs == null) { throw new ArgumentException("cbi.Blobs is null"); }
            if (cbi.Blobs.Length == 0) { return; }

            int left = cbi.Blobs.Select(b => b.Rectangle.Left).Min();
            int right = cbi.Blobs.Select(b => b.Rectangle.Right).Max();
            int top = cbi.Blobs.Select(b => b.Rectangle.Top).Min();
            int bottom = cbi.Blobs.Select(b => b.Rectangle.Bottom).Max();

            cbi.Bounds = new Rectangle(left, top, right - left, bottom - top);
            g.DrawRectangle(Pens.Cyan, cbi.Bounds);
        }

        internal void DetectRowBounds(ref ContentBoundsInfo cbi, Graphics g)
        {
            if (cbi.Blobs == null) { throw new ArgumentException("cbi.Blobs is null"); }
            if (cbi.Blobs.Length == 0) { return; }
            if (cbi.Bounds == Rectangle.Empty) { return; }

            cbi.Rows = new List<Rectangle>();

            // Attempt drawing lines between the rows.
            for (int y = cbi.Bounds.Top; y < cbi.Bounds.Bottom; y++)
            {
                Rectangle rowRect = new Rectangle(cbi.Bounds.Left, y, cbi.Bounds.Width, 1);
                if (cbi.Blobs.FirstOrDefault(b => b.Rectangle.IntersectsWith(rowRect)) == null)
                {
                    g.FillRectangle(Brushes.LightGreen, rowRect);
                }
            }

            // TODO: optimize
            // TODO: add actual rows (not just 1-pixel thin rectangles)
        }

        class BlobsFilter : IBlobsFilter
        {
            readonly Size ImageSize;
            readonly int MinDimension;
            readonly float MinProportion = 0.10f;

            public BlobsFilter(Size imageSize)
            {
                ImageSize = imageSize;
                MinDimension = Math.Max(ImageSize.Width, ImageSize.Height) / 200;
            }

            public bool Check(Blob blob)
            {
                if (blob.Rectangle.Height < MinDimension ||
                    blob.Rectangle.Width < MinDimension) { return false; }

                float proportion = (float)blob.Rectangle.Width / blob.Rectangle.Height;
                if (proportion > 1) { proportion = 1 / proportion; }
                if (proportion < MinProportion) { return false; }

                return true;
            }
        }


    }
}
