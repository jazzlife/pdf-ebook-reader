using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using PDFViewer.Test.TestUtils;
using System.Drawing;
using PDFViewer.Reader.Utils;
using PDFViewer.Reader.GraphicsUtils;
using System.Drawing.Imaging;
using PDFViewer.Reader;

namespace PDFViewer.Test
{
    [TestFixture]
    public class ContentBoundsDetectorTest
    {
        IEnumerable<String> GetPageImageFiles()
        {
            const String path = @"..\..\PageImages";
            return Directory.GetFiles(path, "*.png");
        }

        [Test]
        public void All_DetectPageContentBounds()
        {
            _timerDetectBlobs = new PerfTimer("DetectBlobs");
            _timerRows = new PerfTimer("RowBounds");

            ProcesAllImages(DetectContentBounds);

            Console.WriteLine();
            Console.WriteLine(_timerDetectBlobs);
            Console.WriteLine(_timerRows);
        }

        PerfTimer _timerDetectBlobs;
        PerfTimer _timerRows;

        void DetectContentBounds(Bitmap bmp, Graphics g)
        {
            ContentBoundsInfo cbi;
            ContentBoundsDetector detector = new ContentBoundsDetector();

            using (_timerDetectBlobs.NewRun)
            {
                cbi = detector.DetectBlobs(bmp, g);
            }
            using (_timerRows.NewRun)
            {
                detector.DetectRowBounds(ref cbi, g);
            }
        }

        void ProcesAllImages(CustomRenderDelegate fnRender, int numFiles = -1)
        {
            var files = GetPageImageFiles().Select(x => x);
            if (numFiles > 0) { files = files.Take(numFiles); }

            int i = 0;
            foreach (String file in files)
            {
                ProcessImage(fnRender, file, i++);
            }
        }

        void ProcessImage(CustomRenderDelegate fnRender, String file, int index = 0)
        {
            using (Bitmap inBmp = new Bitmap(file))
            {
                using (Bitmap outBmp = new Bitmap(inBmp.Width, inBmp.Height, PixelFormat.Format24bppRgb))
                {
                    using (Graphics g = Graphics.FromImage(outBmp))
                    {
                        g.DrawImageUnscaled(inBmp, 0, 0);

                        fnRender(outBmp, g);

                        outBmp.Save(String.Format(@"C:\temp\out{0:000}.png", index), ImageFormat.Png);
                    }
                }
            }
        }

    }
}
