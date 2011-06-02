using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Information about a rendered physical page. 
    /// </summary>
    class PhysicalPageInfo : IDisposable
    {
        public readonly int PageNum; // page number in document, 1-n

        Bitmap _image; // physical page image
        PageLayoutInfo _layout; // content layout

        // Distance between top of screen page and content bounds content bounds and 

        /// <summary>
        /// Distance between content bounds top and creen page top 
        /// screen.Top - countentBounds.Top
        /// </summary>
        public int TopOnScreen = 0;

        public PhysicalPageInfo(int pageNum, Bitmap image, PageLayoutInfo layout)
        {
            ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");
            ArgCheck.NotNull(image);

            PageNum = pageNum;
            _image = image;
            _layout = layout;

            ContentBounds = Layout.Bounds;
        }

        public Bitmap Image { get { return _image; } }
        public PageLayoutInfo Layout { get { return _layout; } }

        // For convenience
        public int BottomOnScreen
        {
            get { return TopOnScreen + ContentBounds.Height; }
            set { TopOnScreen = value - ContentBounds.Height; }
        }

        /// <summary>
        /// Usually same as Layout.Bounds, but can be set differently in some
        /// tweaking scenarios (e.g. to avoid splitting a row)
        /// </summary>
        public Rectangle ContentBounds { get; set; }

        public void Dispose()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
                _layout = null;
                ContentBounds = Rectangle.Empty;
            }
        }

        public override string ToString()
        {
            return "PhysicalPage #" + PageNum + " TopOnScreen = " + TopOnScreen;
        }
    }

}