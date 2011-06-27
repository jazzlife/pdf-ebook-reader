using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;
using System.Drawing;
using BookReader.Render.Cache;

namespace BookReader.Render
{
    class PageImage : DW<Bitmap>, ICachedDisposable
    {
        public readonly PageKey Key;

        public PageImage(PageKey key, Bitmap b) : base(b)
        {
            ArgCheck.NotNull(key, "key");
            ArgCheck.NotNull(b, "bitmap");

            Key = key;
        }

        public Bitmap Image { get { return o; } }

        #region ICachedDisposable

        /// <summary>
        /// Dispose object when Return() is called. 
        /// Set to false to manage object disposal manually (e.g. prefetch/cache it)
        /// </summary>
        internal bool DisposeOnReturn = true;

        bool _inUse = true;
        public void Return()
        {
            _inUse = false;
            if (DisposeOnReturn && !IsDisposed) { DisposeItem(); }
        }

        public bool InUse
        {
            get { return _inUse; }
        }

        internal void Reuse()
        {
            _inUse = true;
        }

        #endregion


    }
}
