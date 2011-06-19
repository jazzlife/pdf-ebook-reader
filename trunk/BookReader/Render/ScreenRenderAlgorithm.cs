using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    partial class ScreenBook
    {
        #region Render Down / Up

        /// <summary>
        /// Screen rendering algorithm. 
        /// Several variations, see subclasses
        /// </summary>
        abstract class RenderBase
        {
            readonly protected ScreenBook p;
            readonly protected DW<IPageContentSource> PageSource;

            public RenderBase(ScreenBook sBook, DW<IPageContentSource> pageSource) 
            {
                ArgCheck.NotNull(sBook);
                ArgCheck.NotNull(pageSource);

                p = sBook;
                PageSource = pageSource;
            }

            public DW<Bitmap> Run()
            {
                // Use bottom page from previous screen, or get an appropriate one
                PageContent curPage = GetStartingPage();

                // Final page
                if (curPage == null ||
                    curPage.BottomOnScreen <= 0 ||
                    curPage.TopOnScreen >= p.ScreenSize.Height) { return null; }


                // 24bpp format for compatibility with AForge
                DW<Bitmap> screenBmp = DW.Wrap(new Bitmap(p.ScreenSize.Width, p.ScreenSize.Height, PixelFormat.Format24bppRgb));
                using (Graphics g = Graphics.FromImage(screenBmp.o))
                {
                    p.DrawScreenBefore(g);

                    // Render other pages
                    while (curPage != null)
                    {
                        curPage = DrawCurPageRenderNext(curPage, g);
                    }

                    p.DrawScreenAfter(g);
                }

                Trace.WriteLine("Render done: TopPage = " + p.TopPage);
                Trace.WriteLine("Render done: BottomPage = " + p.BottomPage);
                Trace.WriteLine("");

                // Update position
                p.UpdateBookPosition();

                return screenBmp;
            }

            protected abstract PageContent GetStartingPage();

            /// <summary>
            /// Draw current page.
            /// Set TopPage/BottomPage if appropriate.
            /// Return next page (or return null if none)
            /// </summary>
            /// <param name="curPage"></param>
            /// <param name="g"></param>
            /// <returns></returns>
            PageContent DrawCurPageRenderNext(PageContent curPage, Graphics g)
            {
                Trace.WriteLine("RenderOnePage: curPage = " + curPage);

                p.DrawPhysicalPage(g, curPage);

                // Save new top page
                if (curPage.TopOnScreen <= 0)
                {
                    p.TopPage = curPage;
                }

                // Save new bottom page
                if (curPage.BottomOnScreen >= p.ScreenSize.Height)
                {
                    p.BottomPage = curPage;
                }

                // Final page done
                if (ShouldTerminate(curPage))
                {
                    return null;
                }

                AdvanceCurPage(ref curPage);
                return curPage;
            }

            protected abstract void AdvanceCurPage(ref PageContent curPage);
            protected abstract bool ShouldTerminate(PageContent curPage);
        }

        /// <summary>
        /// Render screen page top-down
        /// </summary>
        class RenderDown : RenderBase
        {
            public RenderDown(ScreenBook parent, DW<IPageContentSource> pageSource) : base(parent, pageSource) { }

            protected override PageContent GetStartingPage()
            {
                PageContent curPage;
                if (p.BottomPage == null)
                {
                    Trace.WriteLine("RenderDown_GetStarting: no BottomPage, getting current page in doc");
                    curPage = p.GetPageContent(p.Book.CurrentPosition.PageNum, PageSource);
                }
                else if (p.BottomPage.TopOnScreen < p.ScreenSize.Height)
                {
                    Trace.WriteLine("RenderDown_GetStarting: using BottomPage");

                    // Bottom page included on screen. Adjust offset for next screen.
                    curPage = p.BottomPage;
                    if (p.BottomPage.BottomOnScreen >= p.ScreenSize.Height)
                    {
                        curPage.TopOnScreen = p.BottomPage.TopOnScreen - p.ScreenSize.Height;
                    }
                }
                else
                {
                    Trace.WriteLine("RenderDown_GetStarting: bottomPage is above screen, getting next page");

                    // Render a new page
                    // NOTE: null if past-the-last page
                    curPage = p.GetPageContent(p.BottomPage.PageNum + 1, PageSource);
                }
                return curPage;
            }

            protected override bool ShouldTerminate(PageContent curPage)
            {
                // Final page (no spill over necessary, may terminate early)
                if (curPage.PageNum == p.BookPageProvider.o.PageCount)
                {
                    p.BottomPage = curPage;
                    return true;
                }

                // Page is spilling over to next
                if (curPage.BottomOnScreen >= p.ScreenSize.Height)
                {
                    return true;
                }

                return false;
            }

            protected override void AdvanceCurPage(ref PageContent curPage)
            {
                int newTop = curPage.BottomOnScreen;
                curPage = p.GetPageContent(curPage.PageNum + 1, PageSource);

                Debug.Assert(curPage != null, "curPage null, should not be, we checked earlier");

                curPage.TopOnScreen = newTop;
            }
        }

        /// <summary>
        /// Re-render current screen page after a screen size change.
        /// </summary>
        class RenderCurrent : RenderDown
        {
            readonly Size OldScreenSize;

            public RenderCurrent(ScreenBook parent, DW<IPageContentSource> pageSource, Size oldScreenSize) 
                : base(parent, pageSource)
            {
                OldScreenSize = oldScreenSize;
            }

            protected override PageContent GetStartingPage()
            {
                PageContent curPage;
                if (p.TopPage == null)
                {
                    Trace.WriteLine("RenderCurrent_GetStarting: no top page, getting current page in doc");
                    curPage = p.GetPageContent(p.Book.CurrentPosition.PageNum, PageSource);
                }
                else 
                {
                    // BUGFIX: render some content, even if bottom is above
                    if (p.TopPage.BottomOnScreen <= 20) { p.TopPage.BottomOnScreen = 20; }

                    // Top page included on screen. 
                    Trace.WriteLine("RenderCurrent_GetStarting: using TopPage");
                    
                    // If width change, we need to re-fetch the physical page,
                    // and scale topOnScreen proportionally
                    if (OldScreenSize.Width != p.ScreenSize.Width)
                    {
                        Trace.WriteLine("RenderCurrent_GetStarting: width changed, re-fetching TopPage, recomputing TopOnScreen");

                        // NOTE: use ContentBounds or Layout.Bounds.Height ?
                        // For now it's the same, but when we mess with ContentBounds it may matter.

                        float topOnScreenRelative = 0;
                        if (p.TopPage.Layout.Bounds.Height > 0)
                        {
                            topOnScreenRelative = (float)p.TopPage.TopOnScreen / p.TopPage.Layout.Bounds.Height;
                        }
                        p.TopPage = p.GetPageContent(p.TopPage.PageNum, PageSource);
                        p.TopPage.TopOnScreen = (int)(topOnScreenRelative * p.TopPage.Layout.Bounds.Height);
                    }

                    curPage = p.TopPage;
                    // TopOnScreen -- retain same relative position as before
                }
                return curPage;
            }
        }

        /// <summary>
        /// Render screen page bottom-up
        /// </summary>
        class RenderUp : RenderBase
        {
            public RenderUp(ScreenBook parent, DW<IPageContentSource> pageSource) : base(parent, pageSource) { }

            protected override PageContent GetStartingPage()
            {
                PageContent curPage;
                if (p.TopPage == null)
                {
                    Trace.WriteLine("RenderUp.GetStarting: no TopPage, getting last page in doc (and setting bounds)");
                    curPage = p.GetPageContent(p.BookPageProvider.o.PageCount, PageSource);
                    curPage.BottomOnScreen = p.ScreenSize.Height;
                }
                else if (p.TopPage.TopOnScreen < 0)
                {
                    Trace.WriteLine("RenderUp_GetStarting: using TopPage");

                    // Bottom page included on screen. Adjust offset for next screen.
                    curPage = p.TopPage;
                    curPage.TopOnScreen = p.ScreenSize.Height + p.TopPage.TopOnScreen; 
                }
                else if (p.TopPage.TopOnScreen == 0)
                {
                    Trace.WriteLine("RenderUp_GetStarting: TopPage below screen, getting previous page");

                    // Render a new page
                    // NOTE: null if past-the-last page
                    curPage = p.GetPageContent(p.TopPage.PageNum - 1, PageSource);
                    curPage.BottomOnScreen = p.ScreenSize.Height;
                }
                else
                {
                    return null;
                }
                return curPage;
            }

            protected override bool ShouldTerminate(PageContent curPage)
            {
                // Final page (no spill over necessary, may terminate early)
                if (curPage.PageNum == 1)
                {
                    p.TopPage = curPage;
                    return true;
                }

                // Page is spilling over to next
                if (curPage.TopOnScreen <= 0)
                {
                    return true;
                }

                return false;
            }

            protected override void AdvanceCurPage(ref PageContent curPage)
            {
                int newBottom = curPage.TopOnScreen;
                curPage = p.GetPageContent(curPage.PageNum - 1, PageSource);

                Debug.Assert(curPage != null, "curPage null, should not be, we checked earlier");

                curPage.BottomOnScreen = newBottom;
            }
        }
        #endregion

    }
}
