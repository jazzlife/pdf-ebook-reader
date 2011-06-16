using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PdfBookReader;
using PdfBookReader.UI;
using NLog;

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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
