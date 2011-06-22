using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace BookReader
{
    static class AppPaths
    {
        readonly static object MyStaticLock = new object();

        static String _cacheFolderPath;
        public static String CacheFolderPath
        {

            get
            {
                lock (MyStaticLock)
                {
                    if (_cacheFolderPath == null)
                    {
                        // For testing
                        String basePath = @"E:\temp";
                        if (!Directory.Exists(basePath))
                        {
                            basePath = Path.GetTempPath();
                        }

                        String dirName = Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "-cache";
                        _cacheFolderPath = Path.Combine(basePath, dirName);
                    }
                    if (!Directory.Exists(_cacheFolderPath)) { Directory.CreateDirectory(_cacheFolderPath); }
                    return _cacheFolderPath;
                }
            }
        }
    }
}
