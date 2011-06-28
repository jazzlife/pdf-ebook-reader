using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using BookReader.Render;
using BookReader.Render.Layout;
using System.Drawing;
using BookReader.Model;
using BookReaderTest.TestUtils;
using BookReader.Utils;
using System.Drawing.Imaging;
using PDFLibNet;
using BookReader.Render.BookFormats;

namespace BookReaderTest.Render.Layout
{
    [TestFixture]
    public class PdfWordsLayoutStrategyRunner
    {
        [Test]
        public void CreateLayout()
        {
            TestConst.GetAllPdfFiles().ForEach( x => CreateLayout(x) );


        }

        PDFWrapper GetPdfWrapper(IBookContent bc)
        {
            return ((PdfBookProvider)bc.BookProvider.o).InternalPdfWrapper.o;
        }

        void CreateLayout(String bookName)
        {
            Console.WriteLine(bookName);
            Book book = new Book(TestConst.GetPdfFile(bookName));
            IBookContent bookC = new PdfBookContent(book, null);

            for (int i = 10; i < Math.Min(16, bookC.BookProvider.o.PageCount); i++)
            {
                CreateLayout(bookC, i);
            }
        }
        void CreateLayout(IBookContent bookC, int pageNum)
        {
            Console.WriteLine(pageNum);
            IPageLayoutStrategy alg = new PdfWordsLayoutStrategy();
            PageLayout layout = alg.DetectLayoutFromBook(bookC, pageNum);

            DW<Bitmap> page = DW.Wrap(bookC.BookProvider.o.RenderPageImage(pageNum, layout.PageSize));
            DW<Bitmap> newPage = layout.Debug_DrawLayout(page);

            GetPdfWrapper(bookC).ExportText(TestConst.GetOutFile(bookC.Book.Filename, "_p" + pageNum + ".txt"), pageNum, pageNum, false, false);

            newPage.o.Save(TestConst.GetOutFile(bookC.Book.Filename, "_p" + pageNum + ".png"), ImageFormat.Png);
        }
    }
}
