using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using PdfBookReader.Utils;

namespace PdfBookReader.UI
{
    public class BookProgressBar : Control
    {
        float _value = 0;

        // Page numbers
        float _incrementSize = 0;
        HashSet<int> _loadedPageNumbers = new HashSet<int>();

        Brush _bBlank = new SolidBrush(SystemColors.ControlDarkDark);
        //Brush _bBlankLoaded = new SolidBrush(SystemColors.ControlDarkDark.NewShade(1.1, 1.1, 1.3));
        Brush _bBlankLoaded = Brushes.DarkCyan;

        Brush _bFull = new SolidBrush(SystemColors.ControlDark);
        //Brush _bFullLoaded = new SolidBrush(SystemColors.ControlDark.NewShade(0.9, 0.9, 1.1));
        Brush _bFullLoaded = Brushes.Cyan;


        public float Value
        {
            get { return _value; }
            set
            {
                // Being lenient is ok, this is display-only
                if (value < 0) { value = 0; }
                if (value > 1) { value = 1; }

                _value = value;

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bounds = new Rectangle(0,0, this.Width-1, this.Height-1);
            e.Graphics.FillRectangle(_bBlank, bounds);

            // Value
            e.Graphics.FillRectangle(_bFull, 0, 0, Value * Width, Height);

            // Loaded pages (if any)
            if (PageIncrementSize > 0 && _loadedPageNumbers.Count > 0)
            {
                foreach(int pageNum in _loadedPageNumbers)
                {
                    float pos = (pageNum - 1) * PageIncrementSize;
                    Brush b = (pos <= Value) ? _bFullLoaded : _bBlankLoaded;
                    
                    // Draw *before* current page
                    e.Graphics.FillRectangle(b, (pos - PageIncrementSize) * Width, 0, PageIncrementSize * Width, Height);
                }
            }

            // Border, position
            e.Graphics.DrawLineVertical(Pens.White, (int)(Value * Width));
            e.Graphics.DrawRectangle(Pens.DarkGray, bounds);

            base.OnPaint(e);
        }

        public float PageIncrementSize
        {
            get { return _incrementSize; }
            set 
            {
                ArgCheck.IsRatio(value);

                _incrementSize = value;
                Invalidate();
            }
        }

        public void AddLoadedPage(int pageNumber)
        {
            _loadedPageNumbers.Add(pageNumber);
            Invalidate();
        }

        public void AddLoadedPages(IEnumerable<int> pageNumbers)
        {
            pageNumbers.ForEach(x=> _loadedPageNumbers.Add(x));
            Invalidate();
        }

        public void ClearLoadedPages()
        {
            _loadedPageNumbers.Clear();
            Invalidate();
        }


    }
}
