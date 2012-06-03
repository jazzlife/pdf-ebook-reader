using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using BookReader.Utils;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using BookReader.Base.ViewModel;

namespace BookReader.Model
{
    [DataContract]
    public class BookLibrary : INotifyPropertyChanged
    {
        [DataMember(Name = "CurrentBookId")]
        Guid _currentBookId;

        [DataMember(Name = "Books")]
        ObservableCollection<Book> _pBooks;

        String _filename;
        ReadOnlyObservableCollection<Book> _roBooks;

        /// <summary>
        /// Gets the file where the library is stored.
        /// </summary>
        public String Filename
        {
            get
            {
                if (_filename == null) { _filename = DefaultSettings.LibraryFile; }
                return _filename;
            }
            private set { _filename = value; }
        }

        /// <summary>
        /// Gets the list of books 
        /// </summary>
        public ReadOnlyObservableCollection<Book> Books
        {
            get
            {
                if (_roBooks == null) 
                {
                    _roBooks = ReadOnlyObservableCollection<Book>(pBooks); 
                }
                return _roBooks.AsReadOnly();
            }
        }

        ObservableCollection<Book> pBooks
        {
            get
            {
                if (_pBooks == null) { _pBooks = new ObservableCollection<Book>(); }
                return _pBooks;
            }
        }

        // for performance only, ID must be updated / saved
        Book _currentBook = null;
        public Book CurrentBook
        {
            get
            {
                if (_currentBook == null)
                {
                    // If ID is empty, it won't find the book, so it remains null
                    _currentBook = Books.FirstOrDefault(x => x.Id == _currentBookId);
                }
                return _currentBook;
            }
            set
            {
                if (_currentBook == value) { return; }

                _currentBook = value;

                if (_currentBook == null) { _currentBookId = Guid.Empty; }
                else { _currentBookId = _currentBook.Id; }

                OnPropertyChanged(() => CurrentBook);
            }
        }

        public void AddFiles(IEnumerable<String> files)
        {
            var booksDict = pBooks.ToDictionary(x => Path.GetFullPath(x.Filename));            
            var filesToAdd = files.Where(x => !booksDict.ContainsKey(Path.GetFullPath(x)));            
            var booksToAdd = files.Select(x => new Book(x));

            AddBooks(booksToAdd);
        }

        public void AddBooks(IEnumerable<Book> booksToAdd)
        {
            // TODO: support bulk adding without firing events
            foreach (var book in booksToAdd)
            {
                pBooks.Add(book);
            }
        }

        public void AddBook(Book book)
        {
            ArgCheck.NotNull(book, "book");
            pBooks.Add(book);
        }

        public void RemoveBook(Book book)
        {
            ArgCheck.NotNull(book, "book");
            pBooks.Remove(book);
            if (CurrentBook == book) { CurrentBook = null; }
        }
        public void RemoveBooks(IEnumerable<Book> booksToRemove)
        {
            foreach (Book book in booksToRemove)
            {
                RemoveBook(book);
            }
        }

        /// <summary>
        /// Load the library.
        /// </summary>
        /// <param name="filename">XML file for the library</param>
        /// <param name="removeMissingBooks">Remove books that no longer exist on disk</param>
        /// <returns></returns>
        public static BookLibrary Load(String filename, bool removeMissingBooks = true)
        {
            BookLibrary library = XmlHelper.DeserializeOrDefault(filename, new BookLibrary());
            library.Filename = filename;

            // remove missing
            if (removeMissingBooks)
            {
                library.RemoveMissingBooks();
                library.Save();
            }

            return library;
        }

        /// <summary>
        /// Remove books that no longer exist on disk.
        /// </summary>
        public void RemoveMissingBooks()
        {
            var toRemove = Books.Where(x => !File.Exists(x.Filename)).ToArray();
            RemoveBooks(toRemove);
        }

        public void Save()
        {
            PathX.EnsureDirectoryExists(Filename);
            XmlHelper.Serialize<BookLibrary>(this, Filename);
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(MvvmUtils.GetPropertyName(expression)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
