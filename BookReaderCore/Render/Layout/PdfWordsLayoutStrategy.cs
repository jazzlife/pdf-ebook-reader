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
        public PageLayoutInfo DetectLayoutFromImage(DW<Bitmap> physicalPage)
        {
            return null;
        }

        public PageLayoutInfo DetectLayoutFromBook(ScreenBook book, int pageNum)
        {
            PdfBookPageProvider pdfPageProvider = book.BookProvider.o as PdfBookPageProvider;
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

                PageLayoutInfo layout = DetectLayout(pdfDoc, pdfPage);
                return layout;
            }
            finally
            {
                // Restore doc state
                pdfDoc.o.CurrentPage = oldPageNum;
            }
        }

        PageLayoutInfo DetectLayout(DW<PDFWrapper> doc, PDFPage page)
        {
            Size pageSize = new Size(doc.o.PageWidth, doc.o.PageHeight);
            PageLayoutInfo layout = new PageLayoutInfo(pageSize);

            // Detect bounds
            layout.Words.AddRange(page.WordList);

            // BUG in PDF library -- first word not given
            // This is bad, will affect search

            // WORKAROUND: bug in PDF library -- last word is 0,0
            if (layout.Words.Count > 1)
            {
                // last word is 0,0
                var last = layout.Words.Last();
                if (last.Bounds.IsEmpty) { layout.Words.RemoveLast(); }
            }

            if (layout.Words.Count > 1)
            {
                int left = layout.Words.Min(x => x.Bounds.Left);
                int width = layout.Words.Max(x => x.Bounds.Right) - left;

                int top = layout.Words.Min(x => x.Bounds.Top);
                int height = layout.Words.Max(x => x.Bounds.Bottom) - top;

                layout.Bounds = new Rectangle(left, top, width, height);
            }
            else
            {
                layout.Bounds = new Rectangle(Point.Empty, pageSize);
            }

            // TODO: detect rows

            // TODO: detect header/footer (if any)

            return layout;
        }

    }
}
