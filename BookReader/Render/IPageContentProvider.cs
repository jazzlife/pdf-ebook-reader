using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Generates PageContent from a physical page image.
    /// </summary>
    interface IPageContentProvider
    {
        IPageLayoutAnalyzer LayoutAnalyzer { get; set; }
        PageContent RenderPhysicalPage(int pageNum, Size screenSize, IPhysicalPageProvider physicalPageProvider);
    }
}
