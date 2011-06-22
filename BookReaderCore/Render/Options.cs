using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookReader.Render
{
    public class Options
    {
        public static Options Current { get; set; }

        static Options()
        {
            Current = new Options();
        }

        public bool NoCache = false;
        public bool Debug_DrawPageNumbers = false;
        public bool Debug_DrawLayoutBounds = false;
        
    
    }
}
