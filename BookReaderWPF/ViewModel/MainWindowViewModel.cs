using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BookReader.Base.ViewModel;

namespace BookReader.ViewModel
{
    /// <summary>
    /// Bind window size/position/state to settings, 
    /// ensure they fit within the desktop.
    /// </summary>
    class MainWindowViewModel : ViewModelBase
    {
        public WindowSettings Settings { get; private set; }
        
        public MainWindowViewModel()
        {
            Settings = new WindowSettings();   
        }

        public void Close()
        {
            Settings.Save();
        }
    }
}
