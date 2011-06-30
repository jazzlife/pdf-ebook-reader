using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using BookReader.Render;
using BookReader.Render.Layout;
using System.Drawing;
using BookReader.Model;
using BookReaderTest.TestUtils;
using BookReader.Utils;
using System.Drawing.Imaging;
using PDFLibNet;
using BookReader.Render.BookFormats;
using System.Runtime.Serialization;
using System.IO;

namespace BookReaderTest.Render.Layout
{
    [TestFixture]
    public class PdfLayoutAccuracy
    {
        bool _haltTests = false;
        LayoutForm _form = new LayoutForm();

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _haltTests = false;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            LayoutTestCase.Save();
        }

        PDFWrapper GetPdfWrapper(IBookContent bc)
        {
            return ((PdfBookProvider)bc.BookProvider.o).InternalPdfWrapper.o;
        }

        DW<IBookContent> _bookContent;

        [TestCaseSource("GetTestCases")]
        public void Create(LayoutTestCase tcase)
        {
            if (_haltTests) { Assert.Ignore("Halt tests"); }

            if (_bookContent == null || _bookContent.o.Book.Filename != tcase.Filename)
            {
                if (_bookContent != null) 
                {
                    _bookContent.DisposeItem();
                    _bookContent = null;
                }

                Book book = new Book(tcase.Filename);
                _bookContent = DW.Wrap<IBookContent>(new PdfBookContent(book, null));
            }

            IPageLayoutStrategy alg = new PdfWordsLayoutStrategy();
            PageLayout layout = alg.DetectLayoutFromBook(_bookContent.o, tcase.PageNum);
            layout.SetPageSizeToScreen(600);

            // Get staus based on the layout
            TestCaseStatus status = tcase.GetStatus(layout);
            
            // Skip pages that pass the test
            if (status == TestCaseStatus.Pass_Good ||
                status == TestCaseStatus.Pass_Acceptable) 
            { 
                return; 
            }

            DW<Bitmap> page = DW.Wrap(_bookContent.o.BookProvider.o.RenderPageImage(tcase.PageNum, layout.PageSize));
            DW<Bitmap> newPage = layout.Debug_DrawLayout(page);

            TestCaseStatus newStatus = _form.Show(status, tcase, newPage);

            page.DisposeItem();
            newPage.DisposeItem();

            if (newStatus == TestCaseStatus.HaltTest)
            {
                _haltTests = true;
                Assert.Ignore("Halt tests");                
            }

            // Update test case object
            tcase.Comment = _form.Comment;

            if (newStatus == TestCaseStatus.Pass_Acceptable ||
                newStatus == TestCaseStatus.Pass_Good)
            {
                tcase.ExpectedLayout = layout;
                tcase.ExpectedLayoutAccurate = (newStatus == TestCaseStatus.Pass_Good);
            }

            if (newStatus == TestCaseStatus.Ignore_Clear)
            {
                tcase.ExpectedLayout = null;
            }

            switch (newStatus)
            {
                case TestCaseStatus.Fail:
                    Assert.Fail("Failed: " + tcase.Comment);
                    break;
                case TestCaseStatus.Ignore:
                case TestCaseStatus.Ignore_Clear:
                    Assert.Ignore("Ignore: " + tcase.Comment);
                    break;
                case TestCaseStatus.Unknown:
                    Assert.Inconclusive();
                    break;
            }
        }


        IEnumerable<LayoutTestCase> GetTestCases()
        {
            const int StartCount = 7;
            const int MidCount = 4;
            const int EndCount = 4;

            var cases = new List<LayoutTestCase>();

            foreach (var file in TestConst.GetAllPdfFiles())
            {
                int pageCount;

                Book book = new Book(file);
                DW<IBookContent> bookC = RenderFactory.Default.NewBookContent(book, null);
                pageCount = bookC.o.PageCount;
                bookC.DisposeItem();

                // indexes
                var pages = LinqExtensions.IntRange(1, StartCount)
                    .Concat(LinqExtensions.IntRange((pageCount - MidCount) / 2, MidCount))
                    .Concat(LinqExtensions.IntRange(pageCount - EndCount + 1, EndCount))
                    .Where(x => 1 <= x && x <= pageCount)
                    .Distinct()
                    .OrderBy(x => x);

                foreach (int pageNum in pages)
                {
                    var tcase = LayoutTestCase.Get(file, pageNum);
                    yield return tcase;
                }
            }
        }
    }

    [DataContract]
    public class LayoutTestCase 
    {
        [DataMember]
        public String Filename { get; private set; }
        
        [DataMember]
        public int PageNum { get; private set; }

        [DataMember]
        internal PageLayout ExpectedLayout { get; set; }

        [DataMember(Name = "Accurate")]
        public bool ExpectedLayoutAccurate { get; set; }

        [DataMember]
        public String Comment { get; set; }

        LayoutTestCase(String filename, int pageNum)
        {
            Filename = filename;
            PageNum = pageNum;
        }

        #region load/save
        const string CaseFile = "LayoutTestCaseResults.xml";
        static Dictionary<String, LayoutTestCase> _caseResults;
        static Dictionary<String, LayoutTestCase> CaseResults 
        {
            get 
            {
                if (_caseResults == null)
                {
                    _caseResults = XmlHelper.DeserializeOrDefault(CaseFile, new Dictionary<string, LayoutTestCase>());
                }
                return _caseResults;
            }
        }

        public static void Save()
        {
            XmlHelper.Serialize(CaseResults, CaseFile);
        }
        #endregion

        public static LayoutTestCase Get(String filename, int pageNum)
        {
            String key = filename + "_p" + pageNum;

            LayoutTestCase tcase;
            if (!CaseResults.TryGetValue(key, out tcase))
            {
                tcase = new LayoutTestCase(filename, pageNum);
                CaseResults.Add(key, tcase);
            }

            return tcase;
        }

        public override string ToString()
        {
            return Path.GetFileNameWithoutExtension(Filename) + " p" + PageNum;
        }

        internal TestCaseStatus GetStatus(PageLayout layout)
        {
            if (ExpectedLayout == null) { return TestCaseStatus.Unknown; }

            // Shallow comparison, just content bounds for now
            if (ExpectedLayout.UnitBounds.AlmostEquals(layout.UnitBounds, 0.003f)) { return TestCaseStatus.Pass_Good; }

            return TestCaseStatus.Fail;
        }
    }

    public enum TestCaseStatus
    {
        Unknown,
        Pass_Good,
        Pass_Acceptable,
        Fail,
        Ignore,
        Ignore_Clear,
        
        HaltTest,
    }
}
