using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using BookReader.Utils;
using BookReader.Render.Cache;

namespace PdfBookReader.UI
{
    [DefaultEvent("ValueChanged")]
    public class BookProgressBar : Control
    {
        float _value = 0;


        Brush _bBlank = new SolidBrush(SystemColors.ControlDarkDark);
        Brush _bFull = new SolidBrush(SystemColors.ControlDark);

        public float Value
        {
            get { return _value; }
            set
            {
                if (value.AlmostEquals(_value)) { return; }

                // Being lenient is ok, this is display-only
                if (value < 0) { value = 0; }
                if (value > 1) { value = 1; }

                _value = value;

                Invalidate();

                if (ValueChanged != null) { ValueChanged(this, EventArgs.Empty); }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Set position
            Value = (float)e.X / Width;
        }


        [CategoryAttribute("Property Changed")]
        public event EventHandler ValueChanged;

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bounds = new Rectangle(0,0, this.Width-1, this.Height-1);
            e.Graphics.FillRectangle(_bBlank, bounds);

            // Value
            e.Graphics.FillRectangle(_bFull, 0, 0, Value * Width, Height);

            // Loaded pages (if any)
            PaintCacheState(e);

            // Border, position
            e.Graphics.DrawLineVertical(Pens.White, (int)(Value * Width));
            e.Graphics.DrawRectangle(Pens.DarkGray, bounds);

            base.OnPaint(e);
        }

        // DEBUG

        #region Cache painting
        // Page numbers
        float _incrementSize = 0;
        IEnumerable<int> _diskPages;

        private void PaintCacheState(PaintEventArgs e)
        {
            if (PageIncrementSize == 0 ||
                _diskPages == null) { return; }

            foreach (int pageNum in _diskPages)
            {
                float pos = (pageNum - 1) * PageIncrementSize;

                Brush brush = (pageNum % 2 == 0) ? Brushes.Green : Brushes.DarkGreen;
                e.Graphics.FillRectangle(brush, pos * Width, Height / 2, PageIncrementSize * Width, Height);
            }
        }

        public float PageIncrementSize
        {
            get { return _incrementSize; }
            set 
            {
                ArgCheck.IsUnit(value);

                _incrementSize = value;
                Invalidate();
            }
        }

        public void SetLoadedPages(IEnumerable<int> diskPages)
        {
            if (diskPages == _diskPages) { return; }
            _diskPages = diskPages;
            Invalidate();
        }

        #endregion

    }
}
