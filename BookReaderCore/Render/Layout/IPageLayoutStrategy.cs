using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;

namespace BookReader.Render
{
    /// <summary>
    /// Analyzes the layout of a physical page.
    /// </summary>
    interface IPageLayoutStrategy
    {
        /// <summary>
        /// Detect layout from physical page image if possible. If not supported, return null.
        /// </summary>
        /// <param name="physicalPage"></param>
        /// <returns></returns>
        PageLayoutInfo DetectLayoutFromImage(DW<Bitmap> physicalPage);

        /// <summary>
        /// Detect layout from book if possible. If not supported, return null.
        /// </summary>
        /// <param name="physicalPage"></param>
        /// <returns></returns>
        PageLayoutInfo DetectLayoutFromBook(ScreenBook book, int pageNum);

    }


}
