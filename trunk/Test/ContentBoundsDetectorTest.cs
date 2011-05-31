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


        IEnumerable<String> GetImageFiles()
        {
            const String path = @"..\..\PageImages";
            return Directory.GetFiles(path, "*.png");
        }
        
        [Test]
        public void AForgeEdgeDetect()
        {
            _timerDetectBlobs = new PerfTimer("DetectBlobs");
            _timerMainContent = new PerfTimer("MainContentBounds");
            _timerRows = new PerfTimer("RowBounds");

            ProcesAllImages(ProcessOneImage);

            Console.WriteLine();
            Console.WriteLine(_timerDetectBlobs);
            Console.WriteLine(_timerMainContent);
            Console.WriteLine(_timerRows);
        }

        PerfTimer _timerDetectBlobs;
        PerfTimer _timerMainContent;
        PerfTimer _timerRows;

        void ProcessOneImage(Bitmap bmp, Graphics g)
        {
            ContentBoundsInfo cbi;
            ContentBoundsDetector detector = new ContentBoundsDetector();

            using (_timerDetectBlobs.NewRun)
            {
                cbi = detector.DetectBlobs(bmp, g);
            }
            using (_timerMainContent.NewRun)
            {
                detector.DetectMainContentBounds(ref cbi, g);
            }
            using (_timerRows.NewRun)
            {
                detector.DetectRowBounds(ref cbi, g);
            }
        }

        void ProcesAllImages(CustomRenderDelegate fnRender, int numFiles = -1)
        {
            var files = GetImageFiles().Select(x => x);
            if (numFiles > 0) { files = files.Take(numFiles); }

            int i = 0;
            foreach (String file in files)
            {
                using (Bitmap inBmp = new Bitmap(file))
                {
                    using (Bitmap outBmp = new Bitmap(inBmp.Width, inBmp.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics g = Graphics.FromImage(outBmp))
                        {
                            g.DrawImageUnscaled(inBmp, 0, 0);

                            fnRender(outBmp, g);

                            outBmp.Save(String.Format(@"C:\temp\out{0:000}.jpg", i++), ImageFormat.Png);
                        }
                    }
                }
            }
        }

    }
}
