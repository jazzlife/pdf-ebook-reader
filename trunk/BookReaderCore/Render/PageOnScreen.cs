using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;
using System.Runtime.Serialization;
using System.IO;
using BookReader.Render.Cache;
using BookReader.Render.Layout;

namespace BookReader.Render
{
    /// <summary>
    /// Physical page from the book along with the detected layout info. 
    /// </summary>
    class PageOnScreen 
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly int PageNum; // page number in document, 1-n
        
        public PageLayout Layout { get; private set; } // content layout

        int _topOnScreen;

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

        public PageOnScreen(int pageNum, PageLayout layout)
        {
            ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");

            PageNum = pageNum;
            Layout = layout;
        }

        // For convenience
        public int BottomOnScreen
        {
            get { return TopOnScreen + Layout.Bounds.Height; }
            set { TopOnScreen = value - Layout.Bounds.Height; }
        }

        public override string ToString()
        {
            return "Page #" + PageNum + " TopOnScreen = " + TopOnScreen;
        }

    }

}