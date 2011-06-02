using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace PdfBookReader.Metadata
{
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
