using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PdfBookReader.UI;
using NLog;
using BookReader.Render;
using BookReader;
using BookReader.Properties;

namespace BookReaderWinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger logger = LogManager.GetLogger("BookReader");
            logger.Debug("");
            logger.Debug("=== Session start (WinForms UI) ===");

            // Debug
            Settings.Default.Debug_DrawPageEnd = true;
            Settings.Default.Debug_DrawPageLayout = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
