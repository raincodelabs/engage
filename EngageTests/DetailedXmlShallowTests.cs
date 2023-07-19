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
    public class DetailedXmlShallowTests
    {
        static string _input100 = GenerateShallowTestInput(100);
        private static readonly Stopwatch Timer = new Stopwatch();
        private int _dump;

        [TestMethod]
        [TestCategory("EAX")]
        public void TestForProfiling()
        {
            var p = new EaxOpenClose.ParserOptimisedForStrings(_input100);
            var r = p.Parse();
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void TestPerformanceOne()
        {
            const int maxLimit = 100; // the higher, the longer test time and the higher chances to crash
            const int stepLimit = 1; // the lower, the denser the coverage of all limits
            const int maxReruns = 10; // the higher, the smoother

            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % stepLimit == 0).ToList();
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
            PrintResultsReadable(limits,
                ticksSax.Select(Median),
                new List<IEnumerable<long>> { ticksEax.Select(Median) });
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void TestPerformanceAll()
        {
            const int maxLimit = 1000; // the higher, the longer test time and the higher chances to crash
            const int stepLimit = 10; // the lower, the denser the coverage of all limits
            const int maxReruns = 30; // the higher, the smoother
            var knownParsers = new List<Func<string, long>>
            {
                TimeCountEaxShallow,
                TimeCountEaxShallowO1,
                TimeCountEaxShallowO2,
                TimeCountEaxShallowO3,
                TimeCountEaxShallowO4,
                TimeCountEaxShallowO5,
                TimeCountEaxShallowO6,
            };

            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % stepLimit == 0).ToList();
            var length = limits.Count;
            // prepare to collect data
            var ticksSax = new List<List<long>>();
            var ticksEax = new List<List<List<long>>>();
            for (var i = 0; i < knownParsers.Count; i++)
                ticksEax.Add(new List<List<long>>());

            for (var i = 0; i < length; i++)
            {
                ticksSax.Add(new List<long>());
                for (var j = 0; j < knownParsers.Count; j++)
                    ticksEax[j].Add(new List<long>());
            }

            // run several times 
            for (var i = 0; i < maxReruns; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    string input = GenerateShallowTestInput(limits[j]);
                    ticksSax[j].Add(TimeCountSaxShallow(input));
                    for (var k = 0; k < knownParsers.Count; k++)
                        ticksEax[k][j].Add(knownParsers[k](input));
                }
            }

            // print only the median from each measurement
            // PrintResultsReadable(limits,
            //     ticksSax.Select(Median),
            //     ticksEax.Select(ticks => ticks.Select(Median)));

            PrintResultsCSV(limits,
                ticksSax.Select(Median).ToList(),
                ticksEax.Select(ticks => ticks.Select(Median).ToList()));
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void TestCorrectnessBasic()
        {
            const int maxLimit = 1000; // the higher, the longer test time and the higher chances to crash
            const int maxReruns = 50; // the higher, the more thorough

            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % 10 == 0).ToList();
            var length = limits.Count;

            // run several times 
            for (var i = 0; i < maxReruns; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    string input = GenerateShallowTestInput(limits[j]);
                    if (!CompareSaxEaxBasic(input))
                        Console.WriteLine($"DIFF for {input}");
                    Assert.IsTrue(CompareSaxEaxBasic(input));
                }
            }
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void TestCorrectnessOptimised()
        {
            const int maxLimit = 1000; // the higher, the longer test time and the higher chances to crash
            const int maxReruns = 50; // the higher, the more thorough

            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % 10 == 0).ToList();
            var length = limits.Count;

            // run several times 
            for (var i = 0; i < maxReruns; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    string input = GenerateShallowTestInput(limits[j]);
                    if (!CompareSaxEaxOptimised(input))
                        Console.WriteLine($"DIFF for {input}");
                    Assert.IsTrue(CompareSaxEaxOptimised(input));
                }
            }
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void TestCorrectnessCollapsed()
        {
            const int maxLimit = 1000; // the higher, the longer test time and the higher chances to crash
            const int maxReruns = 50; // the higher, the more thorough

            // all the limits/depths/… we will use
            var limits = Enumerable.Range(1, maxLimit).Where(n => n % 10 == 0).ToList();
            var length = limits.Count;

            // run several times 
            for (var i = 0; i < maxReruns; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    string input = GenerateShallowTestInput(limits[j]);
                    if (!CompareSaxEaxCollapsed(input))
                        Console.WriteLine($"DIFF for {input}");
                    Assert.IsTrue(CompareSaxEaxCollapsed(input));
                }
            }
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

        private static void PrintResultsReadable(IEnumerable<int> limits, IEnumerable<long> ticksSax,
            IEnumerable<IEnumerable<long>> ticksEax)
        {
            Console.WriteLine($"Limits:     [{CommaSep(limits)}]");
            Console.WriteLine($"SAX times:  [{CommaSep(ticksSax)}]");
            var cx = 1;
            foreach (var ticksEaxEntry in ticksEax)
                Console.WriteLine($"EAX times{cx}: [{CommaSep(ticksEaxEntry)}]");
        }

        private static void PrintResultsCSV(List<int> limits, List<long> ticksSax,
            IEnumerable<List<long>> ticksEax)
        {
            for (int i = 0; i < limits.Count(); i++)
                Console.WriteLine($"{limits[i]};{ticksSax[i]};{SemiSep(ticksEax, i)}");
        }

        private static string CommaSep<T>(IEnumerable<T> list)
            => String.Join(", ", list.Select(e => e.ToString()));

        private static string SemiSep<T>(IEnumerable<List<T>> lists, int index)
            => String.Join(";", lists.Select(list => list[index].ToString()));

        private long TimeCountSaxShallow(string input)
        {
            HashSet<string> tags = new HashSet<string>();

            Timer.Restart();

            using (XmlReader reader = XmlReader.Create(new StringReader(input)))
            {
                reader.MoveToContent();
                while (reader.Read())
                    tags.Add(reader.Name);
            }

            Timer.Stop();
            // Console.WriteLine($"Tags found: {tags.Count}");
            _dump = tags.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallow(string input)
        {
            HashSet<string> tags = new HashSet<string>();

            Timer.Restart();

            var result = Parsers.ParseOpenClose(input);

            foreach (var tag in result.tags)
            {
                if (tag is TagOpen oTag)
                    tags.Add(oTag.n.value);
            }

            Timer.Stop();
            // Console.WriteLine($"Tags found: {tags.Count}");
            _dump = tags.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallowO1(string input)
        {
            HashSet<string> tags = new HashSet<string>();

            Timer.Restart();

            var result = Parsers.ParseOpenCloseO1(input);

            foreach (var tag in result.tags)
            {
                if (tag is TagOpen oTag)
                    tags.Add(oTag.n.value);
            }

            Timer.Stop();
            // Console.WriteLine($"Tags found: {tags.Count}");
            _dump = tags.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallowO2(string input)
        {
            HashSet<string> tags = new HashSet<string>();

            Timer.Restart();

            var result = Parsers.ParseOpenCloseO2(input);

            foreach (var tag in result.tags)
            {
                if (tag is TagOpen oTag)
                    tags.Add(oTag.n.value);
            }

            Timer.Stop();
            // Console.WriteLine($"Tags found: {tags.Count}");
            _dump = tags.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallowO3(string input)
        {
            HashSet<string> tags = new HashSet<string>();

            Timer.Restart();

            var result = Parsers.ParseOpenCloseO3(input);

            foreach (var tag in result.tags)
            {
                if (tag is TagOpen2 oTag)
                    tags.Add(oTag.n);
            }

            Timer.Stop();
            // Console.WriteLine($"Tags found: {tags.Count}");
            _dump = tags.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallowO4(string input)
        {
            HashSet<string> tags = new HashSet<string>();

            Timer.Restart();

            var result = Parsers.ParseOpenCloseO4(input);

            foreach (var tag in result.tags)
            {
                if (tag is TagOpen oTag)
                    tags.Add(oTag.n.value);
            }

            Timer.Stop();
            // Console.WriteLine($"Tags found: {tags.Count}");
            _dump = tags.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallowO5(string input)
        {
            Timer.Restart();
            var result = Parsers.ParseOpenCloseO5(input);
            Timer.Stop();

            _dump = result.Count;

            return Timer.ElapsedTicks;
        }

        private long TimeCountEaxShallowO6(string input)
        {
            Timer.Restart();
            var result = Parsers.ParseOpenCloseO6(input);
            Timer.Stop();

            _dump = result.Count;

            return Timer.ElapsedTicks;
        }

        private bool CompareSaxEaxBasic(string input)
        {
            HashSet<string> tagsSax = new HashSet<string>();
            HashSet<string> tagsEax = new HashSet<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(input)))
            {
                reader.MoveToContent();
                while (reader.Read())
                    tagsSax.Add(reader.Name);
            }

            foreach (var tag in Parsers.ParseOpenClose(input).tags)
            {
                if (tag is TagOpen oTag)
                    tagsEax.Add(oTag.n.value);
            }

            return tagsSax.SetEquals(tagsEax);
        }

        private bool CompareSaxEaxOptimised(string input)
        {
            HashSet<string> tagsSax = new HashSet<string>();
            HashSet<string> tagsEaxO1 = new HashSet<string>();
            HashSet<string> tagsEaxO2 = new HashSet<string>();
            HashSet<string> tagsEaxO3 = new HashSet<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(input)))
            {
                reader.MoveToContent();
                while (reader.Read())
                    tagsSax.Add(reader.Name);
            }

            foreach (var tag in Parsers.ParseOpenCloseO1(input).tags)
            {
                if (tag is TagOpen oTag)
                    tagsEaxO1.Add(oTag.n.value);
            }

            foreach (var tag in Parsers.ParseOpenCloseO2(input).tags)
            {
                if (tag is TagOpen oTag)
                    tagsEaxO2.Add(oTag.n.value);
            }

            foreach (var tag in Parsers.ParseOpenCloseO3(input).tags)
            {
                if (tag is TagOpen2 oTag)
                    tagsEaxO3.Add(oTag.n);
            }

            return tagsSax.SetEquals(tagsEaxO1) &&
                   tagsSax.SetEquals(tagsEaxO2) &&
                   tagsSax.SetEquals(tagsEaxO3);
        }

        private bool CompareSaxEaxCollapsed(string input)
        {
            HashSet<string> tagsSax = new HashSet<string>();
            HashSet<string> tagsEaxO1 = new HashSet<string>();
            HashSet<string> tagsEaxO2 = new HashSet<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(input)))
            {
                reader.MoveToContent();
                while (reader.Read())
                    tagsSax.Add(reader.Name);
            }

            foreach (var tag in Parsers.ParseOpenCloseO4(input).tags)
            {
                if (tag is TagOpen oTag)
                    tagsEaxO1.Add(oTag.n.value);
            }

            tagsEaxO2.UnionWith(Parsers.ParseOpenCloseO5(input));

            Console.WriteLine(String.Join(", ", tagsSax));
            Console.WriteLine(String.Join(", ", tagsEaxO1));
            Console.WriteLine(String.Join(", ", tagsEaxO2));

            return tagsSax.SetEquals(tagsEaxO1) &&
                   tagsSax.SetEquals(tagsEaxO2);
        }

        private static string GenerateShallowTestInput(int limit)
            => Generator.ArbitraryBalancedSequenceShallow(limit);
    }
}