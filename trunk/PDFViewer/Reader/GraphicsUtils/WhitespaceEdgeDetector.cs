using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PDFViewer.Reader.GraphicsUtils
{
    public class WhitespaceEdgeDetector
    {
        const byte Threshold = 250; // background color threshold

        public static void RenderEdgeDetectFrame(Bitmap bmp, Graphics g)
        {
            WhitespaceEdgeDetector d = new WhitespaceEdgeDetector();
            Rectangle r = d.GetContentBounds(bmp);
            g.DrawRectangle(Pens.Red, r);
        }

        public Rectangle GetContentBounds(Bitmap bmp)
        {
            // TODO: if GetPixel is slow, try the faster/unsafe pointer version
            // http://www.codeproject.com/KB/GDI-plus/csharpgraphicfilters11.aspx?fid=3488&df=90&mpp=25&noise=3&sort=Position&view=Quick&fr=76&select=1123224

            int top = GetTopEdge(bmp);
            int bottom = GetBottomEdge(bmp);
            int left = GetLeftEdge(bmp);
            int right = GetRightEdge(bmp);

            return new Rectangle(left, top, right - left, bottom - top);
        }

        // Left, Right
        int GetLeftEdge(Bitmap bmp)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                if (!VerticalLineBlank(bmp, x)) { return x; }
            }
            return -1;
        }

        int GetRightEdge(Bitmap bmp)
        {
            for (int x = bmp.Width - 1; x >= 0; x--)
            {
                if (!VerticalLineBlank(bmp, x)) { return x; }
            }
            return -1;
        }

        // Top, Bottom
        int GetTopEdge(Bitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                if (!HorizontalLineBlank(bmp, y)) { return y; }
            }
            return -1;
        }

        int GetBottomEdge(Bitmap bmp)
        {
            for (int y = bmp.Height - 1; y >= 0; y--)
            {
                if (!HorizontalLineBlank(bmp, y)) { return y; }
            }
            return -1;
        }

        // Line blank detectors
        bool HorizontalLineBlank(Bitmap bmp, int y)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                if (!IsBackground(c)) { return false; }
            }
            return true;
        }

        bool VerticalLineBlank(Bitmap bmp, int x)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                Color c = bmp.GetPixel(x, y);
                if (!IsBackground(c)) { return false; }
            }
            return true;
        }

        bool IsBackground(Color c)
        {
            return c.R > Threshold && c.G > Threshold && c.B > Threshold;
        }


    }
}
