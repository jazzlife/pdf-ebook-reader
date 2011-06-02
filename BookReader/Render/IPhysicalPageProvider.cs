using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Provides physical pages from a book 
    /// (e.g. PDF file, folder with images etc.)
    /// 
    /// Disposable since it may contain large in-memory objects.
    /// </summary>
    public interface IPhysicalPageProvider : IDisposable
    {
        /// <summary>
        /// Total number of pages in the book.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Full path to document or folder or url containing the book.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Render and return the specific physical page.
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="maxSize">Maximum size to fit the page within (preserving aspect ratio)</param>
        /// <param name="quality"></param>
        /// <returns></returns>
        Bitmap RenderPage(int pageNum, Size maxSize, RenderQuality quality = RenderQuality.HighQuality);
    }

    public enum RenderQuality
    {
        Fast,
        HighQuality,
    }

}
