using System;
using System.Collections.Generic;

namespace Engage.FC;

public class Specification
{
    private List<string> Tags = new();
    private string Input;
    private List<TagAction> TagActions = new();
    private List<StackAction> StackActions = new();

    public override string ToString()
    {
        List<string> elements = new();
        if (Tags != null && Tags.Count > 0)
            elements.Add("[" + String.Join(",", Tags) + "]");
        if (!String.IsNullOrEmpty(Input))
            elements.Add("'" + Input + "'");
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
    public readonly string Type;
}

public class StackPop : StackAction
{
    public override string ToString()
        => $"-{Type}";
}

public class StackPush : StackAction
{
    public override string ToString()
        => $"+{Type}";
}