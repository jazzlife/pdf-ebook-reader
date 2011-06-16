using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using System.Diagnostics;
using System.IO;
using log4net.Config;
using log4net;
using log4net.Layout;

namespace PdfBookReader.Utils
{
    public static class LogUtils
    {
        #region Config
        static RollingFileAppender Appender;
        public static string LogFilePath
        {
            get { return Appender.File; }
        }

        static LogUtils()
        {
            // Configure the appender
            Appender = new RollingFileAppender();
            Appender.AppendToFile = true;

            Appender.RollingStyle = RollingFileAppender.RollingMode.Size;
            Appender.MaxSizeRollBackups = 10;
            Appender.MaxFileSize = 1 * 1024 * 1024; // 1Mb
            Appender.StaticLogFileName = true;

            String appName = Process.GetCurrentProcess().ProcessName;
            Appender.File = Path.Combine(Path.GetTempPath(), appName + ".log");
            //Appender.File = @"C:\temp\" + appName + ".log";

            Appender.Layout = new PatternLayout("- %date{yyyy-MM-ddTHH:mm:ss.fffzzz} %5level [%thread] %logger %message %newline");

            Appender.ActivateOptions();

            BasicConfigurator.Configure(Appender);

            LogManager.GetLogger("App").Debug("=== Session started ===");
        }
        #endregion

        public static void ShowLogInExplorer()
        {
            Process.Start("explorer.exe", "/select,\"" + LogFilePath + "\"");
        }

        public static ILog GetLogger()
        {
            // Figure out the caller type
            StackTrace caller = new StackTrace();
            String name = caller.GetFrame(1).GetMethod().DeclaringType.Name;
            return LogManager.GetLogger(name);
        }

        public static ILog GetLogger(String name)
        {
            return LogManager.GetLogger(name);
        }
    }
}
