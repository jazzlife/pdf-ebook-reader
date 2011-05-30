using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PDFViewer.Reader.Utils;
using System.ComponentModel;
using System.Linq;

namespace PDFViewer.Reader
{
    [DataContract]
    public class BookLibrary
    {
        public String Filename { get; private set; }

        [DataMember]
        public List<Book> Books { get ; set; }

        public BookLibrary()
        {
            Filename = DefaultFilename;
            Books = new List<Book>();
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

        public static BookLibrary Load(String filename)
        {
            BookLibrary library = XmlHelper.Deserialize<BookLibrary>(filename);
            library.Filename = filename;            
            return library;
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

    [DataContract]
    public class Book
    {
        [DataMember]
        public String Filename { get; set; }

        [DataMember]
        public String Title { get; set; }

        // TODO: add thumbnail etc.
        public Book(String filename)
        {
            Filename = filename;
            Title = Path.GetFileNameWithoutExtension(filename);
        }
    }
}
