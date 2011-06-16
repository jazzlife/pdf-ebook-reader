﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfBookReader.Utils;

namespace PdfBookReader.Render
{
    /// <summary>
    /// Analyzes the layout of a physical page.
    /// </summary>
    interface IPageLayoutAnalyzer
    {
        PageLayoutInfo DetectPageLayout(DW<Bitmap> physicalPage);
    }


}
