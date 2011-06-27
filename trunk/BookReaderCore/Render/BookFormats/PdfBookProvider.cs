using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using PDFLibNet;
using System.Windows.Forms;
using System.Drawing;
using BookReader.Utils;
using System.Drawing.Imaging;
using BookReader.Render;

namespace BookReader.Render
{
    public class PdfBookProvider : IBookProvider
    {
        DW<PDFWrapper> _pdfDoc;
        String _fullPath;

        // Performance tuning
        PdfRenderPerformanceInfo PerfInfo;

        public PdfBookProvider(String file)
        {
            ArgCheck.NotNull(file, "file");
            ArgCheck.FileExists(file);

            InitialzeXPdfConfig();

            PDFLibNet.xPDFParams.Antialias = true;
            PDFLibNet.xPDFParams.VectorAntialias = true;
            //xPDFParams.ErrorQuiet =true;
            //xPDFParams.ErrorFile = "C:\\stderr.log";

            _fullPath = file;

            LoadPdf(file);
        }

        public string BookFilename
        {
            get { return _fullPath; }
        }



        #region PdfDoc properties

        internal DW<PDFWrapper> InternalPdfWrapper { get { return _pdfDoc; } }

        public int PageCount { get { return _pdfDoc.o.PageCount; } }

        #endregion

        #region Loading PdfDoc

        /// <summary>
        /// Returns true if PDF document is loaded
        /// </summary>
        public bool PdfDocLoaded
        {
            get { return _pdfDoc != null && _pdfDoc.o.PageCount > 0; }
        }

        void AssertPdfDocLoaded()
        {
            if (!PdfDocLoaded) { throw new InvalidOperationException("PdfDoc not loaded."); }
        }

        // TODO: consider making it public
        void LoadPdf(String filename)
        {
            try 
            {
                _pdfDoc = DW.Wrap(new PDFWrapper());
                //_pdfDoc.PDFLoadCompeted += new PDFLoadCompletedHandler(_pdfDoc_PDFLoadCompeted);
                //_pdfDoc.PDFLoadBegin += new PDFLoadBeginHandler(_pdfDoc_PDFLoadBegin);
                //_pdfDoc.UseMuPDF = true;

                LoadFile(filename, _pdfDoc);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show(ex.Message, "IOException");
            }
            catch (System.Security.SecurityException ex)
            {
                MessageBox.Show(ex.Message, "SecurityException");
            }
            catch (System.IO.InvalidDataException ex)
            {
                MessageBox.Show(ex.Message, "InvalidDataException");
            }

            // New doc requires new performance info
            PerfInfo = new PdfRenderPerformanceInfo();
        }

        static bool LoadFile(string filename, DW<PDFWrapper> pdfDoc)
        {
            try
            {
                // Not supported by MuPDF: 
                // pdfDoc.LoadPDF(fileStream);

                bool loaded = pdfDoc.o.LoadPDF(filename);
                return loaded;
            }
            catch (System.Security.SecurityException)
            {
                throw new NotImplementedException("UI for password-protected PDF not implemented");
                /*
                PDFViewer.frmPassword frm = new PDFViewer.frmPassword();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    if (!frm.UserPassword.Equals(String.Empty))
                    {
                        pdfDoc.UserPassword = frm.UserPassword;
                    }
                    if (!frm.OwnerPassword.Equals(String.Empty))
                    {
                        pdfDoc.OwnerPassword = frm.OwnerPassword;
                    }
                    return LoadFile(filename, pdfDoc);
                }
                else
                {
                    // TODO: better error message
                    MessageBox.Show(Resources.UIStrings.ErrorFileEncrypted, filename);
                    return false;
                }
                 */

           }
        }

        static void InitialzeXPdfConfig()
        {
            // Taken from PDFViewer project. Not entirely sure what it does.

            //Update path to xpdfrc
            if (ConfigurationManager.AppSettings.Get("xpdfrc") == "xpdfrc")
            {
                // Open App.Config of executable
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                // Add an Application Setting.
                config.AppSettings.Settings.Remove("xpdfrc");
                config.AppSettings.Settings.Add("xpdfrc", AppDomain.CurrentDomain.BaseDirectory + "xpdfrc");
                // Save the configuration file.
                config.Save(ConfigurationSaveMode.Modified);
                // Force a reload of a changed section.
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        void DisposePdfDoc()
        {
            if (_pdfDoc != null)
            {
                _pdfDoc.DisposeItem();
                _pdfDoc = null;
            }
        }

        #endregion

        const double ZoomConst = 72.0;


        public DW<Bitmap> RenderPageImage(int pageNum, Size maxSize, RenderQuality quality = RenderQuality.Optimal)
        {
            if (pageNum < 1) { throw new ArgumentException("pageNum < 1. Should start at 1"); }
            AssertPdfDocLoaded();

            // Get quality, high by default
            if (quality == RenderQuality.Optimal)
            {
                quality = PerfInfo.QualityToUse;
            }

            DateTime startTime = DateTime.Now;
            DW<Bitmap> image = RenderPageCore(pageNum, maxSize, quality);

            double time = (DateTime.Now - startTime).TotalMilliseconds;
            PerfInfo.SaveTime(time, quality);

            return image;
        }

        DW<Bitmap> RenderPageCore(int pageNum, Size maxSize, RenderQuality quality)
        {
            ArgCheck.InRange(pageNum, 0, PageCount, "pageNum");

            _pdfDoc.o.CurrentPage = pageNum;

            _pdfDoc.o.UseMuPDF = false;
            if (_pdfDoc.o.SupportsMuPDF && quality == RenderQuality.HighQuality) 
            { 
                _pdfDoc.o.UseMuPDF = true; 
            }

            // Scale
            Size pageSize = new Size(_pdfDoc.o.PageWidth, _pdfDoc.o.PageHeight);
            Size size = pageSize.ScaleToFitBounds(maxSize);

            // 24bpp format for compatibility with AForge
            DW<Bitmap> bitmap = DW.Wrap(new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb));

            using (Graphics g = Graphics.FromImage(bitmap.o))
            {
                _pdfDoc.o.Zoom = ZoomConst * (double)bitmap.o.Width / _pdfDoc.o.PageWidth;
                try
                {
                    // Note: not certain what the params mean.
                    // Simple RenderPage sometimes does not zoom properly
                    _pdfDoc.o.RenderPage(g.GetHdc(), true, false);
                    g.ReleaseHdc();
                }
                finally
                {
                    _pdfDoc.o.Zoom = ZoomConst;
                }

                Rectangle bounds = new Rectangle(0, 0, bitmap.o.Width, bitmap.o.Height);
                g.FillRectangle(Brushes.White, bounds);

                _pdfDoc.o.ClientBounds = bounds;
                _pdfDoc.o.DrawPageHDC(g.GetHdc());
                g.ReleaseHdc();
            }

            return bitmap;
        }

        public void Dispose()
        {
            DisposePdfDoc();
        }

    }

}
