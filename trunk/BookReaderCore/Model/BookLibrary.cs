using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using BookReader.Utils;
using System.ComponentModel;
using System.Linq;

namespace BookReader.Model
{
    [DataContract]
    public class BookLibrary
    {
        [DataMember(Name = "CurrentBookId")]
        Guid _currentBookId;

        [DataMember(Name = "Books")]
        List<Book> _books;

        String _filename;

        // NOTE: in data contract serialization, ctor never runs
        public BookLibrary() 
        {
            Filename = DefaultFilename;
            _books = new List<Book>();
        }

        public event EventHandler CurrentBookChanged;
        public event EventHandler BooksChanged;

        /// <summary>
        /// Current book changed, or the page position within the current book changed.
        /// </summary>
        public event EventHandler BookPositionChanged;

        void Initialize()
        {
            if (_books == null) { _books = new List<Book>(); }
            foreach (Book b in Books)
            {
                b.CurrentPositionChanged += OnBookPositionChanged;
            }
        }

        public String Filename
        {
            get
            {
                if (_filename == null) { _filename = DefaultFilename; }
                return _filename;
            }
            private set { _filename = value; }
        }

        public IList<Book> Books
        {
            get
            {
                if (_books == null) { _books = new List<Book>(); }
                return _books.AsReadOnly();
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

                // Fire events
                if (CurrentBookChanged != null) { CurrentBookChanged(this, EventArgs.Empty); }
                if (BookPositionChanged != null) { BookPositionChanged(this, EventArgs.Empty); }
            }
        }

        // For convenience, re-fire the book position changed event
        void OnBookPositionChanged(object sender, EventArgs e)
        {
            if (BookPositionChanged != null) { BookPositionChanged(this, EventArgs.Empty); }
        }

        public void AddFiles(IEnumerable<String> files)
        {
            var booksDict = _books.ToDictionary(x => Path.GetFullPath(x.Filename));
            
            var filesToAdd = files.Where(x => !booksDict.ContainsKey(Path.GetFullPath(x)));
            
            var booksToAdd = files.Select(x => new Book(x));

            AddBooks(booksToAdd);
        }

        public void AddBook(Book book)
        {
            ArgCheck.NotNull(book, "book");

            _books.Add(book);
            book.CurrentPositionChanged += OnBookPositionChanged;

            if (BooksChanged != null) { BooksChanged(this, EventArgs.Empty); }
        }
        public void AddBooks(IEnumerable<Book> booksToAdd)
        {
            foreach(var book in booksToAdd)
            {
                _books.Add(book);
                book.CurrentPositionChanged += OnBookPositionChanged;
            }
            if (BooksChanged != null) { BooksChanged(this, EventArgs.Empty); }            
        }

        public void RemoveBook(Book book)
        {
            ArgCheck.NotNull(book, "book");

            _books.Remove(book);
            book.CurrentPositionChanged -= OnBookPositionChanged;

            if (BooksChanged != null) { BooksChanged(this, EventArgs.Empty); }
        }
        public void RemoveBooks(IEnumerable<Book> booksToRemove)
        {
            foreach (Book book in booksToRemove)
            {
                _books.Remove(book);
                book.CurrentPositionChanged -= OnBookPositionChanged;
            }

            if (BooksChanged != null) { BooksChanged(this, EventArgs.Empty); }
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

            // ctor not called by serializer, must initialize
            library.Initialize();

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

        public static String DefaultFilename
        {
            get
            {
                String myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return PathX.Combine(myDocs, "Books", "PdfEBookLibrary.xml");
            }
        }
    }

}
