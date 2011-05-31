using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using PDFLibNet;
using System.Windows.Forms;
using System.Drawing;
using PDFViewer.Reader.Utils;
using PDFViewer.Reader.GraphicsUtils;
using System.Drawing.Imaging;

namespace PDFViewer.Reader
{

    public class PdfEBookRenderer : IDisposable
    {
        PDFWrapper _pdfDoc;

        public PdfEBookRenderer()
        {
            InitialzeXPdfConfig();

            PDFLibNet.xPDFParams.Antialias = true;
            PDFLibNet.xPDFParams.VectorAntialias = true;
            //xPDFParams.ErrorQuiet =true;
            //xPDFParams.ErrorFile = "C:\\stderr.log";
        }

        #region PdfDoc properties

        public int PageCount { get { return _pdfDoc.PageCount; } }

        #endregion

        #region Loading PdfDoc

        /// <summary>
        /// Returns true if PDF document is loaded
        /// </summary>
        public bool PdfDocLoaded
        {
            get { return _pdfDoc != null && _pdfDoc.PageCount > 0; }
        }

        void AssertPdfDocLoaded()
        {
            if (!PdfDocLoaded) { throw new InvalidOperationException("PdfDoc not loaded."); }
        }

        public void LoadPdf(String filename)
        {            
            try 
            {
                _pdfDoc = new PDFWrapper();
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
        }

        static bool LoadFile(string filename, PDFWrapper pdfDoc)
        {
            try
            {
                // Not supported by MuPDF: 
                // pdfDoc.LoadPDF(fileStream);

                bool loaded = pdfDoc.LoadPDF(filename);
                return loaded;
            }
            catch (System.Security.SecurityException)
            {
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
            }
        }

        static void InitialzeXPdfConfig()
        {
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
                _pdfDoc.Dispose();
                _pdfDoc = null;
            }
        }

        #endregion

        readonly Size LayoutRenderSize = new Size(1000, 1000);

        public Bitmap RenderScreenPageToBitmap(int pdfPageNum, int topOfPdfPage, Size screenPageSize)
        {
            if (pdfPageNum < 1) { throw new ArgumentException("pdfPageNum < 1. Should start at 1"); }

            // 24bpp format for compatibility with AForge
            Bitmap screenPage = new Bitmap(screenPageSize.Width, screenPageSize.Height, PixelFormat.Format24bppRgb);
            ContentBoundsDetector detector = new ContentBoundsDetector();

            using (Graphics g = Graphics.FromImage(screenPage))
            {
                int screenPageTop = 0;
                while (screenPageTop < screenPageSize.Height)
                {
                    // Figure out layout
                    ContentBoundsInfo cbi;
                    using (Bitmap pdfLayoutPage = RenderPdfPageToBitmap(pdfPageNum, LayoutRenderSize))
                    {
                        cbi = detector.DetectBounds(pdfLayoutPage);
                    }

                    // Empty page special case
                    if (cbi.Bounds == Rectangle.Empty)
                    {
                        // TODO: do something more sensible
                        g.FillEllipse(Brushes.DarkSlateGray, 10, 10, 30, 30);
                        break;
                    }

                    // Render actual page. Bounded by width, but not height.
                    int maxWidth = (int)((float)screenPageSize.Width / cbi.BoundsRelative.Width);
                    Size displayPageMaxSize = new Size(maxWidth, int.MaxValue);

                    using (Bitmap pdfDisplayPage = RenderPdfPageToBitmap(pdfPageNum, displayPageMaxSize))
                    {
                        g.DrawImageUnscaled(pdfDisplayPage,
                            - (int)(cbi.BoundsRelative.X * pdfDisplayPage.Width),
                            - topOfPdfPage - (int)(cbi.BoundsRelative.Y * pdfDisplayPage.Height));
                    }

                    // TODO: add other pages as needed
                    break;
                }
            }

            return screenPage;
        }

        const double ZoomConst = 72.0;

        public Bitmap RenderPdfPageToBitmap(int pageNum, Size maxSize, 
            RenderQuality quality = RenderQuality.HighQualityMuPdf)
        {
            if (pageNum < 1) { throw new ArgumentException("pageNum < 1. Should start at 1"); }
            AssertPdfDocLoaded();

            if (pageNum < 1 || pageNum > _pdfDoc.PageCount) { return null; }

            _pdfDoc.CurrentPage = pageNum;

            _pdfDoc.UseMuPDF = false;
            if (_pdfDoc.SupportsMuPDF && quality == RenderQuality.HighQualityMuPdf) 
            { 
                _pdfDoc.UseMuPDF = true; 
            }


            // Scale            
            Size pageSize = new Size(_pdfDoc.PageWidth, _pdfDoc.PageHeight);
            Size size = pageSize.ScaleToFitBounds(maxSize);

            // 24bpp format for compatibility with AForge
            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                _pdfDoc.Zoom = ZoomConst * (double)bitmap.Width / _pdfDoc.PageWidth;
                try
                {
                    // Note: not certain what the params mean.
                    // Simple RenderPage sometimes does not zoom properly
                    _pdfDoc.RenderPage(g.GetHdc(), true, false);
                    g.ReleaseHdc();
                }
                finally
                {
                    _pdfDoc.Zoom = ZoomConst;
                }

                Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                g.FillRectangle(Brushes.White, bounds);

                _pdfDoc.ClientBounds = bounds;
                _pdfDoc.DrawPageHDC(g.GetHdc());
                g.ReleaseHdc();
            }

            return bitmap;
        }

        public void Dispose()
        {
            DisposePdfDoc();
        }
    }

    public delegate void CustomRenderDelegate(Bitmap bmp, Graphics g);

    public enum RenderQuality
    {
        Fast,
        HighQualityMuPdf,
    }

    

}
