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
            layout.Nodes.AddRange(page.WordList.Select(x => new LayoutElement(x.Bounds, x.Word)));

            // Strange bug -- if doing the following, first word is missing and last word is blank.
            // However, with LINQ query above it's fine

            //List<PDFTextWord> ws = new List<PDFTextWord>();
            //ws.AddRange(page.WordList);

            if (layout.Nodes.Count > 0)
            {
                layout.SetBoundsFromNodes(true);

                int expandWidth = (0.05 * layout.Bounds.Width).Round();

                Rectangle bounds = layout.Bounds;
                bounds.X -= expandWidth / 2;
                bounds.Width += expandWidth;

                layout.Bounds = bounds;

                // TODO: expand by width a bit (to prevent cutting off words which
                // may not be recognized properly.
            }

            // error checking
            if (layout.Bounds.X < 0 || layout.Bounds.Y < 0 ||
                layout.Bounds.Width <= 0 || layout.Bounds.Height <= 0)
            {
                logger.Error("Wrong bounds: " + layout.Bounds + " images: " + page.ImagesCount);

                int height = page.ImagesCount > 0 ? pageSize.Height : 100;

                layout.Bounds = new Rectangle(Point.Empty, new Size(pageSize.Width, height));
            }

            // TODO: detect rows

            // TODO: detect header/footer (if any)

            return layout;
        }

        /*
        IEnumerable<LayoutElement> DetectColumns(IEnumerable<LayoutElement> words)
        {
            List<LayoutElement> columns = new List<LayoutElement>();

            LayoutElement curCol = null;

            int prevBottom = 0;
            foreach (var word in words)
            {
                if (curCol == null)
                {
                }

                if (curCol == null || word.Bounds.Top < prevBottom)
                {
                    // start new column
                    curCol = new LayoutElement(LayoutElementType.Column);
                    columns.Add(curCol);
                }

            }



        }
        */


    }
}
