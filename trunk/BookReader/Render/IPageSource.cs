using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Generates PageContent from a physical page image.
    /// </summary>
    interface IPageSource : IDisposable
    {
        IPageLayoutStrategy LayoutAnalyzer { get; set; }

        /// <summary>
        /// Get the given physical page (render or fetch from cache).
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="screenSize"></param>
        /// <param name="bookProvider"></param>
        /// <returns></returns>
        Page GetPage(int pageNum, Size screenSize, DW<IBookProvider> bookProvider);
    }
}
