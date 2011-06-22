using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using NLog;
using BookReaderTest.TestUtils;

namespace Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            LogManager.GetLogger("Simple.Test").Debug("=== Session start at: " + DateTime.Now + " ===");

            NUnit.Gui.AppEntry.Main(new string[]
            {
                Assembly.GetExecutingAssembly().Location, 
                //"/run"
            });
        }
    }
}
