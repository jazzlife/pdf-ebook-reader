using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;
using PDFLibNet;

namespace PdfBookReader.Render.Layout
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
            var words = page.WordList;
            if (words.Any())
            {
                int left = words.Min(x => x.Bounds.Left);
                int width = words.Max(x => x.Bounds.Right) - left;

                int top = words.Min(x => x.Bounds.Top);
                int height = words.Max(x => x.Bounds.Bottom) - top;

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
