using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDFViewer.Reader.Utils
{
    static class ExtensionMethods
    {
        public static bool EqualsIC(this String a, String b)
        {
            if (a == null) { return b == null; }
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
