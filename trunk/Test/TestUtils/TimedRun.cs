using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace PdfBookReader.Test.TestUtils
{
    /// <summary>
    /// Leaf node in the peformance run tree.
    /// </summary>
    [DataContract(Name = "Run")]
    public class TimedRun : IPerformanceNode, IDisposable
    {
        [DataMember]
        public String ParamValue { get; private set; }

        [DataMember]
        virtual public TimeSpan TotalTime { get; private set; }

        #region timing fields

        readonly TimingMethod Method;

        // DateTime.Now
        DateTime _startTime;
        DateTime _endTime;

        // Process.TotalProcessorTime 
        Process _myProcess;
        TimeSpan _startTimeProc;
        TimeSpan _endTimeProc;

        // Stopwatch
        Stopwatch _stopwatch;

        #endregion

        public TimedRun(String paramValue,
            TimingMethod timingMethod = TimingMethod.Stopwatch)
        {
            ParamValue = paramValue;

            Method = timingMethod;
            Start();
        }

        void Start()
        {
            switch (Method)
            {
                case TimingMethod.DateTime_Now:
                    _startTime = DateTime.Now;
                    _endTime = DateTime.MinValue;
                    break;
                case TimingMethod.Process_TotalProcessorTime:
                    _myProcess = Process.GetCurrentProcess();
                    _startTimeProc = _myProcess.TotalProcessorTime;
                    _endTimeProc = TimeSpan.MinValue;
                    break;
                case TimingMethod.Stopwatch:
                    _stopwatch = Stopwatch.StartNew();
                    break;
                default:
                    throw new InvalidOperationException("v= " + Method);
            }
        }

        void End()
        {
            switch (Method)
            {
                case TimingMethod.DateTime_Now:
                    _endTime = DateTime.Now;
                    break;
                case TimingMethod.Process_TotalProcessorTime:
                    _endTimeProc = _myProcess.TotalProcessorTime;
                    break;
                case TimingMethod.Stopwatch:
                    _stopwatch.Stop();
                    break;
                default:
                    throw new InvalidOperationException("v= " + Method);
            }

            TotalTime = GetTotalTime();
        }

        TimeSpan GetTotalTime()
        {
            switch (Method)
            {
                case TimingMethod.DateTime_Now:
                    return _endTime - _startTime;
                case TimingMethod.Process_TotalProcessorTime:
                    return _endTimeProc - _startTimeProc;
                case TimingMethod.Stopwatch:
                    return TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds);
                default:
                    throw new InvalidOperationException("v= " + Method);
            }
        }

        virtual public void Dispose()
        {
            End();
        }

        public override string ToString()
        {
            return "r(" + ParamValue + ")";
        }

        public void AppendStats(StringBuilder sb, int level = 0)
        {
            string indent = new string('>', level * 2);
            sb.Append(indent);

            sb.AppendFormat(PerformanceRunSet.sStatsLeaf, 
                Math.Round(TotalTime.TotalMilliseconds), 
                this);
        }
    }
}
