using System;
using Engage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngageTests;

[TestClass]
public class FormalTests
{
    private const string Header = "namespace X types A; tokens 'a', 'b' :: mark handlers ";

    private int CountSubstrings(string main, string sub)
        => (main.Length - main.Replace(sub, "").Length) / sub.Length;

    [TestMethod]
    [TestCategory("Formal")]
    public void ProducedConcreteSpec()
    {
        var spec = FrontEnd.TextToFormalConcrete(Header, false);
        Assert.IsNotNull(spec);
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void ConcreteEmptyWithoutHandlers()
    {
        var spec = FrontEnd.TextToFormalConcrete(Header, false);
        Assert.AreEqual(spec.ToString(), String.Empty);
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void EndPush()
    {
        var text = Header + "EOF -> push A()";
        var spec = FrontEnd.TextToFormalConcrete(text, false);
        Assert.AreEqual(spec.ToString(), "(#1) EOF --> {+A}");
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void EndPushPop()
    {
        var text = Header + "EOF -> push A(x) where x := pop B";
        var spec = FrontEnd.TextToFormalConcrete(text, false);
        Assert.AreEqual(spec.ToString(), "(#1) EOF --> {-B,+A}");
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void MarkPush()
    {
        var text = Header + "'a' -> push A()";
        var spec = FrontEnd.TextToFormalConcrete(text, false);
        Assert.AreEqual(spec.ToString(), "(#1) 'a' --> {+A}");
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void MarkPushPop()
    {
        var text = Header + "'a' -> push A(x) where x := pop B";
        var spec = FrontEnd.TextToFormalConcrete(text, false);
        Assert.AreEqual(spec.ToString(), "(#1) 'a' --> {-B,+A}");
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void ProducedAbstractSpec()
    {
        var spec = FrontEnd.TextToFormalAbstract(Header, false);
        Assert.IsNotNull(spec);
    }

    [TestMethod]
    [TestCategory("Formal")]
    public void AbstractEmptyWithoutHandlers()
    {
        var spec = FrontEnd.TextToFormalAbstract(Header, false);
        var dot = spec.ToDot();
        Assert.AreEqual(CountSubstrings(dot, "label="), 1);
    }
}