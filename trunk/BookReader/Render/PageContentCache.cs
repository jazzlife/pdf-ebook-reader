using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfBookReader.Metadata;
using System.Runtime.Serialization;
using System.IO;
using System.Diagnostics;
using PdfBookReader.Utils;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace PdfBookReader.Render
{
    [TheadSafe]
    class PageContentCache
    {
        readonly object MyLock = new object();

        const string CacheDirName = "PdfEBookReaderCache";
        String CacheFolderPath
        {
            get
            {
                // For testing
                String path = Path.Combine(@"E:\temp", CacheDirName);
                if (Directory.Exists(path)) { return path; }

                // Real
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                path = Path.Combine(path, CacheDirName);

                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                return path;
            }
        }

        #region Path to ID map
        Dictionary<string, Guid> _pathToId;

        Dictionary<string, Guid> PathToId
        {
            get
            {
                if (_pathToId == null) { LoadPathToIdMap(); }
                return _pathToId;
            }
        }

        String PathToIdFilePath
        {
            get 
            {
                return Path.Combine(CacheFolderPath, "PathToIdMap.xml");
            }
        }

        void SavePathToIdMap()
        {
            XmlHelper.Serialize<Dictionary<string, Guid>>(PathToId, PathToIdFilePath);
        }

        void LoadPathToIdMap()
        {
            try
            {
                _pathToId = XmlHelper.Deserialize<Dictionary<string, Guid>>(PathToIdFilePath);
            }
            catch (FileNotFoundException)
            {
                // This is OK, no message needed
                _pathToId = new Dictionary<string, Guid>();
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception reading: " + PathToIdFilePath + " " + e.Message);
                _pathToId = new Dictionary<string, Guid>();
            }
        }
        #endregion 

        /// <summary>
        /// True if cache contains the page, false otherwise.
        /// NOTE: in multithreaded applications, things can happen between 
        /// this and the next line, so be careful with it. 
        /// </summary>
        /// <param name="fullBookPath"></param>
        /// <param name="pageNum"></param>
        /// <param name="contentWidth"></param>
        /// <returns></returns>
        public bool ContainsPage(String fullBookPath, int pageNum, int contentWidth)
        {
            lock (MyLock)
            {
                Guid id;
                if (!PathToId.TryGetValue(fullBookPath, out id)) { return false; }
                String filename = GetFilename(id, pageNum, contentWidth);
                return File.Exists(filename);
            }
        }

        /// <summary>
        /// Get content page from cache, null if it doesn't exist.
        /// </summary>
        /// <param name="fullBookPath"></param>
        /// <param name="pageNum"></param>
        /// <param name="contentWidth"></param>
        /// <returns></returns>
        public PageContent GetPage(String fullBookPath, int pageNum, int contentWidth)
        {
            lock (MyLock)
            {

                // FullPath is unique, but unwieldy for use in filenames. 
                // Instead, we use an ID
                Guid id;

                // No cache entry
                if (!PathToId.TryGetValue(fullBookPath, out id)) { return null; }

                String filename = GetFilename(id, pageNum, contentWidth);
                if (!File.Exists(filename)) { return null; }

                PageContent ppi = null;
                try
                {
                    ppi = PageContent.Load(filename);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Failed loading image: " + filename + e.Message);
                    ppi = null;
                }
                return ppi;
            }
        }

        /// <summary>
        /// Save content page to cache
        /// </summary>
        /// <param name="ppi"></param>
        /// <param name="fullBookPath"></param>
        /// <param name="contentWidth"></param>
        public void SavePage(PageContent ppi, String fullBookPath, int contentWidth)
        {
            lock (MyLock)
            {
                // TODO: delete items from cache occasionally (e.g. when requested
                // width of saved item changes). Easy to delete wNNN_*.*

                // Get ID or create new if necessary
                Guid id;
                if (!PathToId.TryGetValue(fullBookPath, out id))
                {
                    id = Guid.NewGuid();
                    PathToId.Add(fullBookPath, id);
                    SavePathToIdMap();
                }

                String filename = GetFilename(id, ppi.PageNum, contentWidth);
                ppi.Save(filename);
            }

            if (PageCached != null)
            {
                PageCached(this, new PageCachedEventArgs(fullBookPath, ppi.PageNum, contentWidth));
            }
        }

        public event EventHandler<PageCachedEventArgs> PageCached;

        String GetFilename(Guid id, int pageNum, int contentWidth)
        {
            return Path.Combine(CacheFolderPath, "w" + contentWidth + "_" + id + "_p" + pageNum + ".xml");
        }

    }

    class PageCachedEventArgs : EventArgs
    {
        readonly public String FullPath;
        readonly public int PageNum;
        readonly public int Width;

        public PageCachedEventArgs(String fullName, int pageNum, int width)
        {
            FullPath = fullName;
            PageNum = pageNum;
            Width = width;
        }

    }

}
