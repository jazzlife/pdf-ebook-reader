using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Full physical page area as layout, no header/footer. 
    /// </summary>
    class BlankPageLayoutAnalyzer : IPageLayoutAnalyzer
    {
        public PageLayoutInfo DetectPageLayout(DW<Bitmap> physicalPage)
        {
            PageLayoutInfo pli = new PageLayoutInfo(physicalPage.o.Size);
            pli.Bounds = new Rectangle(0, 0, physicalPage.o.Width, physicalPage.o.Height);
            return pli;
        }
    }
}
