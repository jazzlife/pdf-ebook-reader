using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;
using System.Runtime.Serialization;
using System.IO;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Rendered physical page with the detected layout info. 
    /// </summary>
    [DataContract(Name = "PageContent")]
    class PageContent : IDisposable
    {
        [DataMember (Name = "PageNum")]
        public readonly int PageNum; // page number in document, 1-n
        
        [DataMember (Name = "Layout")]
        PageLayoutInfo _layout; // content layout

        // Serialized separately
        Bitmap _image; // physical page image

        // Not serialized
        /// <summary>
        /// Distance between content bounds top and creen page top 
        /// screen.Top - countentBounds.Top
        /// </summary>
        public int TopOnScreen = 0;

        public PageContent(int pageNum, Bitmap image, PageLayoutInfo layout)
        {
            ArgCheck.GreaterThanOrEqual(pageNum, 1, "pageNum");
            ArgCheck.NotNull(image);

            PageNum = pageNum;
            _image = image;
            _layout = layout;
        }

        public Bitmap Image 
        {
            get { return _image; }
            private set { _image = value; }
        }

        public PageLayoutInfo Layout { get { return _layout; } }

        // For convenience
        public int BottomOnScreen
        {
            get { return TopOnScreen + Layout.Bounds.Height; }
            set { TopOnScreen = value - Layout.Bounds.Height; }
        }

        public void Dispose()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
                _layout = null;
            }
        }

        public override string ToString()
        {
            return "PhysicalPage #" + PageNum + " TopOnScreen = " + TopOnScreen;
        }

        #region Serialization

        public void Save(String dataFileName)
        {
            XmlHelper.Serialize(this, dataFileName);
            String imageFileName = GetImageFileName(dataFileName);
            this.Image.Save(imageFileName);
        }

        public static PageContent Load(String dataFileName)
        {
            if (!File.Exists(dataFileName)) { throw new FileNotFoundException("No data file" + dataFileName); }

            String imageFileName = GetImageFileName(dataFileName);
            if (!File.Exists(imageFileName)) { throw new FileNotFoundException("No image file: " + imageFileName); }

            PageContent ppi = XmlHelper.Deserialize<PageContent>(dataFileName);
            Bitmap image = new Bitmap(imageFileName);
            ppi.Image = image;

            return ppi;
        }

        static String GetImageFileName(String dataFileName)
        {
            return Path.Combine(Path.GetDirectoryName(dataFileName),
                Path.GetFileNameWithoutExtension(dataFileName) + ".png");
        }

        #endregion
    }

}