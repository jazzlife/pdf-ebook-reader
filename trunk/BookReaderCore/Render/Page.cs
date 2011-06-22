using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;
using System.Runtime.Serialization;
using System.IO;
using BookReader.Render.Cache;

namespace BookReader.Render
{
    /// <summary>
    /// Physical page from the book along with the detected layout info. 
    /// </summary>
    [DataContract(Name = "PageContent")]
    class Page : ICachedDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [DataMember (Name = "PageNum")]
        public readonly int PageNum; // page number in document, 1-n
        
        [DataMember (Name = "Layout")]
        PageLayoutInfo _layout; // content layout

        // Serialized separately
        DW<Bitmap> _image; // physical page image


        int _topOnScreen;
        // Not serialized
        /// <summary>
        /// Distance between content bounds top and screen page top 
        /// screen.Top - countentBounds.Top
        /// </summary>
        public int TopOnScreen
        {
            get { return _topOnScreen; }
            set
            {
                if (!(-3000 < value && value < 3000))
                {
                    logger.Error("Wrong _topOfScreen value: " + value + " in: " + this);
                }

                _topOnScreen = value;
            }

        }

        public Page(int pageNum, DW<Bitmap> image, PageLayoutInfo layout)
        {
            ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");

            PageNum = pageNum;
            _image = image;
            _layout = layout;
        }

        public DW<Bitmap> Image { get { return _image; } }
        public PageLayoutInfo Layout { get { return _layout; } }

        // For convenience
        public int BottomOnScreen
        {
            get { return TopOnScreen + Layout.Bounds.Height; }
            set { TopOnScreen = value - Layout.Bounds.Height; }
        }

        public override string ToString()
        {
            return "PhysicalPage #" + PageNum + " TopOnScreen = " + TopOnScreen;
        }


        #region ICachedDisposable

        /// <summary>
        /// Dispose object when Return() is called. 
        /// Set to false to manage object disposal manually (e.g. prefetch/cache it)
        /// </summary>
        internal bool DisposeOnReturn = true;

        bool _inUse = true;
        public void Return()
        {
            _inUse = false;
            if (DisposeOnReturn && Image != null) { Image.DisposeItem(); }
        }

        public bool InUse
        {
            get { return _inUse; }
        }

        internal void Reuse()
        {
            _inUse = true;
        }

        #endregion
    }

}