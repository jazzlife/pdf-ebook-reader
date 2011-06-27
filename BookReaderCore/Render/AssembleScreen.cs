using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BookReader.Utils;
using BookReader.Model;
using System.Drawing.Imaging;
using BookReader.Render.Layout;

namespace BookReader.Render
{
    /// <summary>
    /// Assemble screen in various ways:
    /// (1) current screen (e.g. intial load or size changed)
    /// (2) next screen (downwards)
    /// (3) previous screen (upwards)
    /// </summary>
    abstract class AssembleScreenAlgorithm
    {
        // TODO: determine based on page analysis
        public static int RowSpacing = 4;
        protected readonly ScreenBook ScreenBook;

        public AssembleScreenAlgorithm(ScreenBook screenBook)
        {
            ScreenBook = screenBook;
        }

        /// <summary>
        /// True if algorithm can be applied in the given state.
        /// (e.g. if moving back, it would apply to all positions except the start).
        /// </summary>
        /// <param name="position"></param>
        /// <param name="screenSize"></param>
        /// <returns></returns>
        public abstract bool CanApply(PositionInBook position, Size screenSize);

        /// <summary>
        /// Assemble screen -- return a list of pages for the screen with TopOnScreen set
        /// </summary>
        /// <param name="position"></param>
        /// <param name="screenSize"></param>
        /// <returns></returns>
        public List<PageOnScreen> AssembleScreen(ref PositionInBook position, Size screenSize) 
        {
            ArgCheck.NotNull(position, "newPosition");
            if (!CanApply(position, screenSize)) { throw new InvalidOperationException("Cannot apply at: " + position); }

            // Get the initial page. 
            // Does not necessarily return page at position -- in case of next, will advance it by one page.
            PageOnScreen curPage = GetInitialPage(ref position, screenSize);

            // Stack the pages
            List<PageOnScreen> pageContents = new List<PageOnScreen>();
            while (true)
            {
                pageContents.Add(curPage);

                if (ScreenFull(curPage, screenSize)) { break; }

                AdvancePage(ref curPage, screenSize);
            }

            // Update position based on the topmost page (with lowest page num)
            int minPageNum = pageContents.Min(x => x.PageNum);
            PageOnScreen topPage = pageContents.Find(x => x.PageNum == minPageNum);
            position = PositionInBook.FromPhysicalPage(topPage.PageNum, PageCount,
                topPage.TopOnScreen, topPage.Layout.Bounds.Height);

            return pageContents;
        }

        protected virtual PageOnScreen GetInitialPage(ref PositionInBook position, Size screenSize)
        {
            // Find physical page at position
            PageOnScreen curPage = NewPage(position.PageNum, screenSize);
            curPage.TopOnScreen = position.GetTopOnScreen(curPage.Layout.Bounds.Height);
            return curPage;
        }

        // True if screen is full, should terminate the loop.
        internal abstract bool ScreenFull(PageOnScreen curPage, Size screenSize);

        // Advance page by one
        internal abstract void AdvancePage(ref PageOnScreen curPage, Size sceenSize);

        // Helper
        protected int PageCount { get { return ScreenBook.Book.CurrentPosition.PageCount; } }

        protected PageOnScreen NewPage(int pageNum, Size screenSize)
        {
            PageLayout layout = ScreenBook.BookContent.o.GetPageLayout(pageNum);
            layout = layout.ScaleToScreen(screenSize);
            return new PageOnScreen(pageNum, layout);
        }
    }

    /// <summary>
    /// Assemble current screen, starting at *position*
    /// </summary>
    sealed class AssembleCurrentScreenAlgorithm : AssembleScreenAlgorithm
    {
        public AssembleCurrentScreenAlgorithm(ScreenBook p) 
            : base(p) { }

        public override bool CanApply(PositionInBook position, Size screenSize)
        {
            // Re-rendering current screen always possible
            // Position bounds are guarented already
            return true;
        }

        internal override void AdvancePage(ref PageOnScreen curPage, Size screenSize)
        {
            int bottom = curPage.BottomOnScreen;
            curPage = NewPage(curPage.PageNum + 1, screenSize);
            curPage.TopOnScreen = bottom + RowSpacing;
        }

        internal override bool ScreenFull(PageOnScreen curPage, Size screenSize)
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
    sealed class AssembleNextScreenAlgorithm : AssembleScreenAlgorithm
    {
        AssembleCurrentScreenAlgorithm _assembleCurrent;

        public AssembleNextScreenAlgorithm(ScreenBook p)
            : base(p) 
        {
            _assembleCurrent = new AssembleCurrentScreenAlgorithm(p);
        }

        public override bool CanApply(PositionInBook position, Size screenSize)
        {
            // Get last page of current screen
            // Current screen render won't mess up position
            var pages = _assembleCurrent.AssembleScreen(ref position, screenSize);

            PageOnScreen lastPage = pages.Last();
            return lastPage.PageNum < PageCount ||
                // If last page in book, it must overflow the screen
                lastPage.BottomOnScreen > screenSize.Height;
        }

        protected override PageOnScreen GetInitialPage(ref PositionInBook position, Size screenSize)
        {
            // Assemble current screen
            PageOnScreen curPage = _assembleCurrent.AssembleScreen(ref position, screenSize).Last();

            // Move the last page of current screen up by one screen
            curPage.TopOnScreen -= screenSize.Height;

            // Edge case: if page is exactly above the screen, take the next one
            if (curPage.TopOnScreen == -curPage.Layout.Bounds.Height)
            {
                AdvancePage(ref curPage, screenSize);
            }

            return curPage;
        }

        internal override bool ScreenFull(PageOnScreen curPage, Size screenSize)
        {
            return _assembleCurrent.ScreenFull(curPage, screenSize);
        }

        internal override void AdvancePage(ref PageOnScreen curPage, Size sceenSize)
        {
            _assembleCurrent.AdvancePage(ref curPage, sceenSize);
        }
    }

    /// <summary>
    /// Assemble previous screen, before the current
    /// </summary>
    sealed class AssemblePreviousScreenAlgorithm : AssembleScreenAlgorithm
    {
        public AssemblePreviousScreenAlgorithm(ScreenBook p) 
            : base(p) { }

        public override bool CanApply(PositionInBook position, Size screenSize)
        {
            PageOnScreen firstPage = base.GetInitialPage(ref position, screenSize);
            
            return firstPage.PageNum > 1 ||
                // If first page in book, it must overflow above current screen
                firstPage.TopOnScreen < 0;
        }

        protected override PageOnScreen GetInitialPage(ref PositionInBook position, Size screenSize)
        {
            PageOnScreen curPage = base.GetInitialPage(ref position, screenSize);
            curPage.TopOnScreen += screenSize.Height;

            // Edge case: if page is exactly below the screen, take the next one
            if (curPage.TopOnScreen == screenSize.Height)
            {
                AdvancePage(ref curPage, screenSize);
            }

            // Adjust position
            position = PositionInBook.FromPhysicalPage(curPage.PageNum, PageCount,
                curPage.TopOnScreen, curPage.Layout.Bounds.Height);

            return curPage;
        }

        internal override bool ScreenFull(PageOnScreen curPage, Size screenSize)
        {
            // Spilling over to next page
            if (curPage.TopOnScreen <= 0) { return true; }

            // First page (no spill upwards necessary)
            if (curPage.PageNum == 1) { return true; }

            return false;
        }

        internal override void AdvancePage(ref PageOnScreen curPage, Size screenSize)
        {
            int top = curPage.TopOnScreen;
            curPage = NewPage(curPage.PageNum - 1, screenSize);
            curPage.BottomOnScreen = top - RowSpacing;
        }
    }
}
