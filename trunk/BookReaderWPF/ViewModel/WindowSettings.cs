using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BookReader.Properties;

namespace BookReader.ViewModel
{
    class WindowSettings
    {
        public WindowState WindowState
        {
            get 
            { 
                WindowState rv = Settings.Default.WindowState;
                if (rv == System.Windows.WindowState.Minimized)
                {
                    rv = System.Windows.WindowState.Normal;                    
                }
                return rv;
            }
            set { Settings.Default.WindowState = value; }
        }

        public int Width
        {
            get
            {
                int rv = Settings.Default.Width;
                rv = (int)Math.Min(rv, SystemParameters.VirtualScreenWidth);
                return rv;
            }
            set { Settings.Default.Width = value; }
        }

        public int Height
        {
            get
            {
                int rv = Settings.Default.Height;
                rv = (int)Math.Min(rv, SystemParameters.VirtualScreenHeight);
                return rv;
            }
            set { Settings.Default.Height = value; }
        }

        public int Left
        {
            get
            {
                int rv = Settings.Default.Left;
                rv = (int)Math.Min(rv, System.Windows.SystemParameters.VirtualScreenWidth - Width);
                return rv;
            }
            set { Settings.Default.Left = value; }
        }

        public int Top
        {
            get
            {
                int rv = Settings.Default.Top;
                rv = (int)Math.Min(rv, System.Windows.SystemParameters.VirtualScreenHeight - Height);
                return rv;
            }
            set { Settings.Default.Top = value; }
        }

        public void Save()
        {
            Settings.Default.Save();
        }

    }
}
