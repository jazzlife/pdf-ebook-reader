using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PDFViewer.Reader.Utils
{
    public static class PathX
    {
        public static String Combine(String basePath, params string[] parts)
        {
            string path = basePath;
            foreach (string part in parts)
            {
                path = Path.Combine(path, part);
            }
            return path;
        }

        // Ensure a directory exists
        public static void EnsureDirectoryExists(String path)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(path));
            EnsureDirectoryExists(dir);
        }

        public static void EnsureDirectoryExists(DirectoryInfo dir)
        {
            if (dir.Exists) { return; }
            
            EnsureDirectoryExists(dir.Parent);
            dir.Create();
        }
    }
}
