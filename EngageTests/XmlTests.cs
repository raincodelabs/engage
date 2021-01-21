using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using EAX;
using EaxOpenClose;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using takmelalexer;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;

namespace EngageTests
{
    [TestClass]
    public class XmlTests
    {
        [TestMethod]
        [TestCategory("EAX")]
        public void ParseEmpty()
        {
            var result = Parsers.ParseOpenClose("");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.tags.Count);
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void ParseOneTagOpen()
        {
            var result = Parsers.ParseOpenClose("<tag>");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.tags.Count);
            var tag = result.tags[0] as TagOpen;
            Assert.IsNotNull(tag);
            Assert.AreEqual("tag", tag.n?.value);
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void ParseOneTagOpenClose()
        {
            var result = Parsers.ParseOpenClose("<tag></tag>");
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.tags.Count);
            var tag1 = result.tags[0] as TagOpen;
            Assert.IsNotNull(tag1);
            var tag2 = result.tags[1] as TagClose;
            Assert.IsNotNull(tag2);
            Assert.AreEqual(tag1.n?.value, tag2.n?.value);
        }
        
        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountEax0k1()
            => TimeCountEax(100);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountEax1k()
            => TimeCountEax(1000);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountEax10k()
            => TimeCountEax(10000);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountEax100k()
            => TimeCountEax(100000);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeDepthBalancedEax0k1()
            => TimeDepthBalancedEax(100);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeDepthBalancedEax1k()
            => TimeDepthBalancedEax(1000);

        [TestMethod]
        [TestCategory("EAX")]
        // Dies with stack overflow
        public void TimeDepthBalancedEax10k()
            => TimeDepthBalancedEax(10000);

        /*
        [TestMethod]
        [TestCategory("EAX")]
        // Dies with stack overflow
        public void TimeDepthBalancedEax100k()
            => TimeDepthBalancedEax(100000);
            */
        
        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedSaxDeep0k1()
            => TimeCountSaxDeep(100);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedSaxDeep1k()
            => TimeCountSaxDeep(1000);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedSaxDeep10k()
            => TimeCountSaxDeep(10000);
        
        /*[TestMethod]
        [TestCategory("EAX")]
        //Also dies with stack overflow
        public void TimeCountBalancedSax100k()
            => Assert.Fail(); // TODO!*/

        
        private void TimeCountEax(int limit)
        {
            var input = Generator.ArbitrarySequence(limit: limit);
            // Console.WriteLine(input);
            var timer = new Stopwatch();
            timer.Start();
            var output = Parsers.ParseOpenClose(input);
            timer.Stop();
            Console.WriteLine(
                $"Parsed an input with {Math.Floor((double) limit / 1000)}k tags in {timer.ElapsedMilliseconds}ms.");
            Assert.IsNotNull(output);
            Assert.AreEqual(limit, output.tags.Count);
            timer.Restart();
            var tags = CountTags(output);
            timer.Stop();
            Console.WriteLine(
                $"Counted {tags} different tags in {timer.ElapsedTicks} ticks.");
        }

        private void TimeDepthBalancedEax(int limit)
        {
            var input = Generator.ArbitraryBalancedSequenceDeep(limit: limit);
            Console.WriteLine(input);
            var timer = new Stopwatch();
            timer.Start();
            var output = Parsers.ParseOpenClose(input);
            timer.Stop();
            Console.WriteLine(
                $"Parsed an input with {Math.Floor((double) limit / 1000)}k tags in {timer.ElapsedMilliseconds}ms.");
            Assert.IsNotNull(output);
            timer.Restart();
            var depth = MeasureDepth(output);
            timer.Stop();
            Console.WriteLine(
                $"Depth measured to be {depth} in {timer.ElapsedTicks} ticks.");
        }

        private void TimeValidateBalancedEax(int limit)
        {
            var input = Generator.ArbitraryBalancedSequenceDeep(limit: limit);
            // Console.WriteLine(input);
            var timer = new Stopwatch();
            timer.Start();
            var output = Parsers.ParseOpenClose(input);
            timer.Stop();
            Console.WriteLine(
                $"Parsed an input with {Math.Floor((double) limit / 1000)}k tags in {timer.ElapsedMilliseconds}ms.");
            Assert.IsNotNull(output);
            timer.Restart();
            var depth = MeasureDepth(output);
            timer.Stop();
            Console.WriteLine(
                $"Depth measured to be {depth} in {timer.ElapsedTicks} ticks.");
        }

        private int CountTags(EngagedXmlDoc tree)
        {
            Set<string> tags = new Set<string>();
            foreach (var tag in tree.tags)
                switch (tag)
                {
                    case TagOpen open:
                        tags.Add(open.n.value);
                        break;
                    case TagClose close:
                        tags.Add(close.n.value);
                        break;
                }

            return tags.Count;
        }

        private int MeasureDepth(EngagedXmlDoc tree)
        {
            int max = 0;
            int dep = 0;
            foreach (var tag in tree.tags)
                switch (tag)
                {
                    case TagOpen _:
                        dep++;
                        if (dep > max)
                            max++;
                        break;
                    case TagClose _:
                        dep--;
                        break;
                }

            return max;
        } 

        private bool ValidateBalance(EngagedXmlDoc tree)
        {
            Stack<string> trace = new Stack<string>();
            foreach (var tag in tree.tags)
                switch (tag)
                {
                    case TagOpen open:
                        trace.Push(open.n.value);
                        break;
                    case TagClose close:
                        var name = trace.Pop();
                        if (name != close.n.value)
                            return false;
                        break;
                }

            return trace.Count == 0;
        }

       
        
        private void TimeCountSaxDeep(int limit)
        {
            var input = Generator.ArbitraryBalancedSequenceDeep(limit);
            var timer = new Stopwatch();
            timer.Start();
            var reader = XmlReader.Create(new StringReader(input));
            timer.Stop();
            Console.WriteLine(
                $"Parsed an input with {Math.Floor((double) limit / 1000)}k tags in {timer.ElapsedMilliseconds}ms.");
            timer.Restart();

            Set<string> tags = new Set<string>();
            
            timer.Stop();
            Console.WriteLine(
                $"Counted {tags.Count} different tags in {timer.ElapsedTicks} ticks.");
        }
        
        
    }
}