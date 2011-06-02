using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Analyzes the page layout based on connected blobs contained within it.
    /// </summary>
    public class BlobPageLayoutAnalyzer : IPageLayoutAnalyzer
    {

        public PageLayoutInfo DetectPageLayout(Bitmap bmp)
        {
            ArgCheck.NotNull(bmp, "bmp");

            PageLayoutInfo layout = new PageLayoutInfo(bmp.Size);

            DetectBlobs(ref layout, bmp);
            layout.Bounds = BoundsAroundBlobs(layout.Blobs);
            DetectRowBounds(ref layout);

            return layout;
        }

        void DetectBlobs(ref PageLayoutInfo cbi, Bitmap bmp)
        {
            Invert filter = new Invert();
            filter.ApplyInPlace(bmp);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.FromArgb(8, 8, 8);

            bc.BlobsFilter = new BlobsFilter(bmp.Size);
            bc.FilterBlobs = true;

            bc.ProcessImage(bmp);

            cbi.Blobs.AddRange(bc.GetObjectsInformation());
        }

        void DetectRowBounds(ref PageLayoutInfo cbi)
        {
            if (cbi.Blobs.Count == 0) { return; }
            if (cbi.Bounds == Rectangle.Empty) { return; }

            cbi.Rows = new List<LayoutInfo>();

            LayoutInfo currentRow = null;
            // Attempt drawing lines between the rows.
            for (int y = cbi.Bounds.Top; y < cbi.Bounds.Bottom; y++)
            {
                Rectangle rowRect = new Rectangle(cbi.Bounds.Left, y, cbi.Bounds.Width, 1);

                var blobsInRow = cbi.Blobs.Where(b => b.Rectangle.IntersectsWith(rowRect));

                if (blobsInRow.FirstOrDefault() == null)
                {
                    // Empty row detected. Commit current row (if any)
                    TryAddRow(cbi.Rows, ref currentRow);
                    currentRow = null;
                }
                else
                {
                    // Start new row if needed
                    if (currentRow == null)
                    {
                        currentRow = new LayoutInfo(cbi.PageSize);
                    }
                    currentRow.Blobs.AddRange(blobsInRow);

                    // Advance to test the next empty space
                    // TODO: beware of off-by-1
                    //y = currentRow.Bounds.Bottom - 1;
                }
            }

            // Add row at the end
            TryAddRow(cbi.Rows, ref currentRow);

            FindHeaderAndFooter(ref cbi);

            // Remove header and footer from rows, recompute main content bounds
            if (cbi.Header != null) { cbi.Rows.Remove(cbi.Header); }
            if (cbi.Footer != null) { cbi.Rows.Remove(cbi.Footer); }

            cbi.Bounds = BoundsAroundBlobs(cbi.Rows.SelectMany(x => x.Blobs));
        }

        void FindHeaderAndFooter(ref PageLayoutInfo cbi)
        {
            // KEY HEURISTIC: do most OTHER pages have headers and footers.
            // Difficult to implement at this level, but ought to be reliable.

            // PROBLEM section heading sometimes recognized as header.
            // Height filtering could fix this in theory, but it's bad in other cases

            // FALSE POSITIVES are terrible (worse than missing a header/footer)
            // out006 out038 out044 0ut has a false positive footer of the last line. Heuristic? 
            // Left aligned? By itself, left alignment does not disqualify it

            // Maybe this is a learning problem -- extract features, make probabilistic
            // analysis. Need training data -- set of page pictures labeled with HasHeader/HasFooter

            // Minimum number of rows on a sensible page
            if (cbi.Rows.Count < 2) { return; }

            int lastIdx = cbi.Rows.Count - 1;

            // Exception with small numbers (e.g. 2 elements, upper one much smaller => footer
            if (cbi.Rows.Count <= 3)
            {
                // Check header
                if (cbi.Rows[0].Bounds.Height < cbi.Rows[1].Bounds.Height / 2)
                {
                    cbi.Header = cbi.Rows[0];
                }

                // Check footer
                if (cbi.Rows[lastIdx].Bounds.Height < cbi.Rows[lastIdx - 1].Bounds.Height / 2)
                {
                    cbi.Footer = cbi.Rows[lastIdx];
                }

                return;
            }

            int distanceSum = 0;
            for (int i = 1; i < cbi.Rows.Count; i++)
            {
                distanceSum += DistanceAboveRow(i, cbi);
            }
            float distanceAvg = (float)distanceSum / (cbi.Rows.Count - 1);
            float minDistance = distanceAvg * 1.2f;

            float heightAvg = cbi.Rows.Average(r => (float)r.Bounds.Height);
            float maxHeight = heightAvg * 1.5f;

            // Header
            int headerHeight = cbi.Rows[0].Bounds.Height;
            int headerDistance = DistanceAboveRow(1, cbi);

            if (headerDistance > minDistance &&
                headerHeight < maxHeight)
            {
                cbi.Header = cbi.Rows[0];
            }

            // Footer
            int footerHeight = cbi.Rows[lastIdx].Bounds.Height;
            int footerDistance = DistanceAboveRow(lastIdx, cbi);
            if (footerDistance > minDistance &&
                footerHeight < maxHeight)
            {
                cbi.Footer = cbi.Rows[lastIdx];
            }

            // Note: width heuristic is wrong -- header can be wide
        }

        int DistanceAboveRow(int index, PageLayoutInfo cbi)
        {
            // Lower.Top - Upper.Bottom
            return cbi.Rows[index].Bounds.Top - cbi.Rows[index - 1].Bounds.Bottom;
        }

        void TryAddRow(List<LayoutInfo> rows, ref LayoutInfo currentRow)
        {
            if (currentRow == null) { return; }

            currentRow.Blobs = currentRow.Blobs.Distinct().ToList();
            currentRow.Bounds = BoundsAroundBlobs(currentRow.Blobs);
            rows.Add(currentRow);
        }

        static Rectangle BoundsAroundBlobs(IEnumerable<Blob> blobs)
        {
            if (blobs == null) { throw new ArgumentNullException("blobs"); }
            if (blobs.FirstOrDefault() == null) { return Rectangle.Empty; }

            int left = blobs.Select(b => b.Rectangle.Left).Min();
            int right = blobs.Select(b => b.Rectangle.Right).Max();
            int top = blobs.Select(b => b.Rectangle.Top).Min();
            int bottom = blobs.Select(b => b.Rectangle.Bottom).Max();

            return new Rectangle(left, top, right - left, bottom - top);
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
