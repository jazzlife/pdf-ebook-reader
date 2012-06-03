using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using BookReader.ViewModel;

namespace BookReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();

            // Create the ViewModel
            var viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;

            // Save the options when window is closed
            window.Closed += ((src,ea) => viewModel.Close());

            window.Show();
        }
    }
}
