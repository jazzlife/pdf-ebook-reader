using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Analyzes the layout of a physical page.
    /// </summary>
    public interface IPageLayoutAnalyzer
    {
        PageLayoutInfo DetectPageLayout(Bitmap physicalPage);
    }


}
