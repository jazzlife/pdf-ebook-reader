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
        [DataMember(Name = "ID")]
        Guid _id = Guid.NewGuid();

        /// <summary>
        /// Unique ID, used for caching. 
        /// </summary>
        public Guid ID 
        {
            get
            {
                if (_id == Guid.Empty) { _id = Guid.NewGuid(); }
                return _id;
            }
        }

        [DataMember]
        public String Filename { get; private set; }

        [DataMember]
        public String Title { get; private set; }

        // TODO: add thumbnail etc.
        public Book(String filename)
        {
            Filename = filename;
            Title = Path.GetFileNameWithoutExtension(filename);
            _id = ID; // make sure it gets generated
        }
    }
}
