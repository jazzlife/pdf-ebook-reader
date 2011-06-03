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

namespace PdfBookReader.Render
{
    class PhysicalPageInfoCache
    {
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
            get { return Path.Combine(CacheFolderPath, "PathToIdMap.xml"); }
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

        internal PhysicalPageInfo GetPage(String fullBookPath, int pageNum, int contentWidth)
        {
            // FullPath is unique, but unwieldy for use in filenames. 
            // Instead, we use an ID
            Guid id;

            // No cache entry
            if (!PathToId.TryGetValue(fullBookPath, out id)) { return null; }

            String filename = GetFilename(id, pageNum, contentWidth);
            if (!File.Exists(filename)) { return null; }

            PhysicalPageInfo ppi = null;
            try 
            {
                ppi = PhysicalPageInfo.Load(filename);
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed loading image: " + filename + e.Message);
                ppi = null;
            }
            return ppi;
        }

        public void SavePage(PhysicalPageInfo ppi, String fullBookPath, int contentWidth)
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

        String GetFilename(Guid id, int pageNum, int contentWidth)
        {
            return Path.Combine(CacheFolderPath, "w" + contentWidth + "_" + id + "_p" + pageNum + ".xml");
        }

    }

}
