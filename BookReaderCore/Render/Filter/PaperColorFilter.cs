using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Imaging.Filters;
using AForge;
using BookReader.Utils;

namespace BookReader.Render.Filter
{
    public class PaperColorFilter
    {
        /// <summary>
        /// Darken text multipler 0-1, 1 is original
        /// </summary>
        public float TextDarken { get; set; }

        /// <summary>
        /// Brigness, multipler from 0-1
        /// </summary>
        public float Brightness { get; set; }

        /// <summary>
        /// Invert the image
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Darkest color
        /// </summary>
        public Color DarkestColor { get; set; }

        /// <summary>
        /// Brightest color
        /// </summary>
        public Color BrightestColor { get; set; }

        public PaperColorFilter(Color darkestColor, Color brightestColor, 
            float brightness = 0.5f, bool invert = false,
            float textDarken = 1)
        {
            Invert = invert;
            DarkestColor = darkestColor;
            BrightestColor = brightestColor;
            Brightness = brightness;
            TextDarken = textDarken;
        }

        public Color BackColor
        {
            get
            {
                if (Invert) { return Color.Black; }
                return Scale(DarkestColor, BrightestColor, Brightness);
            }
        }

        public void ApplyInPlace(Bitmap bmp)
        {
            if (Invert)
            {
                Invert invertF = new Invert();
                invertF.ApplyInPlace(bmp);
            }

            Color c = Scale(DarkestColor, BrightestColor, Brightness); ; 
            LevelsLinear levelsF = new LevelsLinear();

            if (Invert)
            {
                levelsF.Input = new IntRange(0, (int)(255 * TextDarken));
            }
            else
            {
                levelsF.Input = new IntRange(255 - (int)(255 * TextDarken), 255);
            }

            levelsF.OutRed = new IntRange(0, c.R);
            levelsF.OutGreen = new IntRange(0, c.G);
            levelsF.OutBlue = new IntRange(0, c.B);

            levelsF.ApplyInPlace(bmp);
        }

        static int Scale(int min, int max, float factor)
        {
            return min + (int)(factor * (max - min));
        }
        static Color Scale(Color dark, Color bright, float factor)
        {
            return Color.FromArgb(
                Scale(dark.R, bright.R, factor),
                Scale(dark.G, bright.G, factor),
                Scale(dark.B, bright.B, factor));
        }

        #region preset colors

        public static PaperColorFilter Sepia(float brightness = 0.5f)
        {
            return new PaperColorFilter(Color.FromArgb(140, 133, 120),
                Color.FromArgb(250, 240, 218), brightness);
        }
        public static PaperColorFilter Black(float brightness = 0.5f)
        {
            return new PaperColorFilter(Color.FromArgb(141, 141, 141),
                Color.FromArgb(255, 255, 255), brightness, true, 0.8f);
        }
        public static PaperColorFilter White(float brightness = 0.5f)
        {
            return new PaperColorFilter(Color.FromArgb(141, 141, 141), 
                Color.FromArgb(255, 255, 255), brightness);
        }

        #endregion

    }
}
