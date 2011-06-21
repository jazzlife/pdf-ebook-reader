using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Render.Cache;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{

    abstract class RenderFactory
    {
        public static RenderFactory ConcreteFactory = new DefaultRenderFactory();

        public abstract IPageLayoutStrategy GetLayoutStrategy();
        public abstract DW<IBookProvider> GetBookProvider(String file);
        public abstract DW<IPageSource> GetPageSource(IPageCacheContextManager contextManager);

        public abstract IPageRetainPolicy GetPageCachePolicyMemory();
        protected abstract PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy();

        public IPageRetainPolicy GetPageCachePolicyDisk()
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
        public override IPageLayoutStrategy GetLayoutStrategy()
        {
            return new ConnectedBlobLayoutStrategy();
        }

        public override DW<IBookProvider> GetBookProvider(String file)
        {
            return DW.Wrap<IBookProvider>(new PdfBookPageProvider(file));
        }

        public override DW<IPageSource> GetPageSource(IPageCacheContextManager contextManager)
        {
            return DW.Wrap<IPageSource>(new CachedPageSource(contextManager));
        }

        public override IPageRetainPolicy GetPageCachePolicyMemory()
        {
            return new PagePrefetchAndRetainPolicy()
            {
                Retain_InCurrentBookAfter = 6,
                Retain_InCurrentBookBefore = 2,
                Retain_InOtherBookAfter = 0,
                Retain_InOtherBookBefore = 0,
                Retain_Initial = 0,
                OtherItemsToKeepCount = 0
            };
        }

        protected override PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy()
        {
            return new PagePrefetchAndRetainPolicy()
                {
                    Retain_InCurrentBookAfter = 20,
                    Retain_InCurrentBookBefore = 4,
                    Retain_InOtherBookAfter = 6,
                    Retain_InOtherBookBefore = 2,
                    Retain_Initial = 5,
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

        public override IPageRetainPolicy GetPageCachePolicyMemory()
        {
            throw new NotImplementedException();
        }

        protected override PagePrefetchAndRetainPolicy GetGeneralPrefetchPolicy()
        {
            throw new NotImplementedException();
        }
    }


}
