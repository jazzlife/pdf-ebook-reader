using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;
using System.Runtime.Serialization;
using System.IO;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Rendered physical page with the detected layout info. 
    /// </summary>
    [DataContract(Name = "PageContent")]
    class PageContent : IDisposable
    {
        [DataMember (Name = "PageNum")]
        public readonly int PageNum; // page number in document, 1-n
        
        [DataMember (Name = "Layout")]
        PageLayoutInfo _layout; // content layout

        // Serialized separately
        Bitmap _image; // physical page image

        // Not serialized
        /// <summary>
        /// Distance between content bounds top and creen page top 
        /// screen.Top - countentBounds.Top
        /// </summary>
        public int TopOnScreen = 0;

        public PageContent(int pageNum, Bitmap image, PageLayoutInfo layout)
        {
            ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");
            ArgCheck.NotNull(image);

            PageNum = pageNum;
            _image = image;
            _layout = layout;
        }

        public PageContent(int pageNum, PageLayoutInfo layout)
        {
            ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");

            PageNum = pageNum;
            _layout = layout;
        }

        public Bitmap Image { get { return _image; } }
        public PageLayoutInfo Layout { get { return _layout; } }

        // For convenience
        public int BottomOnScreen
        {
            get { return TopOnScreen + Layout.Bounds.Height; }
            set { TopOnScreen = value - Layout.Bounds.Height; }
        }

        public void Dispose()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
                _layout = null;
            }
        }

        public override string ToString()
        {
            return "PhysicalPage #" + PageNum + " TopOnScreen = " + TopOnScreen;
        }

    }

}