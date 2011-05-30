using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using PDFViewer.Reader;

namespace PDFViewer
{
    class StatusBusy: IDisposable
    {
        string _oldStatus;
        Cursor _oldCursor;
        IStatusBusyControl _control;
        

        public StatusBusy(IStatusBusyControl control, string statusText)
        {
            if (control == null) { throw new ArgumentNullException("control"); }

            _control = control;

            _oldStatus = _control.StatusText;
            _oldCursor = _control.Cursor;

            _control.StatusText = statusText;
            _control.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
        }

        #region IDisposable
        // IDisposable
        private bool _disposedValue = false; // Evitar llamadas recursivas

        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
                if (disposing)
                {
                    _control.StatusText = _oldStatus;
                    _control.Cursor = _oldCursor;
                }
            _disposedValue = true;
        }

        public void Dispose()
        {
            //No cambiar este codigo, limpiar codigo en la funcion protected void Dispose(bool disposin)
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    interface IStatusBusyControl
    {
        Cursor Cursor { get; set; }
        String StatusText { get; set; }
    }

}
