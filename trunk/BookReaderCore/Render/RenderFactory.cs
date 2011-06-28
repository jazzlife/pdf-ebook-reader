using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Render.Cache;
using BookReader.Utils;
using BookReader.Render.Layout;
using BookReader.Properties;
using BookReader.Render.BookFormats;
using BookReader.Model;

namespace BookReader.Render
{

    abstract class RenderFactory
    {
        public static RenderFactory Default = new DefaultRenderFactory();

        public abstract IPageLayoutStrategy GetLayoutStrategy();
        public abstract DW<IBookProvider> NewBookProvider(String file);
        public abstract DW<IBookContent> NewBookContent(Book book, DW<PageImageCache> cache = null);

        protected abstract PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy();

        public IPageRetainPolicy GetPageCachePolicy()
        {
            return GetGeneralPrefetchPolicy();
        }

        public IPagePrefetchPolicy GetPrefetchPolicy()
        {
            return GetGeneralPrefetchPolicy();
        }
    }

    class DefaultRenderFactory : RenderFactory
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override IPageLayoutStrategy GetLayoutStrategy()
        {
            //return new BlankLayoutStrategy();
            return new PdfWordsLayoutStrategy();

            // return new ConnectedBlobLayoutStrategy();
        }

        public override DW<IBookProvider> NewBookProvider(String file)
        {
            return DW.Wrap<IBookProvider>(new PdfBookProvider(file));
        }

        protected override PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy()
        {
            return new PagePrefetchAndRetainPolicy()
                {
                    Retain_InCurrentBookAfter = 10,
                    Retain_InCurrentBookBefore = 4,
                    Retain_InOtherBookAfter = 6,
                    Retain_InOtherBookBefore = 2,
                    Retain_Initial = 6,
                    OtherItemsToKeepCount = 100
                };
        }


        public override DW<IBookContent> NewBookContent(Book book, DW<PageImageCache> cache)
        {
            return DW.Wrap<IBookContent>(new PdfBookContent(book, cache));
        }

    }

    class BlankRenderFactory : RenderFactory
    {
        public override IPageLayoutStrategy GetLayoutStrategy()
        {
            throw new NotImplementedException();
        }

        public override DW<IBookProvider> NewBookProvider(string file)
        {
            throw new NotImplementedException();
        }

        protected override PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy()
        {
            throw new NotImplementedException();
        }

        public override DW<IBookContent> NewBookContent(Book book, DW<PageImageCache> cache)
        {
            throw new NotImplementedException();
        }
    }


}
