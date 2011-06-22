using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace BookReader.Model
{
    [DataContract]
    public class Book
    {
        Guid _id;
        PositionInBook _currentPosition;

        /// <summary>
        /// Unique ID, used for caching. 
        /// </summary>
        [DataMember(Name = "ID")]
        public Guid Id
        {
            get
            {
                if (_id == Guid.Empty) { _id = Guid.NewGuid(); }
                return _id;
            }
            private set { _id = value; }
        }

        [DataMember]
        public String Filename { get; private set; }

        [DataMember]
        public String Title { get; private set; }

        [DataMember(Name = "CurrentPosition")]
        public PositionInBook CurrentPosition 
        {
            get { return _currentPosition; }
            set
            {
                if (value != null && value.Equals(_currentPosition)) { return; }

                _currentPosition = value;
                if (CurrentPositionChanged != null) { CurrentPositionChanged(this, EventArgs.Empty); }
            }
        }

        public event EventHandler CurrentPositionChanged;

        // TODO: add thumbnail etc.
        public Book(String filename)
        {
            Filename = filename;
            Title = Path.GetFileNameWithoutExtension(filename);
            
            _id = Guid.NewGuid(); // make sure it gets generated
        }
    }
}
