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

    /// <summary>
    /// Cache of physical page content object. 
    /// Remember to Save() when exiting the app.
    /// </summary>
    [TheadSafe]
    [DataContract]
    class PageContentCache
    {
        object MyLock = new object();
        const string CacheDirName = "PdfEBookReaderCache";

        // Serialized fields
        // Mapping fullFilePath to Guid for the file
        [DataMember(Name = "Guids")]
        Dictionary<string, Guid> _guidSet = new Dictionary<string, Guid>();

        // Mapping key(guid,pageNum,width) to a PageContent object
        // Note: never pass info objects directly, always make a copy adding/stripping the bitmap
        [DataMember(Name = "ContentInfos")]
        Dictionary<string, PageContent> _contentInfoSet = new Dictionary<string, PageContent>();


        private PageContentCache() { }

        public static PageContentCache Load()
        {
            PageContentCache cache = null;
            try
            {
                cache = XmlHelper.Deserialize<PageContentCache>(DataFilePath);
                // Serialization does not call ctor
                cache.MyLock = new object();
            }
            catch (FileNotFoundException)
            {
                // This is OK, no message needed
                cache = new PageContentCache();
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception reading: " + DataFilePath + " " + e.Message);
                cache = new PageContentCache();
            }

            // TODO: check required bitmaps exist to aovid orphan entries

            return cache;
        }

        public void Save()
        {
            lock (MyLock)
            {
                XmlHelper.Serialize(this, DataFilePath);
            }
        }

        static String CacheFolderPath
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

        static String DataFilePath
        {
            get { return Path.Combine(CacheFolderPath, "PageContentCache.xml"); }
        }

        /// <summary>
        /// True if cache contains the page, false otherwise.
        /// May return false positives -- only checks the in-memory 
        /// table, not the disk files.
        /// </summary>
        /// <param name="fullBookPath"></param>
        /// <param name="pageNum"></param>
        /// <param name="contentWidth"></param>
        /// <returns></returns>
        public bool ContainsPage(String fullBookPath, int pageNum, int contentWidth)
        {
            lock (MyLock)
            {
                String key = GetKey(fullBookPath, pageNum, contentWidth);
                if (key == null) { return false; }
                return _contentInfoSet.ContainsKey(key);
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
                String key = GetKey(fullBookPath, pageNum, contentWidth);
                if (key == null) { return null; }

                // Return a *copy*
                PageContent cachedPage;
                if (!_contentInfoSet.TryGetValue(key, out cachedPage))
                {
                    return null;
                }

                // Load image
                String imageFilename = GetImageFilename(key);
                if (!File.Exists(imageFilename)) { return null; }

                Bitmap image = new Bitmap(imageFilename);

                PageContent ppi = new PageContent(cachedPage.PageNum, image, cachedPage.Layout);
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
                ArgCheck.NotNull(ppi, "ppi");
                ArgCheck.NotNull(ppi.Image, "ppi.Image");
                

                String key = GetKey(fullBookPath, ppi.PageNum, contentWidth, true);


                // Make and save a copy. Do not hog the handle to the bitmap (in the hashtable). 
                PageContent copyToSave = new PageContent(ppi.PageNum, ppi.Layout);
               
                // Remove old one if it exists (just in case, usuall not there)
                _contentInfoSet.Remove(key);
                _contentInfoSet.Add(key, copyToSave);

                String imageFilename = GetImageFilename(key);
                ppi.Image.Save(imageFilename);
            }

            // Raise an event
            if (PageCached != null)
            {
                PageCached(this, new PageCachedEventArgs(fullBookPath, ppi.PageNum, contentWidth));
            }
        }

        public event EventHandler<PageCachedEventArgs> PageCached;

        /// <summary>
        /// Get the key, or null if it does not exist
        /// </summary>
        String GetKey(String filename, int pageNum, int contentWidth, bool createNew = false)
        {
            lock (MyLock)
            {
                // Lookup the guid
                Guid id;
                if (_guidSet.TryGetValue(filename, out id))
                {
                    return GetKey(id, pageNum, contentWidth);
                }

                // Not found, create new
                if (createNew)
                {
                    id = Guid.NewGuid();
                    _guidSet.Add(filename, id);
                    return GetKey(id, pageNum, contentWidth);
                }

                // Not found, return null
                return null;
            }
        }
        String GetKey(Guid id, int pageNum, int contentWidth)
        {
            return contentWidth + "_" + id + "_" + pageNum;
        }

        String GetImageFilename(String key)
        {
            return Path.Combine(CacheFolderPath, key + ".png"); 
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
