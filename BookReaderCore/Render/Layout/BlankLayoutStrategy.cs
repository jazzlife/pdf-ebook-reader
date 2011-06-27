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
            pli.Bounds = new Rectangle(0, 0, physicalPage.o.Width, physicalPage.o.Height);
            return pli;
        }


        public PageLayout DetectLayoutFromBook(ScreenBook book, int pageNum)
        {
            return null;
        }
    }
}
