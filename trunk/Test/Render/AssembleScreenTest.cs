using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PdfBookReader.Render;
using PdfBookReader.Utils;
using System.Drawing;
using PdfBookReader.Model;

namespace PdfBookReaderTest.Render
{
    [TestFixture]
    public class AssembleScreenTest
    {
        Size ScreenSize = new Size(100, 200);
        int PageHeight 
        {
            get { return ((TestBookProvider)bookProv.o).PageHeight; }
            set { ((TestBookProvider)bookProv.o).PageHeight = value; }
        }

        DW<IPageSource> src;
        DW<IBookProvider> bookProv; 

        AssembleScreenAlgorithm algCur; 
        AssembleScreenAlgorithm algNext; 
        AssembleScreenAlgorithm algPrevious;

        [SetUp]
        public void SetUp()
        {
            src = DW.Wrap<IPageSource>(new TestPageSource());
            bookProv = DW.Wrap<IBookProvider>(new TestBookProvider());

            algCur = new AssembleCurrentScreenAlgorithm(src, bookProv);
            algNext = new AssembleNextScreenAlgorithm(src, bookProv);
            algPrevious = new AssemblePreviousScreenAlgorithm(src, bookProv);
        }

        [Test]
        public void AssembleCurrent_FirstPage()
        {
            // Test with null position
            PageHeight = 60;
            PositionInBook pos = PositionInBook.FromPhysicalPage(1, bookProv.o.PageCount);

            Assert.IsTrue(algCur.CanApply(pos, ScreenSize));
            var pages = algCur.AssembleScreen(ref pos, ScreenSize);

            // Check
            Assert.AreEqual(1, pos.PageNum);
            CollectionAssert.AreEqual(
                A(1, 2, 3, 4), pages.Select(x => x.PageNum), "numbers");
            CollectionAssert.AreEqual(
                A(0, 60, 120, 180), pages.Select(x => x.TopOnScreen), "tops");
        }

        [Test]
        public void AssembleNext_FromFirstPage_ShortPage()
        {
            // Test with null position
            PageHeight = 60;
            PositionInBook pos = PositionInBook.FromPhysicalPage(1, bookProv.o.PageCount);

            // Current
            Assert.IsTrue(algNext.CanApply(pos, ScreenSize));
            var pages = algNext.AssembleScreen(ref pos, ScreenSize);

            // Height 60, so 2nd page starts at page=4 top =-20
           Assert.AreEqual(4, pos.PageNum);
            CollectionAssert.AreEqual(
                A(4, 5, 6, 7), pages.Select(x => x.PageNum), "numbers");
            CollectionAssert.AreEqual(
                A(-20, 40, 100, 160), pages.Select(x => x.TopOnScreen), "tops");

            // Do one more
            Assert.IsTrue(algNext.CanApply(pos, ScreenSize));
            pages = algNext.AssembleScreen(ref pos, ScreenSize);

            Assert.AreEqual(7, pos.PageNum);
            CollectionAssert.AreEqual(
                A(7, 8, 9, 10), pages.Select(x => x.PageNum), "numbers");
            CollectionAssert.AreEqual(
                A(-40, 20, 80, 140), pages.Select(x => x.TopOnScreen), "tops");
            
            // Bounds-aligned, important edge case
            Assert.IsTrue(algNext.CanApply(pos, ScreenSize), "CanApply, bounds aligned");
            pages = algNext.AssembleScreen(ref pos, ScreenSize);

            Assert.AreEqual(11, pos.PageNum);
            CollectionAssert.AreEqual(
                A(11, 12, 13, 14), pages.Select(x => x.PageNum), "numbers");
            CollectionAssert.AreEqual(
                A(0, 60, 120, 180), pages.Select(x => x.TopOnScreen), "tops");
        }

        [Test]
        public void AssembleNext_FromFirstPage_TallPage()
        {
            // Test with null position
            PageHeight = 300;
            PositionInBook pos = PositionInBook.FromPhysicalPage(1, bookProv.o.PageCount);

            // Current
            Assert.IsTrue(algNext.CanApply(pos, ScreenSize));
            var pages = algNext.AssembleScreen(ref pos, ScreenSize);

            // Height 60, so 2nd page starts at page=4 top =-20
            Assert.AreEqual(1, pos.PageNum);
            CollectionAssert.AreEqual(
                A(1, 2), pages.Select(x => x.PageNum), "numbers");
            CollectionAssert.AreEqual(
                A(-200, 100), pages.Select(x => x.TopOnScreen), "tops");

            // Do one more, bounds - aligned
            Assert.IsTrue(algNext.CanApply(pos, ScreenSize));
            pages = algNext.AssembleScreen(ref pos, ScreenSize);

            Assert.AreEqual(2, pos.PageNum);
            CollectionAssert.AreEqual(
                A(2), pages.Select(x => x.PageNum), "numbers");
            CollectionAssert.AreEqual(
                A(-100), pages.Select(x => x.TopOnScreen), "tops");
        }

        [Test]
        public void AssembleNext_FromMiddle_PageSameAsScreen()
        {
            // Test with null position
            PageHeight = ScreenSize.Height;
            PositionInBook pos = PositionInBook.FromPhysicalPage(10, bookProv.o.PageCount);

            // Do a few
            for (int i = 11; i < 30; i++)
            {
                Assert.IsTrue(algNext.CanApply(pos, ScreenSize));
                var pages = algNext.AssembleScreen(ref pos, ScreenSize);

                Assert.AreEqual(i, pos.PageNum);
                CollectionAssert.AreEqual(
                    A(i), pages.Select(x => x.PageNum), "numbers");
                CollectionAssert.AreEqual(
                    A(0), pages.Select(x => x.TopOnScreen), "tops");
            }

            // Pages not starting from zero
            // Start halfway down first page
            pos = PositionInBook.FromPositionUnit(0.005f, bookProv.o.PageCount);

            for (int i = 1; i < 20; i++)
            {
                Assert.IsTrue(algNext.CanApply(pos, ScreenSize));
                var pages = algNext.AssembleScreen(ref pos, ScreenSize);

                Assert.AreEqual(i+1, pos.PageNum);
                CollectionAssert.AreEqual(
                    A(i+1, i+2), pages.Select(x => x.PageNum), "numbers");
                CollectionAssert.AreEqual(
                    A(-100, 100), pages.Select(x => x.TopOnScreen), "tops");
            }
        }

        [Test]
        public void AssemblePrevious_ShortPage()
        {
            // Test with null position
            PageHeight = 60;
            PositionInBook pos = PositionInBook.FromPhysicalPage(1, bookProv.o.PageCount);

            // Current
            Assert.IsFalse(algPrevious.CanApply(pos, ScreenSize));

            // Go forward a bit
            algNext.AssembleScreen(ref pos, ScreenSize);
            algNext.AssembleScreen(ref pos, ScreenSize);
            algNext.AssembleScreen(ref pos, ScreenSize);
            Assert.AreEqual(11, pos.PageNum);

            // Back one
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            var pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            Assert.AreEqual(7, pos.PageNum);
            CollectionAssert.AreEqual(
                A(7, 8, 9, 10), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(-40, 20, 80, 140), pages.Select(x => x.TopOnScreen).Reverse(), "tops");

            // One more
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            // Height 60, so 2nd page starts at page=4 top =-20
            Assert.AreEqual(4, pos.PageNum);
            CollectionAssert.AreEqual(
                A(4, 5, 6, 7), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(-20, 40, 100, 160), pages.Select(x => x.TopOnScreen).Reverse(), "tops");
            
            // Back at first page
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            // Height 60, so 2nd page starts at page=4 top =-20
            Assert.AreEqual(1, pos.PageNum);
            CollectionAssert.AreEqual(
                A(1, 2, 3, 4), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(0, 60, 120, 180), pages.Select(x => x.TopOnScreen).Reverse(), "tops");

            Assert.IsFalse(algPrevious.CanApply(pos, ScreenSize));
        }

        [Test]
        public void AssemblePrevious_TallPage()
        {
            // Test with null position
            PageHeight = 300;
            PositionInBook pos = PositionInBook.FromPhysicalPage(1, bookProv.o.PageCount);

            // Current
            Assert.IsFalse(algPrevious.CanApply(pos, ScreenSize));

            // Go forward a bit
            algNext.AssembleScreen(ref pos, ScreenSize);
            algNext.AssembleScreen(ref pos, ScreenSize);
            algNext.AssembleScreen(ref pos, ScreenSize);
            Assert.AreEqual(3, pos.PageNum);
            Assert.AreEqual(0, pos.GetTopOnScreen(PageHeight));

            // Back one
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            var pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            Assert.AreEqual(2, pos.PageNum);
            CollectionAssert.AreEqual(
                A(2), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(-100), pages.Select(x => x.TopOnScreen).Reverse(), "tops");

            // One more
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            // Height 60, so 2nd page starts at page=4 top =-20
            Assert.AreEqual(1, pos.PageNum);
            CollectionAssert.AreEqual(
                A(1,2), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(-200, 100), pages.Select(x => x.TopOnScreen).Reverse(), "tops");

            // Back at first page
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            // Height 60, so 2nd page starts at page=4 top =-20
            Assert.AreEqual(1, pos.PageNum);
            CollectionAssert.AreEqual(
                A(1), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(0), pages.Select(x => x.TopOnScreen).Reverse(), "tops");

            Assert.IsFalse(algPrevious.CanApply(pos, ScreenSize));
        }

        [Test]
        public void AssemblePrevious_FirstPageEdgeCase()
        {
            // Test with null position
            PageHeight = ScreenSize.Height;
            PositionInBook pos = PositionInBook.FromPhysicalPage(2, bookProv.o.PageCount, -100, PageHeight);
            Assert.AreEqual(1.5f, pos.Position);

            // One page back
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            var pages = algPrevious.AssembleScreen(ref pos, ScreenSize);

            Assert.AreEqual(0.5f, pos.Position);
            CollectionAssert.AreEqual(
                A(1, 2), pages.Select(x => x.PageNum).Reverse(), "numbers");
            CollectionAssert.AreEqual(
                A(-100, 100), pages.Select(x => x.TopOnScreen).Reverse(), "tops");

            // Special case: going back would go *past* the first page start 
            // Allow this in the algorithm, but position does not go below zero
            Assert.IsTrue(algPrevious.CanApply(pos, ScreenSize));
            pages = algPrevious.AssembleScreen(ref pos, ScreenSize);
            CollectionAssert.AreEqual(
                A(1), pages.Select(x => x.PageNum).Reverse(), "numbers - first page");
            CollectionAssert.AreEqual(
                A(100), pages.Select(x => x.TopOnScreen).Reverse(), "tops - first page");

            // This *should* be -0.005 normally, but we snap to the start.
            Assert.AreEqual(0, pos.Position);


            Assert.IsFalse(algPrevious.CanApply(pos, ScreenSize));
        }

        [Test]
        public void CanApply_Next()
        {
            // Test with null position
            PageHeight = ScreenSize.Height;

            // top of last page (exact)
            PositionInBook pos = PositionInBook.FromPhysicalPage(bookProv.o.PageCount, bookProv.o.PageCount);
            Assert.IsFalse(algNext.CanApply(pos, ScreenSize), "last page exactly full");

            // slight bit before - true
            PositionInBook posBefore = PositionInBook.FromPositionUnit(pos.PositionUnit - 0.001f, bookProv.o.PageCount);
            Assert.IsTrue(algNext.CanApply(posBefore, ScreenSize), "bit before");

            // slight bit after - false
            PositionInBook posAfter = PositionInBook.FromPositionUnit(pos.PositionUnit + 0.001f, bookProv.o.PageCount);
            Assert.IsFalse(algNext.CanApply(posAfter, ScreenSize), "bit after");
        }

        [Test]
        public void CanApply_Previous()
        {
            // Test with null position
            PageHeight = ScreenSize.Height;

            // exactly at the start
            PositionInBook pos = PositionInBook.FromPhysicalPage(1, bookProv.o.PageCount);
            Assert.IsFalse(algPrevious.CanApply(pos, ScreenSize), "exactly at 0");

            // slight bit after - false
            PositionInBook posAfter = PositionInBook.FromPositionUnit(pos.PositionUnit + 0.001f, bookProv.o.PageCount);
            Assert.IsTrue(algPrevious.CanApply(posAfter, ScreenSize), "bit after");

        }

        // Nicer syntax
        static T[] A<T>(params T[] args) { return args; }        
    }

    class TestBookProvider : IBookProvider
    {
        public int PageHeight { get; set; }
        public int PageCount { get; set; }
        public string BookFilename { get; set; }

        public TestBookProvider(String filename = "foo", int pageCount = 100, int pageHeight = 60)
        {
            BookFilename = filename;
            PageCount = pageCount;
            PageHeight = pageHeight;
        }

        public DW<Bitmap> RenderPageImage(int pageNum, Size maxSize, RenderQuality quality = RenderQuality.HighQuality)
        {
            DW<Bitmap> image = DW.Wrap(new Bitmap(maxSize.Width, PageHeight));
            return image;
        }

        public void Dispose() { }
    }

    class TestPageSource : IPageSource
    {
        public IPageLayoutStrategy LayoutStrategy { get; set; }
             
        public TestPageSource()
        {
            LayoutStrategy = new BlankLayoutStrategy();
        }

        public Page GetPage(int pageNum, Size screenSize, DW<IBookProvider> bookProvider)
        {
            DW<Bitmap> image = bookProvider.o.RenderPageImage(pageNum, new Size(screenSize.Width, int.MaxValue));
            PageLayoutInfo layout = LayoutStrategy.DetectLayout(image);
            return new Page(pageNum, image, layout);
        }

        public void Dispose() { }
    }
}
