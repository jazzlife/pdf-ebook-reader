using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Utils;
using System.Runtime.Serialization;

namespace PdfBookReader.Metadata
{
    /// <summary>
    /// Current position wihtin the book. Expressed in terms of physical pages.
    /// </summary>
    /// <remarks>
    /// This is the best approximation, since physical page height is unknown and varies.
    /// Exact position (in terms of total height of all pages) is unknwon, since page 
    /// heights can vary and are not all known. 
    /// </remarks>
    [DataContract]
    public class PositionInfo
    {
        /// <summary>
        /// Position of the top of the current screen in terms of physical pages.
        /// If top of the screen is on top of page #n, positon is n-1,
        /// since pages are numbered from 1.
        /// 
        /// Ranges in [0, PageCount] 
        /// Special range (PageCount - 1, PageCount) 
        /// means screen top is below last page top (rendering forward).
        /// </summary>
        [DataMember]
        public float Position { get; private set; }
       
        /// <summary>
        /// Total number of physical pages.
        /// </summary>
        [DataMember]
        public int PageCount { get; private set; }

        private PositionInfo(float position, int pageCount)
        {
            ArgCheck.GreaterThan(pageCount, 0, "pageCount");
            ArgCheck.InRange(position, 0, pageCount, "position");

            Position = position;
            PageCount = pageCount;
        }

        /// <summary>
        /// Create from the 0-1 position unit
        /// </summary>
        /// <param name="positionUnit"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public static PositionInfo FromPositionUnit(float positionUnit, int pageCount)
        {
            ArgCheck.GreaterThan(pageCount, 0, "pageCount");
            ArgCheck.IsUnit(positionUnit, "positionUnit");

            float position = positionUnit * pageCount;
            return new PositionInfo(position, pageCount);
        }

        /// <summary>
        /// Create based on physical page number and position within page
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="pageCount"></param>
        /// <param name="topOnScreen"></param>
        /// <param name="pageHeight"></param>
        /// <returns></returns>
        public static PositionInfo FromPhysicalPage(int pageNum, int pageCount, 
            int topOnScreen = 0, int pageHeight = 1)
        {
            ArgCheck.GreaterThan(pageCount, 0, "pageCount");
            ArgCheck.InRange(pageNum, 1, pageCount, "pageNum");

            float positionWithinPage = -(float)topOnScreen / pageHeight;

            // BUGFIX: screen top can be *above* page top when rendering
            // backwards at first page.
            if (positionWithinPage < 0) { positionWithinPage = 0; }

            float position = (pageNum - 1) + positionWithinPage;
            return new PositionInfo(position, pageCount);
        }

        /// <summary>
        /// Page number in range 0-1
        /// </summary>
        public int PageNum
        {
            get
            {
                int pos = (int)Position + 1;
                if (pos > PageCount) { pos = PageCount; }
                return pos;
            }
        }

        /// <summary>
        /// Absolute position in range [0-1]
        /// </summary>
        public float PositionUnit
        {
            get
            {
                return Position * UnitSize;
            }
        }

        /// <summary>
        /// Unit used for Position, to normalize on 0-1 scale.
        /// </summary>
        public float UnitSize
        {
            get 
            { 
                return (float)1 / PageCount; 
            }
        }

        public int GetTopOnScreen(int pageHeight)
        {
            float positionWithinPage = (Position + 1) - PageNum;
            int topOnScreen = -(int)(positionWithinPage * pageHeight);
            return topOnScreen;
        }
    }
}
