using System;
using System.Diagnostics;
using System.Text;
using EAX;
using EaxOpenClose;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void TimeCountBalancedEax100()
            => TimeCountBalancedEax(100);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedEax1k()
            => TimeCountBalancedEax(1000);

        [TestMethod]
        [TestCategory("EAX")]
        // Dies with stack overflow
        public void TimeCountBalancedEax10k()
            => TimeCountBalancedEax(10000);

        [TestMethod]
        [TestCategory("EAX")]
        // Dies with stack overflow
        public void TimeCountBalancedEax100k()
            => TimeCountBalancedEax(100000);

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountSax100()
            => Assert.Fail(); // TODO!

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountSax1k()
            => Assert.Fail(); // TODO!

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountSax10k()
            => Assert.Fail(); // TODO!

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountSax100k()
            => Assert.Fail(); // TODO!

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedSax1k()
            => Assert.Fail(); // TODO!

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedSax10k()
            => Assert.Fail(); // TODO!

        [TestMethod]
        [TestCategory("EAX")]
        public void TimeCountBalancedSax100k()
            => Assert.Fail(); // TODO!

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
        }

        private void TimeCountBalancedEax(int limit)
        {
            var input = Generator.ArbitraryBalancedSequence(limit: limit);
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
    }
}