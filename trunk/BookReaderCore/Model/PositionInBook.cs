using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;
using System.Runtime.Serialization;
using BookReader.Render.Cache;

namespace BookReader.Model
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
    [Immutable]
    public class PositionInBook
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

        private PositionInBook(float position, int pageCount)
        {
            ArgCheck.GreaterThan(pageCount, 0, "pageCount");
            ArgCheck.InRange(position, 0, pageCount, "position");

            if (position == float.NaN) { throw new Exception(); }

            Position = position;
            PageCount = pageCount;
        }

        /// <summary>
        /// Create from the 0-1 position unit
        /// </summary>
        /// <param name="positionUnit"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public static PositionInBook FromPositionUnit(float positionUnit, int pageCount)
        {
            ArgCheck.GreaterThan(pageCount, 0, "pageCount");
            ArgCheck.IsUnit(positionUnit, "positionUnit");

            float position = positionUnit * pageCount;
            return new PositionInBook(position, pageCount);
        }

        /// <summary>
        /// Create based on physical page number and position within page
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="pageCount"></param>
        /// <param name="topOnScreen"></param>
        /// <param name="pageHeight"></param>
        /// <returns></returns>
        public static PositionInBook FromPhysicalPage(int pageNum, int pageCount, 
            int topOnScreen = 0, int pageHeight = 1)
        {
            ArgCheck.GreaterThan(pageHeight, 0, "pageHeight");
            ArgCheck.GreaterThan(pageCount, 0, "pageCount");
            ArgCheck.InRange(pageNum, 1, pageCount, "pageNum");

            float positionWithinPage = -(float)topOnScreen / pageHeight;

            // BUGFIX: screen top can be *above* page top when rendering
            // backwards at first page.
            if (positionWithinPage < 0) { positionWithinPage = 0; }

            float position = (pageNum - 1) + positionWithinPage;
            return new PositionInBook(position, pageCount);
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
            int topOnScreen = -(int)Math.Round(positionWithinPage * pageHeight);
            return topOnScreen;
        }

        public override bool Equals(object obj)
        {
            PositionInBook that = obj as PositionInBook;
            if (obj == null) { return false; }

            return this.Position.AlmostEquals(that.Position) && this.PageCount == that.PageCount;
        }

        public override string ToString()
        {
            return "Position: {0:0.0}/{1} pageNum={2}".F(Position, PageCount, PageNum);
        }
    }
}
