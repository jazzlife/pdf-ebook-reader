using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Diagnostics;
using BookReader.Render.Layout;

namespace BookReader.Render
{
    /// <summary>
    /// Analyzes the page layout based on connected blobs contained within it.
    /// </summary>
    class ConnectedBlobLayoutStrategy : IPageLayoutStrategy
    {

        public PageLayout DetectLayoutFromImage(DW<Bitmap> bmp)
        {
            ArgCheck.NotNull(bmp, "bmp");

            PageLayout layout = new PageLayout(bmp.o.Size);

            Blob[] blobs = DetectBlobs(bmp);

            DetectRowBounds(layout, blobs);

            return layout;
        }

        Blob[] DetectBlobs(DW<Bitmap> bmp)
        {
            Invert filter = new Invert();
            filter.ApplyInPlace(bmp.o);

            BlobCounter bc = new BlobCounter();
            bc.BackgroundThreshold = Color.FromArgb(8, 8, 8);

            bc.BlobsFilter = new BlobsFilter(bmp.o.Size);
            bc.FilterBlobs = true;

            bc.ProcessImage(bmp.o);

            // Revert back
            filter.ApplyInPlace(bmp.o);

            return bc.GetObjectsInformation();
        }

        void DetectRowBounds(PageLayout cbi, Blob[] blobs)
        {
            if (blobs.Length == 0) { return; }
            if (cbi.Bounds == Rectangle.Empty) { return; }

            List<LayoutElement> rows = new List<LayoutElement>();

            LayoutElement currentRow = null;
            // Attempt drawing lines between the rows.
            for (int y = cbi.Bounds.Top; y < cbi.Bounds.Bottom; y++)
            {
                Rectangle rowRect = new Rectangle(cbi.Bounds.Left, y, cbi.Bounds.Width, 1);

                var blobsInRow = blobs.Where(b => b.Rectangle.IntersectsWith(rowRect));

                if (blobsInRow.FirstOrDefault() == null)
                {
                    // Empty row detected. Commit current row (if any)
                    TryAddRow(rows, currentRow);
                    currentRow = null;
                }
                else
                {
                    // Start new row if needed
                    if (currentRow == null)
                    {
                        currentRow = new LayoutElement();
                        currentRow.Type = LayoutElementType.Row;
                    }
                    currentRow.Children.AddRange(blobsInRow.Select(x => LayoutElement.NewWord(cbi.PageSize, x.Rectangle)));

                    // Advance to test the next empty space
                    // TODO: beware of off-by-1
                    //y = currentRow.Bounds.Bottom - 1;
                }
            }

            // Add row at the end
            TryAddRow(rows, currentRow);

            FindAndRemoveHeaderAndFooter(cbi, rows);

            cbi.Children = rows;
            cbi.SetBoundsFromNodes(true);
        }

        void FindAndRemoveHeaderAndFooter(PageLayout cbi, List<LayoutElement> rows)
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
            if (rows.Count < 2) { return; }

            int lastIdx = rows.Count - 1;

            LayoutElement header = null;
            LayoutElement footer = null;

            // Exception with small numbers (e.g. 2 elements, upper one much smaller => footer
            if (rows.Count <= 3)
            {
                // Check header
                if (rows[0].UnitBounds.Height < rows[1].UnitBounds.Height / 2)
                {
                    header = rows[0];
                }

                // Check footer
                if (rows[lastIdx].UnitBounds.Height < rows[lastIdx - 1].UnitBounds.Height / 2)
                {
                    footer = rows[lastIdx];
                }

                return;
            }

            float distanceSum = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                distanceSum += DistanceAboveRow(i, rows);
            }
            float distanceAvg = (float)distanceSum / (rows.Count - 1);
            float minDistance = distanceAvg * 1.2f;

            float heightAvg = rows.Average(r => (float)r.UnitBounds.Height);
            float maxHeight = heightAvg * 1.5f;

            // Header
            float headerHeight = rows[0].UnitBounds.Height;
            float headerDistance = DistanceAboveRow(1, rows);

            if (headerDistance > minDistance &&
                headerHeight < maxHeight)
            {
                header = rows[0];
            }

            // Footer
            float footerHeight = rows[lastIdx].UnitBounds.Height;
            float footerDistance = DistanceAboveRow(lastIdx, rows);
            if (footerDistance > minDistance &&
                footerHeight < maxHeight)
            {
                footer = rows[lastIdx];
            }

            // Note: width heuristic is wrong -- header can be wide

            // Remove header and footer from rows, recompute main content bounds
            if (header != null) { rows.Remove(header); }
            if (footer != null) { rows.Remove(footer); }
        }

        float DistanceAboveRow(int index, List<LayoutElement> rows)
        {
            // Lower.Top - Upper.Bottom
            return rows[index].UnitBounds.Top - rows[index - 1].UnitBounds.Bottom;
        }

        void TryAddRow(List<LayoutElement> rows, LayoutElement currentRow)
        {
            if (currentRow == null) { return; }

            currentRow.Children = currentRow.Children.Distinct().ToList();
            currentRow.SetBoundsFromNodes(false);
            rows.Add(currentRow);
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

        public PageLayout DetectLayoutFromBook(IBookContent book, int pageNum)
        {
            return null;
        }
    }
}
