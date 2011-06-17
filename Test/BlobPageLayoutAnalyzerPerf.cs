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
using PdfBookReader.Render.Cache;

namespace PdfBookReader.Test
{
    [TestFixture]
    public class BlobPageLayoutAnalyzerPerf
    {
        IEnumerable<String> GetPageImageFiles()
        {
            String path = CacheUtils.CacheFolderPath;
            return Directory.GetFiles(path, "*.png"); //.Take(30);
        }

        [Test]
        public void All_DetectPageContentBounds()
        {
            using (PTimer timer = new PTimer("Layout Analysis: blob page layout"))
            {
                ProcesAllImages(timer);
            }
        }

        void ProcesAllImages(PTimer timer)
        {
            var files = GetPageImageFiles().Select(x => x);

            int i = 0;
            foreach (String file in files)
            {
                ProcessImage(timer, file, i++);
            }
        }

        void ProcessImage(PTimer timer, String file, int index = 0)
        {
            BlobPageLayoutAnalyzer detector = new BlobPageLayoutAnalyzer();
            PageLayoutInfo layout;

            // Convert format
            using (Bitmap inBmp = LoadAndConvert(file, PixelFormat.Format24bppRgb))
            {
                using (timer.NewRun)
                {
                    layout = detector.DetectPageLayout(DW.Wrap(inBmp));
                }

                String path = Path.Combine(CacheUtils.CacheFolderPath, "layout");
                if (Directory.Exists(path)) { Directory.Delete(path); }
                Directory.CreateDirectory(path);

                // Save for inspection
                using (Bitmap debugOut = layout.Debug_RenderLayout(inBmp))
                {
                    debugOut.Save(Path.Combine(path, Path.GetFileName(file)), ImageFormat.Png);
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
