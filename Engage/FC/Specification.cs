using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.FC;

public class Specification
{
    private readonly List<Formula> _formulae = new();

    public void AddFormula(Formula f)
        => _formulae.Add(f);

    public override string ToString()
    {
        var fs = _formulae.Select(f => f.ToString());
        return String.Join(Environment.NewLine, fs);
    }
}

public class Formula
{
    private List<string> Tags = new();
    private string Input;
    private List<TagAction> TagActions = new();
    private List<StackAction> StackActions = new();

    public Formula(
        IEnumerable<string> tags,
        string input,
        IEnumerable<TagAction> tActions,
        IEnumerable<StackAction> sActions)
    {
        if (tags != null)
            Tags.AddRange(tags);
        Input = input;
        if (tActions != null)
            TagActions.AddRange(tActions);
        if (sActions != null)
            StackActions.AddRange(sActions);
    }

    public override string ToString()
    {
        List<string> elements = new();
        if (Tags != null && Tags.Count > 0)
            elements.Add("[" + String.Join(",", Tags) + "]");
        if (!String.IsNullOrEmpty(Input))
            elements.Add(Input);
        elements.Add("->");
        if (TagActions != null && TagActions.Count > 0)
            elements.Add("[" + String.Join(",", TagActions) + "]");
        if (StackActions != null && StackActions.Count > 0)
            elements.Add("{" + String.Join(",", StackActions) + "}");
        return String.Join(" ", elements);
    }
}

public class TagAction
{
    private bool flag;
    public bool Lift => flag;
    public bool Drop => !flag;

    public readonly string Name;

    public TagAction(bool flag, string name)
    {
        this.flag = flag;
        Name = name;
    }

    public override string ToString()
        => flag
            ? Name
            : "!" + Name;
}

public abstract class StackAction
{
    public string Type { get; init; }
}

public class StackPop : StackAction
{
    public override string ToString()
        => $"-{Type}";

    public StackPop(string type)
    {
        Type = type;
    }
}

public class StackPopS : StackAction
{
    public override string ToString()
        => $"-{Type}*";

    public StackPopS(string type)
    {
        Type = type;
    }
}

public class StackPush : StackAction
{
    public override string ToString()
        => $"+{Type}";

    public StackPush(string type)
    {
        Type = type;
    }
}