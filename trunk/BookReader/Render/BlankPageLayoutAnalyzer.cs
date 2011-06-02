using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Full physical page area as layout, no header/footer. 
    /// </summary>
    public class BlankPageLayoutAnalyzer : IPageLayoutAnalyzer
    {
        public PageLayoutInfo DetectPageLayout(Bitmap physicalPage)
        {
            PageLayoutInfo pli = new PageLayoutInfo(physicalPage.Size);
            pli.Bounds = new Rectangle(0, 0, physicalPage.Width, physicalPage.Height);
            return pli;
        }
    }
}
