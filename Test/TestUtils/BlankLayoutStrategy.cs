using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;

namespace BookReader.Render.Layout
{
    /// <summary>
    /// Full physical page area as layout, no header/footer. 
    /// </summary>
    class BlankLayoutStrategy : IPageLayoutStrategy
    {
        public PageLayout DetectLayoutFromImage(DW<Bitmap> physicalPage)
        {
            PageLayout pli = new PageLayout(physicalPage.o.Size);
            return pli;
        }

        public PageLayout DetectLayoutFromBook(IBookContent book, int pageNum)
        {
            // This is hacky, but OK for now
            Bitmap b = book.BookProvider.o.RenderPageImage(pageNum, Size.Empty);
            PageLayout pli = new PageLayout(b.Size);
            b.Dispose();
            return pli;
        }
    }
}
