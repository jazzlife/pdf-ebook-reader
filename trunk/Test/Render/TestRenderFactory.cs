using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Render;
using BookReader.Render.Layout;
using BookReader.Render.Cache;
using BookReader.Utils;

namespace BookReaderTest.Render
{
    class TestRenderFactory : BlankRenderFactory
    {
        public static IPageLayoutStrategy LayoutStrategy = new BlankLayoutStrategy();
        public override IPageLayoutStrategy GetLayoutStrategy()
        {
            return LayoutStrategy;
        }

        public static DW<IBookProvider> BookProvider = DW.Wrap<IBookProvider>(new BlankBookProvider());
        public override DW<IBookProvider> GetBookProvider(string file)
        {
            return BookProvider;
        }

        public static DW<IPageSource> PageSource = DW.Wrap<IPageSource>(new BlankPageSource());
        public override DW<IPageSource> GetPageSource(IPageCacheContextManager contextManager)
        {
            return PageSource;
        }
    }

    class BlankBookProvider : IBookProvider
    {
        public int PageHeight { get; set; }
        public int PageCount { get; set; }
        public string BookFilename { get; set; }

        public BlankBookProvider(String filename = "foo", int pageCount = 100, int pageHeight = 60)
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

    class BlankPageSource : IPageSource
    {
        public IPageLayoutStrategy LayoutStrategy { get; set; }

        public BlankPageSource()
        {
            LayoutStrategy = new BlankLayoutStrategy();
        }

        public Page GetPage(int pageNum, Size screenSize, ScreenBook screenBook)
        {
            DW<Bitmap> image = screenBook.BookProvider.o.RenderPageImage(pageNum, new Size(screenSize.Width, int.MaxValue));
            PageLayoutInfo layout = LayoutStrategy.DetectLayoutFromImage(image);
            return new Page(pageNum, image, layout);
        }

        public void Dispose() { }
    }
}
