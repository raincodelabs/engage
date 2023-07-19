using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngageTests
{
    [TestClass]
    public class ManyTests
    {
        private static Stopwatch _timer = new Stopwatch();

        [TestMethod]
        [TestCategory("EAX")]
        public void TestEaxShallow()
        {
            const int maxLimit = 2000; // the higher, the longer test time and the higher chances to crash
            const int maxReruns = 20; // the higher, the smoother
            
            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % 10 == 0).ToList();
            var length = limits.Count;
            // prepare to collect data
            var ticks = new List<List<long>>();
            for (var i = 0; i < length; i++)
                ticks.Add(new List<long>());

            // run several times 
            for (var i = 0; i < maxReruns; i++)
            {
                for (var j = 0; j < length; j++)
                    ticks[j].Add(TimeCountSaxShallow(limits[j]));
            }

            // print only the median from each measurement
            PrintResults(limits, ticks.Select(Median));
        }

        private static long Median(IEnumerable<long> list)
        {
            long[] array = list.ToArray();
            Array.Sort(array);
            var size = array.Length;
            var middle = size / 2;
            if (size % 2 == 0)
                return (long)((array[middle] + (double)array[middle - 1]) / 2);
            return array[middle];
        }

        private static void PrintResults(IEnumerable<int> limits, IEnumerable<long> ticks)
        {
            Console.WriteLine($"Limits: [{String.Join(", ", limits.Select(limit => limit.ToString()))}]");
            Console.WriteLine($"Times:  [{String.Join(", ", ticks.Select(tick => (tick / 10).ToString()))}]");
        }

        private long TimeCountSaxShallow(int limit)
        {
            var input = Generator.ArbitraryBalancedSequenceShallow(limit);
            // Console.WriteLine(input);
            var timer = new Stopwatch();
            timer.Start();


            HashSet<string> tags = new HashSet<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(input)))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    tags.Add(reader.Name);
                }
            }

            timer.Stop();

            return timer.ElapsedTicks;
        }
    }
}