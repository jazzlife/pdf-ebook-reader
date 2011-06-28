using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Model;
using BookReader.Render.Layout;
using BookReader.Utils;
using BookReader.Render.Cache;
using System.IO;
using BookReader.Properties;
using System.Drawing;

namespace BookReader.Render
{
    // Heavy items should be lazy-created
    interface IBookContent : IDisposable
    {
        Book Book { get; }
        PageLayout GetPageLayout(int pageNum);
        PageImage GetPageImage(int pageNum, int screenWidth);
        
        PositionInBook Position { get; set; }
        int PageCount { get; }

        // TODO: consider merging into BookContent
        DW<IBookProvider> BookProvider { get; }
    }

    abstract class BookContentBase : IBookContent
    {
        readonly Book _book;
        public Book Book { get { return _book; } }
        DW<IBookProvider> _bookProvider;

        protected Dictionary<int, PageLayout> Layouts;
        protected DW<PageImageCache> ImageCache;

        public BookContentBase(Book book, DW<PageImageCache> imageCache = null)
        {
            ArgCheck.NotNull(book, "book");
            _book = book;

            // null is ok
            ImageCache = imageCache;

            // Load layouts 
            Layouts = new Dictionary<int, PageLayout>();
            if (Settings.Default.Cache_SaveLayouts)
            {
                Layouts = XmlHelper.DeserializeOrDefault(LayoutsFile, Layouts);
            }

            // Slightly hacky but best way to do it
            // -- set the book position info if it's null
            if (Book.CurrentPosition == null)
            {
                Book.CurrentPosition = PositionInBook.FromPhysicalPage(1, BookProvider.o.PageCount);
            }
        }

        public void Save()
        {
            if (Settings.Default.Cache_SaveLayouts)
            {
                XmlHelper.Serialize(Layouts, LayoutsFile);
            }
        }

        protected string LayoutsFile { get { return Path.Combine(AppPaths.DataFolderPath, "Layouts_" + _book.Id + ".xml"); } }

        public PageLayout GetPageLayout(int pageNum)
        {
            if (!Layouts.ContainsKey(pageNum))
            {
                // create the layout
                PageLayout layout = CreatePageLayout(pageNum);
                Layouts.Add(pageNum, layout);
            }

            return Layouts[pageNum];
        }

        protected abstract PageLayout CreatePageLayout(int pageNum);

        public DW<IBookProvider> BookProvider
        {
            get 
            {
                if (_bookProvider == null)
                {
                    _bookProvider = RenderFactory.Default.NewBookProvider(Book.Filename);
                }
                return _bookProvider;
            }
        }

        public PageImage GetPageImage(int pageNum, int screenWidth)
        {
            // TODO: revise locking
            lock (this)
            {

                PageKey key = new PageKey(Book.Id, pageNum, screenWidth);
                PageImage image;

                // Try to get from cache
                if (ImageCache != null)
                {
                    image = ImageCache.o.Get(key);
                    if (image != null) { return image; }

                }

                // Create
                image = CreatePageImage(key);

                // Save to cache
                if (ImageCache != null)
                {
                    image.DisposeOnReturn = false;
                    ImageCache.o.Add(key, image);
                }

                return image;
            }
        }

        protected PageImage CreatePageImage(PageKey key)
        {
            var layout = GetPageLayout(key.PageNum);
            int pageWidth = ((float)key.ScreenWidth / layout.UnitBounds.Width).Round();

            Bitmap b = _bookProvider.o.RenderPageImage(key.PageNum, new Size(pageWidth, int.MaxValue), RenderQuality.Optimal);

            return new PageImage(key, b);
        }

        public PositionInBook Position
        {
            get { return Book.CurrentPosition; }
            set { Book.CurrentPosition = value; }
        }

        public int PageCount
        {
            get { return BookProvider.o.PageCount; }
        }

        public void Dispose()
        {
            if (_bookProvider != null)
            {
                _bookProvider.DisposeItem();
            }
            if (Layouts != null)
            {
                Layouts.Clear();
                Layouts = null;
            }
            if (ImageCache != null)
            {
                // Do not dispose, we don't own it
                ImageCache = null;
            }
        }

    }


    
}
