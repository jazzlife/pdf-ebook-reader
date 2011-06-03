using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PdfBookReader.Test.TestUtils
{
    public class PerfTimer 
    {
        public readonly String Name;

        TimeSpan _totalTime = TimeSpan.Zero;
        int _count = 0;
        Process _proc = Process.GetCurrentProcess();

        public PerfTimer(String nameFormat, params object[] args)
        {
            Name = String.Format(nameFormat, args);
        }


        public int RunCount { get { return _count; } }
        public TimeSpan TotalTime { get { return _totalTime; } }

        /// <summary>
        /// Average time of a run in milisecons
        /// </summary>
        public int AverageTimeMilis { get { return (int)(_totalTime.TotalMilliseconds / _count); } }

        /// <summary>
        /// Create a new run withn a using statement, like:
        /// using(myPerfTimer.NewRun) {
        ///   // ... operation you need to time ...
        /// }
        /// </summary>
        public IDisposable NewRun
        {
            get { return new PerfTimerRun(this, Name); }
        }

        /// <summary>
        /// Create a single run of the timer in using statement, like:
        /// using(PerfTimer.SingleRun("Load File")
        /// {
        /// 
        /// }
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDisposable SingleRun(String nameFormat, params object[] args)
        {
            return new PerfTimerRun(null, String.Format(nameFormat, args));
        }

        class PerfTimerRun : IDisposable
        {
            readonly String Name;
            readonly PerfTimer Owner;
            readonly TimeSpan StartTime;

            static Process MyProcess = Process.GetCurrentProcess();

            public PerfTimerRun(PerfTimer owner, String name)
            {
                Name = name;
                Owner = owner;
                StartTime = MyProcess.TotalProcessorTime;
            }

            bool IsSingleRun { get { return Owner == null; } }

            public void Dispose()
            {
                TimeSpan endTime = MyProcess.TotalProcessorTime;
                TimeSpan runTime = endTime - StartTime;

                if (IsSingleRun)
                {
                    // Single run, output end time
                    Console.WriteLine(String.Format("{0,5}ms   {1}", (int)runTime.TotalMilliseconds, Name));
                }
                else
                {
                    // Multiple run, track and output progress
                    Owner._totalTime += runTime;
                    Owner._count++;

                    Console.Write(".");
                    if (Owner._count % 60 == 0) { Console.WriteLine(); }
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0,5}ms/run   {1} ({2} runs)", AverageTimeMilis, Name, RunCount);
        }

    }
}
