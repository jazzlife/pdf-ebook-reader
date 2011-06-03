using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using PdfBookReader.Test.TestUtils;
using System.Drawing;
using PdfBookReader.Utils;
using System.Drawing.Imaging;
using PdfBookReader;
using PdfBookReader.Render;

namespace PdfBookReader.Test
{
    [TestFixture]
    public class BlobPageLayoutAnalyzerTest
    {
        IEnumerable<String> GetPageImageFiles()
        {
            const String path = @"..\..\PageImages";
            return Directory.GetFiles(path, "*.png");
        }

        PerfTimer _timer;

        [Test]
        public void All_DetectPageContentBounds()
        {
            _timer = new PerfTimer("Blob page layout analysis");
            ProcesAllImages();

            Console.WriteLine();
            Console.WriteLine(_timer);
            _timer = null;
        }

        void ProcesAllImages(int numFiles = -1)
        {
            var files = GetPageImageFiles().Select(x => x);
            if (numFiles > 0) { files = files.Take(numFiles); }

            int i = 0;
            foreach (String file in files)
            {
                ProcessImage(file, i++);
            }
        }

        void ProcessImage(String file, int index = 0)
        {
            DefaultPageLayoutAnalyzer detector = new DefaultPageLayoutAnalyzer();
            PageLayoutInfo layout;

            // Convert format
            using (Bitmap inBmp = LoadAndConvert(file, PixelFormat.Format24bppRgb))
            {
                using (_timer.NewRun)
                {
                    layout = detector.DetectPageLayout(inBmp);
                }

                // Save for inspection
                using (Bitmap debugOut = layout.Debug_RenderLayout(inBmp))
                {
                    debugOut.Save(String.Format(@"C:\temp\out{0:000}.png", index), ImageFormat.Png);
                }
            }
        }

        static Bitmap LoadAndConvert(String file, PixelFormat pixelFormat)
        {
            using(Bitmap temp = new Bitmap(file))
            {
                if (temp.PixelFormat == pixelFormat) { return temp; }
                return temp.Clone(new Rectangle(0, 0, temp.Width, temp.Height), pixelFormat);
            }
        }

    }
}
