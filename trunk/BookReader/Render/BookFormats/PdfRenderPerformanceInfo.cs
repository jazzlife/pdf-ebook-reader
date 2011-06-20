using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace PdfBookReader.Render
{

    /// <summary>
    /// Tracks performance of Fast vs. HighQuality PDF rendering,
    /// and chooses the optimal one.
    /// </summary>
    class PdfRenderPerformanceInfo
    {
        double _highQualityTime;
        int _highQualityCount;

        double _fastTime;
        int _fastCount;

        public int MeasureThreshold;
        public int ImprovementThreshold;

        public PdfRenderPerformanceInfo()
        {
            _highQualityCount = 0;
            _highQualityTime = 0;

            _fastCount = 0;
            _fastTime = 0;

            // Defaults
            MeasureThreshold = 800;
            ImprovementThreshold = 400;
        }


        // Note: we can refactor this to keep track of the average
        public void SaveTime(double time, RenderQuality quality)
        {
            switch (quality)
            {
                case RenderQuality.Fast:
                    _fastTime += time;
                    _fastCount++;
                    break;
                case RenderQuality.HighQuality:
                    _highQualityTime += time;
                    _highQualityCount++;
                    break;
            }
        }
        public double GetTime(RenderQuality quality)
        {
            switch (quality)
            {
                case RenderQuality.Fast:
                    return (_fastCount == 0) ? -1 : _fastTime / _fastCount;
                case RenderQuality.HighQuality:
                    return (_highQualityCount == 0) ? -1 : _highQualityTime / _highQualityCount;
            }
            return 0;
        }

        public RenderQuality QualityToUse
        {
            get
            {
                double fast = GetTime(RenderQuality.Fast);
                double highQuality = GetTime(RenderQuality.HighQuality);

                // Do we need to *measure* fast time?
                // Only measure if high quality time is not good enough.
                if (highQuality > MeasureThreshold)
                {
                    if (_highQualityCount < 3)
                    {
                        Trace.WriteLine("Perf: MEASURE high quality time, #" + (_highQualityCount + 1));
                        return RenderQuality.HighQuality;
                    }
                    else if (_fastCount < 3)
                    {
                        Trace.WriteLine("Perf: MEASURE fast time, #" + (_fastCount + 1));
                        return RenderQuality.Fast;
                    }
                }

                // Use high quality if there are no measuements,
                // or if it is fast below threshold
                if (// No measurements
                    _fastCount == 0 || _highQualityCount == 0 ||
                    // High quality is not good enough (below threshold)
                    highQuality <= MeasureThreshold ||
                    // Fast quality is not sufficiently faster
                    fast > highQuality + ImprovementThreshold)
                {
                    return RenderQuality.HighQuality;
                }

                // Finally use fast if all consitions are satisfied
                Trace.WriteLine("Perf: use FAST time");
                return RenderQuality.Fast;
            }
        }
    }

}
