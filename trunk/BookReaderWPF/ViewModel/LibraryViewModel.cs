using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using BookReader.Base.ViewModel;
using BookReader.Model;
using BookReader.Utils;
using System.Windows.Input;

namespace BookReader.ViewModel
{
    class LibraryViewModel : ViewModelBase
    {
        public BookLibrary Library { get; private set; }

        public LibraryViewModel(BookLibrary library)
        {
            ArgCheck.NotNull(library);
            Library = library;
        }
    }
}
