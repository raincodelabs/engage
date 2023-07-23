using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EAX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace EngageTests;

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
        var tag = result.tags[0] as EaxOpenClose.TagOpen;
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
        var tag1 = result.tags[0] as EaxOpenClose.TagOpen;
        Assert.IsNotNull(tag1);
        var tag2 = result.tags[1] as EaxOpenClose.TagClose;
        Assert.IsNotNull(tag2);
        Assert.AreEqual(tag1.n?.value, tag2.n?.value);
    }

    [TestMethod]
    [TestCategory("EAX")]
    public void ParseFuzzyNothing()
    {
        var result = Parsers.ParseFuzzy("<tag></tag>");
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.tags.Count);
    }

    [TestMethod]
    [TestCategory("EAX")]
    public void ParseFuzzyOne()
    {
        var result = Parsers.ParseFuzzy("<a><x></x></a>");
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.tags.Count);
        var open = result.tags[0] as EaxFuzzy.TagOpen;
        Assert.IsNotNull(open);
        Assert.AreEqual("x", open.n?.value);
        var close = result.tags[1] as EaxFuzzy.TagClose;
        Assert.IsNotNull(close);
        Assert.AreEqual("x", close.n?.value);
    }

    [TestMethod]
    [TestCategory("EAX")]
    public void ParseFuzzyTwoOutOfThree()
    {
        var result = Parsers.ParseFuzzy("<root><a><b></b></a><b></b><a><b></b></a></root>");
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.tags.Count);
        // first one
        var open = result.tags[0] as EaxFuzzy.TagOpen;
        Assert.IsNotNull(open);
        Assert.AreEqual("b", open.n?.value);
        var close = result.tags[1] as EaxFuzzy.TagClose;
        Assert.IsNotNull(close);
        Assert.AreEqual("b", close.n?.value);
        // second one is skipped because it's not wrapped in <a>...</a>
        // third one
        open = result.tags[0] as EaxFuzzy.TagOpen;
        Assert.IsNotNull(open);
        Assert.AreEqual("b", open.n?.value);
        close = result.tags[1] as EaxFuzzy.TagClose;
        Assert.IsNotNull(close);
        Assert.AreEqual("b", close.n?.value);
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
    [Ignore]
    // Dies with stack overflow
    public void TimeDepthBalancedEax10k()
        => TimeDepthBalancedEax(10000);

    [TestMethod]
    [TestCategory("EAX")]
    [Ignore]
    // Dies with stack overflow
    public void TimeDepthBalancedEax100k()
        => TimeDepthBalancedEax(100000);

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
    [Ignore]
    public void TimeCountBalancedSaxDeep10k()
        => TimeCountSaxDeep(10000);

    [TestMethod]
    [TestCategory("EAX")]
    [Ignore]
    //Also dies with stack overflow
    public void TimeCountBalancedSax100k()
        => Assert.Fail(); // TODO!

    [TestMethod]
    [TestCategory("EAX")]
    public void TimeFindEax0k1()
        => TimeFindEax(100);

    [TestMethod]
    [TestCategory("EAX")]
    public void TimeFindEax1k()
        => TimeFindEax(1000);

    [TestMethod]
    [TestCategory("EAX")]
    [Ignore]
    // Dies with a stack overflow
    public void TimeFindEax10k()
        => TimeFindEax(10000);

    [TestMethod]
    [TestCategory("EAX")]
    [Ignore]
    // Dies with a stack overflow
    public void TimeFindEax100k()
        => TimeFindEax(100000);

    private void TimeFindEax(int limit)
    {
        var input = Generator.ArbitraryBalancedSequenceShallowConstName("a", limit: limit);
        Console.WriteLine(input);
        var timer = new Stopwatch();
        timer.Start();
        var output = Parsers.ParseFuzzy(input);
        timer.Stop();
        Console.WriteLine(
            $"Parsed an input with {Math.Floor((double) limit / 1000)}k tags in {timer.ElapsedMilliseconds}ms.");
        Assert.IsNotNull(output);
        Assert.AreEqual(2 * limit, output.tags.Count);
        timer.Restart();
        var tags = CountTagsFuzzy(output);
        timer.Stop();
        Console.WriteLine(
            $"Counted {tags} different tags in {timer.ElapsedTicks} ticks.");
    }

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
        var tags = CountTagsOpenClose(output);
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

    private int CountTagsOpenClose(EaxOpenClose.EngagedXmlDoc tree)
    {
        HashSet<string> tags = new HashSet<string>();
        foreach (var tag in tree.tags)
            switch (tag)
            {
                case EaxOpenClose.TagOpen open:
                    tags.Add(open.n.value);
                    break;
                case EaxOpenClose.TagClose close:
                    tags.Add(close.n.value);
                    break;
            }

        return tags.Count;
    }

    private int CountTagsFuzzy(EaxFuzzy.EngagedXmlDoc tree)
    {
        HashSet<string> tags = new HashSet<string>();
        foreach (var tag in tree.tags)
            switch (tag)
            {
                case EaxFuzzy.TagOpen open:
                    tags.Add(open.n.value);
                    break;
                case EaxFuzzy.TagClose close:
                    tags.Add(close.n.value);
                    break;
            }

        return tags.Count;
    }

    private int MeasureDepth(EaxOpenClose.EngagedXmlDoc tree)
    {
        int max = 0;
        int dep = 0;
        foreach (var tag in tree.tags)
            switch (tag)
            {
                case EaxOpenClose.TagOpen _:
                    dep++;
                    if (dep > max)
                        max++;
                    break;
                case EaxOpenClose.TagClose _:
                    dep--;
                    break;
            }

        return max;
    }

    private bool ValidateBalance(EaxOpenClose.EngagedXmlDoc tree)
    {
        Stack<string> trace = new Stack<string>();
        foreach (var tag in tree.tags)
            switch (tag)
            {
                case EaxOpenClose.TagOpen open:
                    trace.Push(open.n.value);
                    break;
                case EaxOpenClose.TagClose close:
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

        HashSet<string> tags = new HashSet<string>();

        timer.Stop();
        Console.WriteLine(
            $"Counted {tags.Count} different tags in {timer.ElapsedTicks} ticks.");
    }
}