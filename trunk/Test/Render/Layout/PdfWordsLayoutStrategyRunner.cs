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

        void CreateLayout(String bookName)
        {
            Console.WriteLine(bookName);
            Book book = new Book(TestConst.GetPdfFile(bookName));
            ScreenBook sBook = new ScreenBook(book, new Size(800, 600));

            for (int i = 10; i < Math.Min(16, sBook.BookProvider.o.PageCount); i++)
            {
                CreateLayout(sBook, i);
            }
        }
        void CreateLayout(ScreenBook sBook, int pageNum)
        {
            Console.WriteLine(pageNum);


            IPageLayoutStrategy alg = new PdfWordsLayoutStrategy();
            PageLayoutInfo layout = alg.DetectLayoutFromBook(sBook, pageNum);

            DW<Bitmap> page = sBook.BookProvider.o.RenderPageImage(pageNum, layout.PageSize);
            DW<Bitmap> newPage = layout.Debug_DrawLayout(page);

            newPage.o.Save(TestConst.GetOutFile(sBook.Book.Filename, "_p" + pageNum + ".png"), ImageFormat.Png);
        }
    }
}
