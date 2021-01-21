using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using takmelalexer;

namespace EngageTests
{
    [TestClass]
    public class ManyTests
    {
        [TestMethod]
        [TestCategory("EAX")]
        public void TestSaxShallow()
        {
            SortedDictionary<int, long> totalResults = new SortedDictionary<int, long>();
            for (int i = 0; i <= 10; i++)
            {
                List<Tuple<int, long>> results = new List<Tuple<int, long>>();
                for (double j = 10; j < 12000; j = j * 1.3 + 10)
                {
                    int limit = (int) Math.Floor(j);
                    long ticks = TimeCountSaxShallow(limit);
                    if (!totalResults.ContainsKey(limit))
                    {
                        totalResults.Add(limit, ticks);
                    }
                    else
                    {
                        totalResults[limit] = (long) totalResults[limit] + ticks;
                    }
                }
            }

            PrintResults(totalResults);
        }

        private void PrintResults(SortedDictionary<int, long> totalResults)
        {
            List<int> limits = new List<int>();
            List<long> ticks = new List<long>();
            foreach (var kvp in totalResults)
            {
                limits.Add(kvp.Key);
                ticks.Add(kvp.Value);
            }

            Console.Write("[");
            foreach (int limit in limits)
            {
                Console.Write("{0}, ", limit);
            }
            Console.WriteLine("]");
            
            Console.Write("[");
            foreach (long tick in ticks)
            {
                Console.Write("{0}, ", tick/10);
            }
            Console.WriteLine("]");
        }
        
        private long TimeCountSaxShallow(int limit)
        {
            var input = Generator.ArbitraryBalancedSequenceShallow(limit);
            var timer = new Stopwatch();
            timer.Start();
            

            Set<string> tags = new Set<string>();
            
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