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

        /// <summary>
        /// Get the given physical page (render or fetch from cache).
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="screenSize"></param>
        /// <param name="physicalPageProvider"></param>
        /// <returns></returns>
        PageContent GetPage(int pageNum, Size screenSize, IBookPageProvider physicalPageProvider);
    }
}
