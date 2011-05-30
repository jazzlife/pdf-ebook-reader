using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using PDFLibNet;
using System.Management;
using PDFViewer;

namespace PDFViewer.Reader
{

    public partial class PDFViewerControl : UserControl, PDFViewer.IStatusBusyControl
    {
        public delegate void RenderNotifyInvoker(int page, bool isCurrent);
     
        PDFWrapper _pdfDoc = null;

        public PDFViewerControl()
        {
            InitializeComponent();

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

            pageView.PageSize = new Size(pageView.Width,(int)( pageView.Width*11/8.5));
            pageView.Visible = true;
            
            StatusLabel.Text = Resources.UIStrings.StatusReady;
        }

        void frmPDFViewer_Resize(object sender, EventArgs e)
        {
            if (!PdfDocLoaded) { return; }

            using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusResizing))
            {
                FitWidth();
                Render();
            }
        }
        
        private void Render()
        {
            pageView.PageSize = new Size(_pdfDoc.PageWidth, _pdfDoc.PageHeight);
            txtPage.Text = string.Format("{0}/{1}", _pdfDoc.CurrentPage, _pdfDoc.PageCount);

            pageView.ResizeAndInvalidate();
        }

        private void FitWidth()
        {
            if (!PdfDocLoaded) { return; }

            using (PictureBox p = new PictureBox())
            {
                p.Width = pageView.ClientSize.Width;
                _pdfDoc.FitToWidth(p.Handle);
            }
            //_pdfDoc.RenderPageThread(pageView.Handle, true);
            _pdfDoc.RenderPage(pageView.Handle);
            Render();
        }

        private void FitHeight()
        {
            if (!PdfDocLoaded) { return; }

            using (PictureBox p = new PictureBox())
            {
                p.Height = pageView.ClientSize.Height;
                _pdfDoc.FitToHeight(p.Handle);
            }
            //_pdfDoc.RenderPageThread(pageView.Handle, false);
            _pdfDoc.RenderPage(pageView.Handle);
            Render();
        }

        private void txtPage_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
                {
                    if (_pdfDoc != null && e.KeyCode == Keys.Return)
                    {
                        int page = -1;
                        if (int.TryParse(txtPage.Text, out page))
                        {
                            if (page > 0 && page <= _pdfDoc.PageCount)
                            {
                                _pdfDoc.CurrentPage = page;
                                _pdfDoc.RenderPage(pageView.Handle);
                                Render();
                            }
                            else
                                page = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public void OpenFile(String filename)
        {
            try 
            {
                if (_pdfDoc != null)
                {
                    _pdfDoc.Dispose();
                    _pdfDoc = null;
                }

                //if (_pdfDoc == null)
                //{
                _pdfDoc = new PDFWrapper();
                //_pdfDoc.RenderNotifyFinished += ...
                _pdfDoc.PDFLoadCompeted += new PDFLoadCompletedHandler(_pdfDoc_PDFLoadCompeted);
                _pdfDoc.PDFLoadBegin += new PDFLoadBeginHandler(_pdfDoc_PDFLoadBegin);
                _pdfDoc.UseMuPDF = tsbUseMuPDF.Checked;
                //}
                //xPDFParams.ErrorQuiet =true;
                //xPDFParams.ErrorFile = "C:\\stderr.log";
                //}
                int ts = Environment.TickCount;
                using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingFile))
                {
                    if (LoadFile(filename, _pdfDoc))
                    {
                        Text = string.Format(Resources.UIStrings.StatusFormCaption, _pdfDoc.Author, _pdfDoc.Title);
                        _pdfDoc.CurrentPage = 1;

                        _pdfDoc.FitToWidth(pageView.Handle);
                        _pdfDoc.RenderPage(pageView.Handle);
                        Render();
                    }
                }
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

        public bool LoadStream(System.IO.Stream fileStream)
        {
            if (_pdfDoc != null)
            {
                _pdfDoc.Dispose();
                _pdfDoc = null;
            }
            //if (_pdfDoc == null)
            //{
            _pdfDoc = new PDFWrapper();
            //_pdfDoc.RenderNotifyFinished += ...
            _pdfDoc.PDFLoadCompeted += new PDFLoadCompletedHandler(_pdfDoc_PDFLoadCompeted);
            _pdfDoc.PDFLoadBegin += new PDFLoadBeginHandler(_pdfDoc_PDFLoadBegin);
            _pdfDoc.UseMuPDF = tsbUseMuPDF.Checked;

            try
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                //Does not supported by MuPDF.                
                //fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);                
                //return pdfDoc.LoadPDF(fs);                
                bool bRet = _pdfDoc.LoadPDF(fileStream);
                tsbUseMuPDF.Checked = _pdfDoc.UseMuPDF;
                return bRet;
            }
            catch (System.Security.SecurityException ex)
            {
                frmPassword frm = new frmPassword();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    if (!frm.UserPassword.Equals(String.Empty))
                        _pdfDoc.UserPassword = frm.UserPassword;
                    if (!frm.OwnerPassword.Equals(String.Empty))
                        _pdfDoc.OwnerPassword = frm.OwnerPassword;
                    bool bRet = _pdfDoc.LoadPDF(fileStream);
                    tsbUseMuPDF.Checked = _pdfDoc.UseMuPDF;
                    return bRet;
                }
                else
                {
                    throw;
                }
            }
                            
        }
        public bool ShowStream(System.IO.Stream fileStream)
        {
            if (LoadStream(fileStream))
            {

                Text = string.Format(Resources.UIStrings.StatusFormCaption, _pdfDoc.Author, _pdfDoc.Title);
                _pdfDoc.CurrentPage = 1;

                _pdfDoc.FitToWidth(pageView.Handle);
                _pdfDoc.RenderPage(pageView.Handle);
                Render();

                return true;
            }
            return false;
        }

        System.IO.FileStream fs = null;
        private bool LoadFile(string filename, PDFLibNet.PDFWrapper pdfDoc)
        {
            try
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                //Does not supported by MuPDF.                
                //fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);                
                //return pdfDoc.LoadPDF(fs);                
                bool bRet =  pdfDoc.LoadPDF(filename);               
                tsbUseMuPDF.Checked = pdfDoc.UseMuPDF;
                return bRet;                
            }
            catch (System.Security.SecurityException)
            {
                 frmPassword frm = new frmPassword();
                 if (frm.ShowDialog() == DialogResult.OK)
                 {
                     if (!frm.UserPassword.Equals(String.Empty))
                         pdfDoc.UserPassword = frm.UserPassword;
                     if (!frm.OwnerPassword.Equals(String.Empty))
                         pdfDoc.OwnerPassword = frm.OwnerPassword;
                     return LoadFile(filename, pdfDoc);
                 }
                 else
                 {
                     MessageBox.Show(Resources.UIStrings.ErrorFileEncrypted ,Text);
                     return false;
                 }
            }
        }

        void _pdfDoc_PDFLoadBegin()
        {
            UpdateParamsUI(false);
            Resize -= new EventHandler(frmPDFViewer_Resize);
        }

        void _pdfDoc_PDFLoadCompeted()
        {
            Resize += new EventHandler(frmPDFViewer_Resize);
            UpdateParamsUI();
        }

        #region Searching
        private int SearchCallBack(object sender, PDFViewer.SearchArgs e)
        {
            int lFound=0;
            if (_pdfDoc != null)
            {
                //PDFLibNet.xPDFParams.ErrorQuiet = false;
                //PDFLibNet.xPDFParams.ErrorFile="C:\\errorstd.log";
                _pdfDoc.SearchCaseSensitive = e.Exact;
                
                if (e.FromBegin)
                {
                    lFound = _pdfDoc.FindFirst(e.Text,
                    e.WholeDoc ? PDFLibNet.PDFSearchOrder.PDFSearchFromdBegin : PDFLibNet.PDFSearchOrder.PDFSearchFromCurrent,
                    e.Up,
                    e.WholeWord);
                }
                else if (e.FindNext)
                {   
                    if (e.Up)
                        lFound = _pdfDoc.FindPrevious(e.Text);
                    else
                        lFound = _pdfDoc.FindNext(e.Text);    
                }
                else
                {
                    lFound = _pdfDoc.FindText(e.Text,
                        _pdfDoc.CurrentPage,
                        (e.WholeDoc ? PDFLibNet.PDFSearchOrder.PDFSearchFromdBegin : PDFLibNet.PDFSearchOrder.PDFSearchFromCurrent),
                        e.Exact,
                        e.Up,
                        true,
                        e.WholeWord);    
                }
                if (lFound > 0)
                {
                    _pdfDoc.CurrentPage = _pdfDoc.SearchResults[0].Page;
                    _pdfDoc.RenderPage(pageView.Handle);
                    FocusSearchResult(_pdfDoc.SearchResults[0]);
                    Render();
                }
            }
            return lFound;
        }

        private void FocusSearchResult(PDFLibNet.PDFSearchResult res)
        {
            Point loc = res.Position.Location; 
            Point dr = pageView.ScrollPosition;
            if (_pdfDoc.PageWidth > pageView.Width)
                dr.X =(int)( loc.X * _pdfDoc.RenderDPI / 72) -pageView.Width ;
            if (_pdfDoc.PageHeight > pageView.Height)
                dr.Y =(int) ( loc.Y * _pdfDoc.RenderDPI / 72) -pageView.Height;
            pageView.ScrollPosition = dr;

        }

        private void tsbSearch_Click(object sender, EventArgs e)
        {
            if (!PdfDocLoaded) { return; }

            try
            {
                frmSearch frm = new frmSearch(new SearchPdfHandler(SearchCallBack));
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        #endregion

        private void tsbZoomIn_Click(object sender, EventArgs e)
        {
            try
            {
                using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
                {
                    if (_pdfDoc != null)
                    {
                        _pdfDoc.ZoomIN();
                        //_pdfDoc.RenderPageThread(pageView.Handle, false);
                        _pdfDoc.RenderPage(pageView.Handle);
                        Render();
                    }
                }
            }
            catch (Exception) { }
            
        }

        private void tsbZoomOut_Click(object sender, EventArgs e)
        {
            try
            {
                using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
                {
                    if (_pdfDoc != null)
                    {
                        _pdfDoc.ZoomOut();
                        //_pdfDoc.RenderPageThread(pageView.Handle, false);
                        _pdfDoc.RenderPage(pageView.Handle);
                        Render();
                    }
                }
            }
            catch (Exception) { }
        }

        private static string GetDefaultBrowserPath()
        {
            string key = @"htmlfile\shell\open\command";
            Microsoft.Win32.RegistryKey registryKey =
            Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key, false);
            // get default browser path
            return ((string)registryKey.GetValue(null, null)).Split('"')[1];

        }


        private void doubleBufferControl1_PaintControl(object sender,Rectangle view, Point location, Graphics g)
        {
            
            if (_pdfDoc != null)
            {
                Size sF= new Size(view.Right,view.Bottom);
                Rectangle r = new Rectangle(location, sF);
                _pdfDoc.ClientBounds = r;
                _pdfDoc.CurrentX = view.X;
                _pdfDoc.CurrentY = view.Y;
                _pdfDoc.DrawPageHDC(g.GetHdc());
                g.ReleaseHdc();
            }
        }

        private void OnNextPage(object sender, EventArgs e)
        {
            if (!PdfDocLoaded) { return; }

            using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
            {
                if (_pdfDoc != null)
                {
                    _pdfDoc.NextPage();
                    _pdfDoc.RenderPage(pageView.Handle);
                    Render();
                }
            }
        }

        private void OnPreviousPage(object sender, EventArgs e)
        {
            if (!PdfDocLoaded) { return; }

            using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
            {
                if (_pdfDoc != null && !IsDisposed)
                {
                    _pdfDoc.PreviousPage();
                    _pdfDoc.RenderPage(pageView.Handle);
                    Render();
                }
            }
        }

        private void tsbAntialias_Click(object sender, EventArgs e)
        {
            PDFLibNet.xPDFParams.Antialias = !PDFLibNet.xPDFParams.Antialias;
            tsbAntialias.Checked = PDFLibNet.xPDFParams.Antialias;

            if (!PdfDocLoaded) { return; }

            using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
            {
                if (_pdfDoc != null)
                {
                    _pdfDoc.RenderPage(pageView.Handle, true);
                    Render();
                    pageView.Invalidate();
                }
            }
        }

        private void tsbVectorAntialias_Click(object sender, EventArgs e)
        {
            PDFLibNet.xPDFParams.VectorAntialias = !PDFLibNet.xPDFParams.VectorAntialias;
            tsbVectorAntialias.Checked = PDFLibNet.xPDFParams.VectorAntialias;

            if (!PdfDocLoaded) { return; }

            using (StatusBusy sb = new StatusBusy(this, Resources.UIStrings.StatusLoadingPage))
            {
                if (_pdfDoc != null)
                {

                    _pdfDoc.RenderPage(pageView.Handle, true);
                    Render();
                    pageView.Invalidate();
                }
            }
        }

        private void UpdateParamsUI()
        {
            UpdateParamsUI(true);
        }
        private void UpdateParamsUI(bool enabled)
        {
            tsbAntialias.Enabled = _pdfDoc != null && enabled; 
            tsbVectorAntialias.Enabled = _pdfDoc != null && enabled;

            tsbAntialias.Checked = PDFLibNet.xPDFParams.Antialias;
            tsbVectorAntialias.Checked = PDFLibNet.xPDFParams.VectorAntialias;
        }
     
        private void tsbUseMuPDF_Click(object sender, EventArgs e)
        {
            if (_pdfDoc != null)
            {
                if (_pdfDoc.SupportsMuPDF)
                {
                    bool bs = _pdfDoc.UseMuPDF;
                    _pdfDoc.UseMuPDF = tsbUseMuPDF.Checked;
                    if (tsbUseMuPDF.Checked != bs)
                    {
                        tsbUseMuPDF.Checked = _pdfDoc.UseMuPDF;
                        if (PdfDocLoaded)
                        {
                            _pdfDoc.RenderPage(pageView.Handle, true);
                            Render();
                            pageView.Invalidate();
                        }
                    }
                }
            }
        }

        bool PdfDocLoaded
        {
            get { return _pdfDoc != null && _pdfDoc.PageCount > 0; }
        }

        public string StatusText
        {
            get { return StatusLabel.Text; }
            set { StatusLabel.Text = value; }
        }

    }    

}