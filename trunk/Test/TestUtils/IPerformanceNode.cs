using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookReaderTest.TestUtils
{
    public interface IPerformanceNode
    {
        String ParamValue { get; }
        TimeSpan TotalTime { get; }

        void AppendStats(StringBuilder sb, int level = 0);
    }

    public enum TimingMethod
    {
        None,
        Stopwatch,
        DateTime_Now,
        Process_TotalProcessorTime,
    }

}
