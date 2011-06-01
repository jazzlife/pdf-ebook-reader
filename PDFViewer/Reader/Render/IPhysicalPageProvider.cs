﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PDFViewer.Reader.Render
{
    /// <summary>
    /// Provides physical pages from a book 
    /// (e.g. PDF file, folder with images etc.)
    /// </summary>
    interface IPhysicalPageProvider
    {
        /// <summary>
        /// Total number of pages in the book.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Render and return the specific physical page.
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="maxSize">Maximum size to fit the page within (preserving aspect ratio)</param>
        /// <param name="quality"></param>
        /// <returns></returns>
        Bitmap RenderPage(int pageNum, Size maxSize, RenderQuality quality);


    }

    public enum RenderQuality
    {
        Fast,
        HighQuality,
    }

}
