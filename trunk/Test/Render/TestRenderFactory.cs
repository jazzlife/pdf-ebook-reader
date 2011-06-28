using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Render;
using BookReader.Render.Layout;
using BookReader.Render.Cache;
using BookReader.Utils;
using BookReader.Model;

namespace BookReaderTest.Render
{
    class TestRenderFactory : BlankRenderFactory
    {
        public static IPageLayoutStrategy LayoutStrategy = new BlankLayoutStrategy();
        public override IPageLayoutStrategy GetLayoutStrategy()
        {
            return LayoutStrategy;
        }

        public static DW<IBookProvider> BookProvider = DW.Wrap<IBookProvider>(new BlankBookProvider(new Size(100,60)));
        public override DW<IBookProvider> NewBookProvider(string file)
        {
            return BookProvider;
        }

        public static DW<IBookContent> BookContent = DW.Wrap<IBookContent>(new BlankBookContent());
        public override DW<IBookContent> NewBookContent(Book book, DW<PageImageCache> cache)
        {
            return BookContent;
        }

    }

    class BlankBookProvider : IBookProvider
    {
        public Size PageSize { get; set; }
        public int PageCount { get; set; }
        public string BookFilename { get; set; }

        public BlankBookProvider(Size pageSize, String filename = "foo", int pageCount = 100)
        {
            BookFilename = filename;
            PageCount = pageCount;
            PageSize = pageSize;
        }

        public Bitmap RenderPageImage(int pageNum, Size maxSize, RenderQuality quality = RenderQuality.HighQuality)
        {
            if (maxSize == Size.Empty) { return new Bitmap(PageSize.Width, PageSize.Height); }
            return new Bitmap(maxSize.Width, PageSize.Height);
        }

        public void Dispose() { }

    }

    class BlankBookContent : IBookContent
    {
        public IPageLayoutStrategy LayoutStrategy { get; set; }

        public BlankBookContent()
        {
            Book = new Book("blank");
            LayoutStrategy = new BlankLayoutStrategy();

            PageCount = 100;

            if (Book.CurrentPosition == null)
            {
                Book.CurrentPosition = PositionInBook.FromPhysicalPage(1, PageCount);
            }
        }

        public Book Book { get; private set; }

        public PageLayout GetPageLayout(int pageNum)
        {
            return LayoutStrategy.DetectLayoutFromBook(this, pageNum);
        }

        public PageImage GetPageImage(int pageNum, int screenWidth)
        {
            var layout = GetPageLayout(pageNum);
            var bmp = BookProvider.o.RenderPageImage(pageNum, new Size(screenWidth, int.MaxValue));
            var key = new PageKey(Book.Id, pageNum, screenWidth);
            return new PageImage(key, bmp);
        }

        public PositionInBook Position
        {
            get { return Book.CurrentPosition; }
            set { Book.CurrentPosition = value; }
        }

        public int PageCount { get; set; }

        public DW<IBookProvider> BookProvider
        {
            get { return RenderFactory.Default.NewBookProvider(Book.Filename); }
        }

        public void Dispose() { }
    }
}
