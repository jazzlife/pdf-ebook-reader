using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using PdfBookReader.Utils;
using System.ComponentModel;
using System.Linq;

namespace PdfBookReader.Metadata
{
    [DataContract]
    public class BookLibrary
    {
        String _filename;

        [DataMember(Name = "Books")]
        List<Book> _books;

        // NOTE: in data contract serialization, ctor never runs
        public BookLibrary() 
        {
            Filename = DefaultFilename;
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

        public List<Book> Books
        {
            get
            {
                if (_books == null) { _books = new List<Book>(); }
                return _books;
            }
            private set { _books = value; }
        }


        public void AddFiles(IEnumerable<String> files)
        {
            foreach (String file in files) 
            {
                // Skip duplicates
                if (Books.FirstOrDefault(x => x.Filename.EqualsIC(file)) == null)
                {
                    Books.Add(new Book(file)); 
                }
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
            BookLibrary library = XmlHelper.Deserialize<BookLibrary>(filename);
            library.Filename = filename;

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
            foreach (Book b in Books)
            {
                if (!File.Exists(b.Filename))
                {
                    Books.Remove(b);
                }
            }
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
