using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Targets;
using NLog;
using NLog.Layouts;
using System.Diagnostics;

namespace Utils
{
    public static class LogUtils
    {
        public static IEnumerable<String> GetLogFiles()
        {
            IEnumerable<Target> targets = LogManager.Configuration.AllTargets;

            foreach (Target t in targets)
            {
                FileTarget ft = t as FileTarget;
                if (ft != null)
                {
                    String fileName = SimpleLayout.Evaluate(ft.FileName.ToString());
                    fileName = fileName.Trim('\'');
                    fileName = fileName.Replace(@"\\", @"\");

                    yield return fileName;
                }
            }
        }

        public static void ShowLogInExplorer()
        {
            String file = GetLogFiles().First();
            Process.Start("explorer.exe", String.Format("/select,\"{0}\"", file));
        }


    }
}
