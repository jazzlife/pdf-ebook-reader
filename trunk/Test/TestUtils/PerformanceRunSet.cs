using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PdfBookReaderTest.TestUtils
{
    /// <summary>
    /// Non-leaf node in performance run tree.
    /// </summary>
    [DataContract(Name="Set", IsReference=true)]
    public class PerformanceRunSet : IPerformanceNode
    {
        [DataMember]
        public string ParamValue { get; private set; }

        [DataMember]
        public string ChildParamType { get; private set; }

        [DataMember(Name="Nodes")]
        List<IPerformanceNode> _nodes;

        readonly TimingMethod Method;

        public PerformanceRunSet(string paramValue, string childParamType,
            TimingMethod timingMethod = TimingMethod.Stopwatch)
        {
            ParamValue = paramValue;
            ChildParamType = childParamType;

            Method = timingMethod;
        }

        #region Create new runs / run sets
        public IList<IPerformanceNode> Nodes 
        { 
            get 
            {
                if (_nodes == null) { _nodes = new List<IPerformanceNode>(); }
                return _nodes.AsReadOnly(); 
            } 
        }

        /// <summary>
        /// Create a new run to measure time within using statement, e.g.:
        /// using(me.NewRun("book A")) { ... }
        /// </summary>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public IDisposable NewRun(String paramValue)
        {
            // Ensure all other items are runs
            foreach (IPerformanceNode r in Nodes)
            {
                TimedRun rr = r as TimedRun;
                if (r == null)
                {
                    throw new InvalidOperationException("Runs list contains run sets. Cannot add a simple run.");
                }
            }

            TimedRun run = new TimedRun(paramValue, Method);
            _nodes.Add(run);

            return run;
        }

        public IDisposable NewRun<T>(T paramValue)
        {
            return NewRun(paramValue.ToString());
        }

        /// Create a new run to measure time within using statement, e.g.:
        /// using(me.NewRun())) { ... }
        public IDisposable NewRun()
        {
            string val = (Nodes.Count + 1).ToString(); // next index
            return NewRun(val);
        }

        public PerformanceRunSet NewSet(String paramValue, String childParamType)
        {
            // Ensure all other items are sets and have the same param type
            foreach(IPerformanceNode r in Nodes)
            {
                PerformanceRunSet rs = r as PerformanceRunSet;
                if (rs == null )
                {
                    throw new InvalidOperationException("Runs list contains simple runs. Cannot add a run set.");
                }
                else if (rs.ChildParamType != childParamType)
                {
                    throw new InvalidOperationException("Runs list constains set with ChildParamType="
                        + rs.ChildParamType + ". Cannot add new set with ChildParamType=" + childParamType);
                }
            }

            PerformanceRunSet runSet = new PerformanceRunSet(paramValue, childParamType, Method);
            _nodes.Add(runSet);

            return runSet;
        }
        #endregion

        #region Root node creation (static)


        #endregion


        #region Stats

        public TimeSpan TotalTime
        {
            get
            {
                double milis = Nodes.Sum(x => x.TotalTime.TotalMilliseconds);
                return TimeSpan.FromMilliseconds(milis);
            }
        }

        /// <summary>
        /// Run count of leaf runs (not immediate children)
        /// </summary>
        public int TotalRunCount
        {
            get
            {
                return GetLeafRuns().Count;
            }
        }

        List<TimedRun> GetLeafRuns()
        {
            List<TimedRun> leafRuns = new List<TimedRun>();

            foreach (IPerformanceNode run in Nodes)
            {
                TimedRun leafRun = run as TimedRun;
                if (leafRun != null)
                {
                    leafRuns.Add(leafRun);
                }
                else
                {
                    var subItemLeafRuns = ((PerformanceRunSet)run).GetLeafRuns();
                    leafRuns.AddRange(subItemLeafRuns);
                }
            }
            return leafRuns;
        }

        public TimeSpan AverageTime
        {
            get 
            {
                double milis = TotalTime.TotalMilliseconds / TotalRunCount;
                return TimeSpan.FromMilliseconds(milis);
            }
        }

        public TimeSpan MedianTime
        {
            get
            {
                var leafRuns = GetLeafRuns();
                if (leafRuns.Count == 0) { return TimeSpan.Zero; }

                TimeSpan median = leafRuns
                    .OrderBy(x=> x.TotalTime.TotalMilliseconds)
                    .ElementAt(leafRuns.Count / 2).TotalTime;
                return median;
            }
        }

        public String GetStats(bool showSubsets = true, bool showLeafRuns = false)
        {
            StringBuilder sb = new StringBuilder();
            AppendStats(sb, 0, showSubsets, showLeafRuns);
            return sb.ToString();
        }

        const String sHeader     = @"Average | Median | Runs | Total   | Name";
        const String sStatsNoRun = @"No runs                         | {0}";
        const String sStats      = @" {0,6} | {1,6} | {2,4} | {3,7} | {4}";
        public const String sStatsLeaf  = @" {0,6}                             | {1}"; 

        public void AppendStats(StringBuilder sb, int level,
            bool showSubsets, bool showLeafRuns)
        {
            string indent = new string('>', level * 2);
            sb.Append(indent);

            if (level == 0) { sb.AppendLine(sHeader); }

            if (Nodes.Count == 0)
            {
                sb.AppendFormat(sStatsNoRun, this);
            }
            else
            {
                sb.AppendFormat(sStats, 
                    Math.Round(AverageTime.TotalMilliseconds), 
                    Math.Round(MedianTime.TotalMilliseconds), 
                    TotalRunCount, 
                    Math.Round(TotalTime.TotalMilliseconds),
                    this);

                if (!showSubsets) { return; }

                foreach (IPerformanceNode run in Nodes)
                {
                    if (showLeafRuns || !(run is TimedRun))
                    {
                        sb.AppendLine();
                        sb.Append(indent);
                        run.AppendStats(sb, level + 1);
                    }
                }
            }
        }

        public void AppendStats(StringBuilder sb, int level = 0)
        {
            AppendStats(sb, level, true, false);
        }

        #endregion

        public override string ToString()
        {
            return "rs(" + ParamValue + ", " + ChildParamType +")";
        }

    }
}
