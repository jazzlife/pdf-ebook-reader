using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Render.Layout;
using BookReader.Model;
using BookReader.Render.Cache;
using BookReader.Utils;

namespace BookReader.Render.BookFormats
{
    class PdfBookContent : BookContentBase
    {
        IPageLayoutStrategy _layoutStrategy;

        public PdfBookContent(Book book, DW<PageImageCache> cache)
            : base(book, cache)
        {
            _layoutStrategy = RenderFactory.Default.GetLayoutStrategy();
        }

        protected override PageLayout CreatePageLayout(int pageNum)
        {
            return _layoutStrategy.DetectLayoutFromBook(this, pageNum);
        }
    }
}
