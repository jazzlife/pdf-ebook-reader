using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;
using PdfBookReader.Model;
using System.Drawing.Imaging;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Assemble screen in various ways:
    /// (1) current screen (e.g. intial load or size changed)
    /// (2) next screen (downwards)
    /// (3) previous screen (upwards)
    /// </summary>
    abstract class AssembleScreenAlgorithm
    {
        protected readonly DW<IPageSource> PageSource;
        protected readonly DW<IBookProvider> BookProvider;

        public AssembleScreenAlgorithm(DW<IPageSource> source, DW<IBookProvider> bookPageProvider)
        {
            PageSource = source;
            BookProvider = bookPageProvider;
        }

        public List<Page> AssembleScreen(ref PositionInBook position, Size screenSize) 
        {
            ArgCheck.NotNull(position, "newPosition");

            if (!CanApply(position, screenSize)) { throw new InvalidOperationException("Cannot apply"); }

            // Find physical page at the new position
            Page curPage = GetInitialPage(ref position, screenSize);

            List<Page> pageContents = new List<Page>();
            while (true)
            {
                pageContents.Add(curPage);

                if (ScreenFull(curPage, screenSize)) { break; }
                AdvancePage(ref curPage, screenSize);
            }
            return pageContents;
        }

        // True if it's possible to run the algorithm, false othewise
        public abstract bool CanApply(PositionInBook position, Size screenSize);

        // Get the initial page in the list.
        protected abstract Page GetInitialPage(ref PositionInBook position, Size screenSize);

        // True if screen is full, should terminate the loop.
        protected abstract bool ScreenFull(Page curPage, Size screenSize);

        // Advance page by one
        protected abstract void AdvancePage(ref Page curPage, Size sceenSize);

        // Helper
        protected int PageCount { get { return BookProvider.o.PageCount; } }
    }

    /// <summary>
    /// Assemble current screen, starting at *position*
    /// </summary>
    class AssembleCurrentScreenAlgorithm : AssembleScreenAlgorithm
    {
        public AssembleCurrentScreenAlgorithm(DW<IPageSource> s, DW<IBookProvider> p) : base(s, p) { }

        public override bool CanApply(PositionInBook position, Size screenSize)
        {
            // Re-rendering current screen always possible
            // Position bounds are guarented already
            return true;
        }

        protected override Page GetInitialPage(ref PositionInBook position, Size screenSize)
        {
            // Find physical page at the new position
            Page curPage = PageSource.o.GetPage(position.PageNum, screenSize, BookProvider);
            curPage.TopOnScreen = position.GetTopOnScreen(curPage.Layout.Bounds.Height);
            return curPage;
        }

        protected override void AdvancePage(ref Page curPage, Size sceenSize)
        {
            int bottom = curPage.BottomOnScreen;
            curPage = PageSource.o.GetPage(curPage.PageNum + 1, sceenSize, BookProvider);
            curPage.TopOnScreen = bottom;
        }

        protected override bool ScreenFull(Page curPage, Size screenSize)
        {
            // Spilling over to next page
            if (curPage.BottomOnScreen >= screenSize.Height) { return true; }

            // Final page (no spill over necessary)
            if (curPage.PageNum == PageCount) { return true; }

            return false;
        }
    }

    /// <summary>
    /// Assemble next screen, after the current.
    /// </summary>
    class AssembleNextScreenAlgorithm : AssembleCurrentScreenAlgorithm
    {
        public AssembleNextScreenAlgorithm(DW<IPageSource> s, DW<IBookProvider> p) : base(s, p) { }

        public override bool CanApply(PositionInBook position, Size screenSize)
        {
            // TODO

            // Re-rendering current screen always possible
            // Position bounds are guarented already
            return true;
        }


        protected override Page GetInitialPage(ref PositionInBook position, Size screenSize)
        {
            Page curPage = base.GetInitialPage(ref position, screenSize);

            // Skip one screen -- move past without adding items to the list
            while (true)
            {
                if (ScreenFull(curPage, screenSize)) { break; }
                AdvancePage(ref curPage, screenSize);
            }

            // Move the last page up one screen
            curPage.TopOnScreen -= screenSize.Height;

            // Adjust position
            position = PositionInBook.FromPhysicalPage(curPage.PageNum, PageCount,
                curPage.TopOnScreen, curPage.Layout.Bounds.Height);

            return curPage;
        }
    }

    /// <summary>
    /// Assemble previous screen, before the current
    /// </summary>
    class AssemblePreviousScreenAlgorithm : AssembleScreenAlgorithm
    {
        public AssemblePreviousScreenAlgorithm(DW<IPageSource> s, DW<IBookProvider> p) : base(s, p) { }

        public override bool CanApply(PositionInBook position, Size screenSize)
        {
            throw new NotImplementedException();
        }

        protected override Page GetInitialPage(ref PositionInBook position, Size screenSize)
        {
            throw new NotImplementedException();
        }

        protected override bool ScreenFull(Page curPage, Size screenSize)
        {
            throw new NotImplementedException();
        }

        protected override void AdvancePage(ref Page curPage, Size sceenSize)
        {
            throw new NotImplementedException();
        }
    }

}
