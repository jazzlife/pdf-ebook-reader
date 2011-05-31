using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using PDFLibNet;
using System.Windows.Forms;
using System.Drawing;
using PDFViewer.Reader.Utils;

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


        const double ZoomConst = 72.0;

        public Bitmap RenderPdfPageToBitmap(int pageNum, Size maxSize, RenderQuality quality, CustomRenderDel fnCustomRender)
        {
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
            Bitmap bitmap = new Bitmap(size.Width, size.Height);

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
                g.FillRectangle(Brushes.Orange, bounds);

                _pdfDoc.ClientBounds = bounds;
                _pdfDoc.DrawPageHDC(g.GetHdc());
                g.ReleaseHdc();

                if (fnCustomRender != null)
                {
                    fnCustomRender(bitmap, g);
                }
            }

            return bitmap;
        }

        public delegate void CustomRenderDel(Bitmap bmp, Graphics g);


        public void Dispose()
        {
            DisposePdfDoc();
        }
    }

    public enum RenderQuality
    {
        Fast,
        HighQualityMuPdf,
    }

    

}
