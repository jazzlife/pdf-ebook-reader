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
        static AppPaths()
        {
            // For testing
            const string TestPath = @"E:\temp";
            string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            
            if (Directory.Exists(TestPath))
            {
                _cacheFolderPath = Path.Combine(TestPath, appName + "-cache");
                _dataFolderPath = Path.Combine(TestPath, appName + "-data");
            }
            else 
            {
                _cacheFolderPath = Path.Combine(Path.GetTempPath(), appName + "-cache");
                _dataFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    appName);
            }

            if (!Directory.Exists(_cacheFolderPath)) { Directory.CreateDirectory(_cacheFolderPath); }
            if (!Directory.Exists(_dataFolderPath)) { Directory.CreateDirectory(_dataFolderPath); }
        }

        static String _dataFolderPath;
        public static string DataFolderPath { get { return _dataFolderPath; } }

        static string _cacheFolderPath;
        public static string CacheFolderPath { get { return _cacheFolderPath; } }

    }
}
