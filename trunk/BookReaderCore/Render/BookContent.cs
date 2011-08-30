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
        PageLayout GetPageLayout(int pageNum, int screenWidth);
        PageImage GetPageImage(int pageNum, int screenWidth);
        
        PositionInBook Position { get; set; }
        int PageCount { get; }

        // TODO: consider merging into BookContent
        DW<IBookProvider> BookProvider { get; }
    }

    class BookContent : IBookContent
    {
        readonly Book _book;
        public Book Book { get { return _book; } }
        DW<IBookProvider> _bookProvider;

        protected Dictionary<int, PageLayout> Layouts;
        protected DW<PageImageCache> ImageCache;

        IPageLayoutStrategy _layoutStrategy;

        public BookContent(Book book, DW<PageImageCache> imageCache = null)
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

            _layoutStrategy = RenderFactory.Default.GetLayoutStrategy();

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

        public PageLayout GetPageLayout(int pageNum, int screenWidth)
        {
            if (!Layouts.ContainsKey(pageNum))
            {
                PageLayout layout = CreatePageLayout(pageNum, screenWidth);
                Layouts.Add(pageNum, layout);
            }

            return Layouts[pageNum];
        }

        PageLayout CreatePageLayout(int pageNum, int screenWidth)
        {
            PageLayout layout = _layoutStrategy.DetectLayoutFromBook(this, pageNum);
            if (layout == null)
            {
                // create from page image
                if (_lastPageWidth == 0) { _lastPageWidth = 800; }

                PageImage pi = GetPageImage(pageNum, screenWidth, _lastPageWidth);
                layout = _layoutStrategy.DetectLayoutFromImage(pi.Image);
                pi.Return();
            }
            return layout;
        }

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
            return GetPageImage(pageNum, screenWidth, 0);
        }

        PageImage GetPageImage(int pageNum, int screenWidth, int pageWidth)
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
                image = CreatePageImage(key, pageWidth);

                // Save to cache
                if (ImageCache != null)
                {
                    image.DisposeOnReturn = false;
                    ImageCache.o.Add(key, image);
                }

                return image;
            }
        }

        int _lastPageWidth = 0;
        protected PageImage CreatePageImage(PageKey key, int pageWidth)
        {
            if (pageWidth == 0)
            {
                var layout = GetPageLayout(key.PageNum, key.ScreenWidth);
                pageWidth = ((float)key.ScreenWidth / layout.UnitBounds.Width).Round();
            }
            _lastPageWidth = pageWidth;

            Bitmap b = BookProvider.o.RenderPageImage(key.PageNum, new Size(pageWidth, int.MaxValue), RenderQuality.Optimal);

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
