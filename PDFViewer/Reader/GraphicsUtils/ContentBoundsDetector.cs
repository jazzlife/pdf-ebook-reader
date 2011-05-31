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
    /// <summary>
    /// Detect real bounds of the content on a physical page
    /// (main text, header, footer).
    /// </summary>
    public class ContentBoundsDetector
    {
        public static void RenderBlobsInfo(Bitmap bmp, Graphics g)
        {
            Invert filter = new Invert();
            filter.ApplyInPlace(bmp);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.FromArgb(8, 8, 8);

            bc.BlobsFilter = new BlobsFilter(bmp.Size);
            bc.FilterBlobs = true;

            bc.ProcessImage(bmp);

            var blobs = bc.GetObjectsInformation();
            if (bc.ObjectsCount == 0) 
            {
                g.FillEllipse(Brushes.Red, 0,0,10,10);
                return;
            }

            foreach (var blob in blobs)
            {
                if (blob.Rectangle.Size == bmp.Size) { continue; }

                g.DrawRectangle(Pens.Orange, blob.Rectangle);
            }

            int left = blobs.Select(b => b.Rectangle.Left).Min();
            int right = blobs.Select(b => b.Rectangle.Right).Max();
            int top = blobs.Select(b => b.Rectangle.Top).Min();
            int bottom = blobs.Select(b => b.Rectangle.Bottom).Max();

            Rectangle contentBounds = new Rectangle(left, top, right - left, bottom - top);

            g.DrawRectangle(Pens.Cyan, contentBounds);
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
