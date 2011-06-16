using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using PdfBookReader.Utils;
using NLog;

namespace PdfBookReader.Test.TestUtils
{
    public class PTimer : IDisposable
    {
        static readonly Logger Log = LogManager.GetLogger("Simple.Performance");

        public readonly String Name;

        public List<double> RunTimes = new List<double>();
        Process _proc = Process.GetCurrentProcess();

        public PTimer(String nameFormat, params object[] args)
        {
            Name = String.Format(nameFormat, args);
        }

        public int RunCount { get { return RunTimes.Count; } }
        public double TotalTime { get { return RunTimes.Sum(); } }
        public double AverageTime 
        { 
            get 
            { 
                return TotalTime / RunCount; 
            } 
        }
        public double MedianTime 
        { 
            get 
            {
                if (RunTimes.Count == 0) { return 0; }
                return RunTimes.OrderBy(x => x).ElementAt(RunTimes.Count / 2); 
            } 
        }

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

        public void Dispose()
        {
            LogAndOutput();
        }

        public void LogAndOutput()
        {
            string msg = "{2,5} ms avg; {3,5} ms median; {1} runs; {0}".F(Name, RunCount, (int)AverageTime, (int)MedianTime);

            Log.Debug(msg);

            Console.WriteLine(msg);
            Console.WriteLine();
        }

        class PerfTimerRun : IDisposable
        {
            readonly PTimer Owner;
            readonly TimeSpan StartTime;

            static Process MyProcess = Process.GetCurrentProcess();

            public PerfTimerRun(PTimer owner, String name)
            {
                ArgCheck.NotNull(owner);
                
                Owner = owner;
                StartTime = MyProcess.TotalProcessorTime;
            }

            public void Dispose()
            {
                TimeSpan endTime = MyProcess.TotalProcessorTime;
                TimeSpan runTime = endTime - StartTime;

                // Multiple run, track and output progress
                Owner.RunTimes.Add(runTime.TotalMilliseconds);

                Console.Write(".");
                if (Owner.RunCount % 60 == 0) { Console.WriteLine(); }
            }
        }

    }
}
