using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;
using BookReader.Render.Layout;

namespace BookReader.Render
{
    /// <summary>
    /// Generates PageContent from a physical page image.
    /// </summary>
    interface IPageSource : IDisposable
    {
        IPageLayoutStrategy LayoutStrategy { get; set; }

        /// <summary>
        /// Get the given physical page (render or fetch from cache).
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="screenSize"></param>
        /// <param name="bookProvider"></param>
        /// <returns></returns>
        Page GetPage(int pageNum, Size screenSize, ScreenBook screenBook);
    }
}
