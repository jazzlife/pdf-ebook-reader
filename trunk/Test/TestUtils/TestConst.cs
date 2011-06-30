using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BookReaderTest.TestUtils
{
    internal static class TestConst
    {
        public const String OutPath = @"E:\temp\Out";
        public const String PdfFilePath = @"E:\temp\TestPDFs";

        public static String GetPdfFile(String name) { return Path.Combine(PdfFilePath, name); }
        public static String GetOutFile(String originalName, String suffix) 
        {
            if (!Directory.Exists(OutPath)) { Directory.CreateDirectory(OutPath); }

            return Path.Combine(
                OutPath,
                Path.GetFileNameWithoutExtension(originalName) + suffix); 
        }

        public static IEnumerable<String> GetAllPdfFiles(bool recurseDirs = false)
        {
            SearchOption opt = recurseDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(PdfFilePath, "*.pdf", opt);

            foreach (var f in files)
            {
                yield return Path.Combine(PdfFilePath, f);
            }
        }

    }
}
