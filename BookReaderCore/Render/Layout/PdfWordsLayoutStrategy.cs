using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;
using PDFLibNet;

namespace BookReader.Render.Layout
{
    class PdfWordsLayoutStrategy : IPageLayoutStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public PageLayout DetectLayoutFromImage(DW<Bitmap> physicalPage)
        {
            return null;
        }

        public PageLayout DetectLayoutFromBook(IBookContent book, int pageNum)
        {
            Console.WriteLine("book: " + book.Book.Title + " page: " + pageNum);

            PdfBookProvider pdfPageProvider = book.BookProvider.o as PdfBookProvider;
            if (pdfPageProvider == null)
            {
                throw new InvalidOperationException("SreenBook.BookProvider must be a PdfBookPageProvider for this LayoutAnalyzer");
            }

            DW<PDFWrapper> pdfDoc = pdfPageProvider.InternalPdfWrapper;


            PDFPage pdfPage = pdfDoc.o.Pages[pageNum];
            if (pdfPage == null) { throw new InvalidOperationException("No PDFPage pageNum=" + pageNum); }


            // Save doc state
            int oldPageNum = pdfDoc.o.CurrentPage;
            try
            {
                pdfDoc.o.CurrentPage = pageNum;

                PageLayout layout = DetectLayout(pdfDoc, pdfPage);
                return layout;
            }
            finally
            {
                // Restore doc state
                pdfDoc.o.CurrentPage = oldPageNum;
            }
        }



        PageLayout DetectLayout(DW<PDFWrapper> doc, PDFPage page)
        {
            Size pageSize = new Size(doc.o.PageWidth, doc.o.PageHeight);
            PageLayout layout = new PageLayout(pageSize);

            // Get text
            // TODO: check how text is split in multicolumn case -- is this the method with correct options (flow, not physical)
            layout.Text = page.Text;

            var words = new List<LayoutElement>();
            var nonEmptyWords = page.WordList
                .Where(x => !x.Bounds.IsEmpty && !x.Word.IsEmpty())
                .Select(x => LayoutElement.NewWord(pageSize, x.Bounds, x.Word));

            words.AddRange(nonEmptyWords);

            // Detect rows and columns
            var rows = words.Split(StartsNewRow).Select(ws => LayoutElement.NewRow(ws, LayoutElementType.Row));
            var cols = rows.Split(StartsNewColumn).Select(rs => LayoutElement.NewRow(rs, LayoutElementType.Column));

            // TODO: detect header/footer
            layout.Children.AddRange(cols);

            // Strange bug -- if doing the following, first word is missing and last word is blank.
            // However, with LINQ query above it's fine

            //List<PDFTextWord> ws = new List<PDFTextWord>();
            //ws.AddRange(page.WordList);

            if (layout.Children.Count > 0)
            {
                layout.SetBoundsFromNodes(true);

                /*
                // expand by width a bit (to prevent cutting off words which
                // may not be recognized properly.
                int expandWidth = (0.05 * layout.Bounds.Width).Round();

                RectangleF expBounds = layout.UnitBounds;
                expBounds.X -= expandWidth / 2;
                expBounds.Width += expandWidth;

                if (expBounds.X <= 1 && expBounds.Width <= 1)
                {
                    layout.UnitBounds = expBounds;
                }
                 */
            }

            // error checking
            if (layout.Bounds.X < 0 || layout.Bounds.Y < 0 ||
                layout.Bounds.Width <= 0 || layout.Bounds.Height <= 0)
            {
                logger.Error("Wrong bounds: " + layout.Bounds + " images: " + page.ImagesCount);

                float height = page.ImagesCount > 0 ? 1 : 0.1f;

                layout.UnitBounds = new RectangleF(0,0, 1, height);
            }

            // TODO: detect rows

            // TODO: detect header/footer (if any)

            return layout;
        }

        bool StartsNewRow(LayoutElement prev, LayoutElement cur)
        {
            // Normal case: next row
            // QQ: do rows ever overlap?
            if (prev.UnitBounds.Bottom <= cur.UnitBounds.Top) { return true; }
            
            // Special case: next column
            if (prev.UnitBounds.Top >= cur.UnitBounds.Bottom) { return true; }

            return false;
        }

        bool StartsNewColumn(LayoutElement a, LayoutElement b)
        {
            return b.UnitBounds.Bottom <= a.UnitBounds.Top &&
                b.UnitBounds.Left > a.UnitBounds.Right;
        }



    }
}
