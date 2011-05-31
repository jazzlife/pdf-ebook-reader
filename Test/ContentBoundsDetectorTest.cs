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
            ProcesAllImages("AForge", ContentBoundsDetector.RenderBlobsInfo);
        }

        void ProcesAllImages(String timerName, CustomRenderDelegate fnRender, int numFiles = -1)
        {
            PerfTimer timer = new PerfTimer(timerName);

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

                            using (timer.NewRun)
                            {
                                fnRender(outBmp, g);
                            }

                            outBmp.Save(String.Format(@"C:\temp\out{0:000}.jpg", i++), ImageFormat.Png);
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(timer);
        }

    }
}
