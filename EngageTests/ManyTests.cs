using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using EAX;
using EaxOpenClose;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngageTests
{
    [TestClass]
    public class ManyTests
    {
        private static readonly Stopwatch Timer = new Stopwatch();

        [TestMethod]
        [TestCategory("EAX")]
        public void TestXmlShallow()
        {
            const int maxLimit = 1000; // the higher, the longer test time and the higher chances to crash
            const int maxReruns = 20; // the higher, the smoother

            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % 10 == 0).ToList();
            var length = limits.Count;
            // prepare to collect data
            var ticksSax = new List<List<long>>();
            var ticksEax = new List<List<long>>();
            for (var i = 0; i < length; i++)
            {
                ticksSax.Add(new List<long>());
                ticksEax.Add(new List<long>());
            }

            // run several times 
            for (var i = 0; i < maxReruns; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    string input = GenerateShallowTestInput(limits[j]);
                    ticksSax[j].Add(TimeCountSaxShallow(input));
                    ticksEax[j].Add(TimeCountEaxShallow(input));
                }
            }

            // print only the median from each measurement
            PrintResults(limits, ticksSax.Select(Median), ticksEax.Select(Median));
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

        private static void PrintResults(IEnumerable<int> limits, IEnumerable<long> ticks1, IEnumerable<long> ticks2)
        {
            Console.WriteLine($"Limits: [{CommaSep(limits)}]");
            Console.WriteLine($"Times1: [{CommaSep(ticks1)}]");
            Console.WriteLine($"Times2: [{CommaSep(ticks2)}]");
        }

        private static string CommaSep<T>(IEnumerable<T> list)
            => String.Join(", ", list.Select(e => e.ToString()));

        private long TimeCountSaxShallow(string input)
        {
            Timer.Restart();

            HashSet<string> tags = new HashSet<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(input)))
            {
                reader.MoveToContent();
                while (reader.Read())
                    tags.Add(reader.Name);
            }

            Timer.Stop();

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallow(string input)
        {
            Timer.Restart();

            HashSet<string> tags = new HashSet<string>();

            var result = Parsers.ParseOpenClose(input);

            foreach (var tag in result.tags)
            {
                if (tag is TagOpen oTag)
                    tags.Add(oTag.n.value);
            }

            Timer.Stop();

            return Timer.ElapsedTicks;
        }

        private static string GenerateShallowTestInput(int limit)
            => Generator.ArbitraryBalancedSequenceShallow(limit);
    }
}