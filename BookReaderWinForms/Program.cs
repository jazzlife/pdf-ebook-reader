using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PdfBookReader.UI;
using NDesk.Options;
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

            var o = new OptionSet()
            {
                { "nocache", "Do not use page prefetch/cache", x => { if (x != null) Settings.Default.NoCache = true; } }
            };

            // Debug
            Settings.Default.Debug_DrawPageEnd = true;
            Settings.Default.Debug_DrawPageLayout = true;

            // Case-insensitive
            var args = Environment.GetCommandLineArgs().Select(x => x.ToLowerInvariant());
            o.Parse(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
