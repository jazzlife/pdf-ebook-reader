using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDFViewer.Test.TestUtils
{
    public class PerfTimer : IDisposable
    {
        readonly DateTime StartTime;
        readonly String Name;
        readonly int NumOps;

        public PerfTimer(String name, int numOps = 1)
        {
            if (numOps < 1) { throw new ArgumentException("numOps < 1"); }

            StartTime = DateTime.Now;
            Name = name;
            NumOps = numOps;
        }

        public void Dispose()
        {
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime.Subtract(StartTime);

            Console.WriteLine(Name + " " + (duration.TotalMilliseconds / NumOps) + " ms");
        }
    }
}
