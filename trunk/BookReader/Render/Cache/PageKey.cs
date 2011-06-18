using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using PdfBookReader.Utils;

namespace PdfBookReader.Render.Cache
{
    /// <summary>
    /// Key for cached PageContent items
    /// </summary>
    [DataContract]
    class PageKey
    {
        [DataMember]
        public readonly Guid BookId;
        
        [DataMember]
        public readonly int PageNum;
        
        [DataMember]
        public readonly int ScreenWidth;

        public readonly String Rep;

        public PageKey(Guid bookId, int pageNum, int screenWidth)
        {
            BookId = bookId;
            PageNum = pageNum;
            ScreenWidth = screenWidth;

            Rep = "{0}_p{1}_w{2}".F(BookId, PageNum, ScreenWidth);
        }

        public override int GetHashCode()
        {
            return Rep.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PageKey that = obj as PageKey;
            if (that == null) { return false; }

            return Rep.Equals(that.Rep);
        }

        public override string ToString()
        {
            return Rep;
        }
    }
}
