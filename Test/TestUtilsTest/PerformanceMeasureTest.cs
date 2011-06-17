using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PdfBookReader.Test.TestUtils;
using System.Threading;

namespace PdfBookReader.Test.TestUtilsTest
{
    [TestFixture]
    public class PerformanceMeasureTest
    {
        const int TimeToWork = 10;

        [Test]
        public void a1_RunSingleTest()
        {
            PerformanceRunSet set = new PerformanceRunSet("MySimpleOp", "#");
            using (set.NewRun())
            {
                DoWork(TimeToWork);
            }

            Console.WriteLine(set.GetStats());
        }


        double DoWork(int minTimeToWork)
        {
            Thread.Sleep(minTimeToWork);
            return 0;

            // Can't just sleep as control is handed outside this process
            /*
            double res = 0;
            DateTime end = DateTime.Now + TimeSpan.FromMilliseconds(minTimeToWork);
            while (DateTime.Now < end)
            {
                for (int i = 0; i < 1000; i++)
                {
                    res += Math.Sqrt(i);
                }
            }
            return res;
             */
        }

        [Test]
        public void a1_RunSingleTestDateTime()
        {
            PerformanceRunSet set = new PerformanceRunSet("MySimpleOp", "#", TimingMethod.DateTime_Now);
            using (set.NewRun())
            {
                DoWork(TimeToWork);
            }

            Console.WriteLine(set.GetStats());
        }

        [Test]
        public void a2_RunRepeatedTest()
        {
            PerformanceRunSet set = new PerformanceRunSet("MySimpleOp", "#");

            for (int i = 0; i < 10; i++)
            {
                using (set.NewRun())
                {
                    DoWork(TimeToWork);
                }
            }

            Console.WriteLine(set.GetStats());
        }

        [Test]
        public void a2_RunRepeatedTestDateTime()
        {
            PerformanceRunSet set = new PerformanceRunSet("MySimpleOp", "#", TimingMethod.DateTime_Now);

            for (int i = 0; i < 10; i++)
            {
                using (set.NewRun())
                {
                    DoWork(TimeToWork);
                }
            }

            Console.WriteLine(set.GetStats());
        }

        [Test]
        public void a3_ThreeLevelTest()
        {
            PerformanceRunSet pRoot = new PerformanceRunSet("Library Render", "book");

            Random r = new Random(1);
            for (int i = 0; i < 10; i++)
            {
                var pBook = pRoot.NewSet("book #" + (i + 1), "page");
                
                int numPages = r.Next(3, 20);
                for (int y = 0; y < numPages; y++)
                {
                    using (pBook.NewRun())
                    {
                        DoWork(r.Next(5, 50));
                    }
                }
            }

            Console.WriteLine(pRoot.GetStats());
        }

    }


}
