using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PdfBookReader;
using PdfBookReader.UI;
using NLog;
using NDesk.Options;
using PdfBookReader.Render;

namespace PdfBookReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger log = LogManager.GetLogger("PdfBookReader");
            log.Debug("");
            log.Debug("=== Session start ===");

            var o = new OptionSet()
            {
                { "nocache", "Do not use page prefetch/cache", x => { if (x != null) DefaultRenderFactory.NoCache = true; } }
            };

            // Case-insensitive
            var args = Environment.GetCommandLineArgs().Select(x => x.ToLowerInvariant());
            o.Parse(args);



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
