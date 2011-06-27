using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDFLibNet;
using System.Drawing;
using BookReader.Render.Cache;

namespace BookReader.Render.Layout
{
    /// <summary>
    /// Information on the word layout
    /// </summary>
    [Immutable]
    class WordInfo
    {
        public Rectangle Bounds { get; private set; }
        public String Word { get; private set; }

        // Color, font size etc. not used for now

        public WordInfo(PDFTextWord word)
        {
            Bounds = word.Bounds;
            Word = word.Word;
        }

        public WordInfo(Rectangle bounds, String word)
        {
            Bounds = bounds;
            Word = word;
        }


    }
}
