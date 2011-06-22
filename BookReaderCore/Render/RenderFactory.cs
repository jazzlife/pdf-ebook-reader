using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Render.Cache;
using BookReader.Utils;
using BookReader.Render.Layout;

namespace BookReader.Render
{

    abstract class RenderFactory
    {
        public static RenderFactory ConcreteFactory = new DefaultRenderFactory();

        public abstract IPageLayoutStrategy GetLayoutStrategy();
        public abstract DW<IBookProvider> GetBookProvider(String file);
        public abstract DW<IPageSource> GetPageSource(IPageCacheContextManager contextManager);

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
            return new PdfWordsLayoutStrategy();

            // return new ConnectedBlobLayoutStrategy();
        }

        public override DW<IBookProvider> GetBookProvider(String file)
        {
            return DW.Wrap<IBookProvider>(new PdfBookPageProvider(file));
        }

        public override DW<IPageSource> GetPageSource(IPageCacheContextManager contextManager)
        {
            if (Options.Current.NoCache)
            {
                logger.Warn("GetPageSource: NO CACHE, returning simple logger");
                return DW.Wrap<IPageSource>(new SimplePageSource());
            }

            return DW.Wrap<IPageSource>(new CachedPageSource(contextManager));
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
    }

    class BlankRenderFactory : RenderFactory
    {
        public override IPageLayoutStrategy GetLayoutStrategy()
        {
            throw new NotImplementedException();
        }

        public override DW<IBookProvider> GetBookProvider(string file)
        {
            throw new NotImplementedException();
        }

        public override DW<IPageSource> GetPageSource(IPageCacheContextManager contextManager)
        {
            throw new NotImplementedException();
        }

        protected override PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy()
        {
            throw new NotImplementedException();
        }
    }


}
