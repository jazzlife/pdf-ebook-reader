using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BookReader.Utils;

namespace BookReader.Model
{
    static class DefaultSettings
    {
        static readonly string AppName = Assembly.GetExecutingAssembly().FullName;

        static String AppData
        {
            get
            {
                return PathX.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    AppName);
            }
        }

        /// <summary>
        /// App data in non-roaming user profile 
        /// (won't get sync'd over the network)
        /// </summary>
        static String LocalAppData
        {
            get
            {
                return PathX.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AppName);
            }
        }

        public static String LibraryFile
        {
            get
            {
                return PathX.Combine(AppData, "Library", "BookLibrary.xml");
            }
        }

        public static String CacheFolder
        {
            get
            {
                return PathX.Combine(LocalAppData, "Cache");
            }
        }

    }
}
