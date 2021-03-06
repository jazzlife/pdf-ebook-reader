﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using BookReader.Utils;

namespace BookReader.Render.Cache
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

        public PageKey(Guid bookId, int pageNum, int screenWidth)
        {
            BookId = bookId;
            PageNum = pageNum;
            ScreenWidth = screenWidth;
        }

        public String Rep { get { return "{0}_p{1}_w{2}".F(BookId, PageNum, ScreenWidth); } }

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
            return "PageKey: p={0} w={1} b={2}".F(PageNum, ScreenWidth, BookId);
        }
    }
}
