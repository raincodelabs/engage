using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.FC;

public class Formula
{
    private readonly List<SignedTag> Tags = new();
    private readonly string Input;
    private readonly List<SignedTag> TagActions = new();
    private readonly List<StackAction> StackActions = new();

    public bool Tagged => Tags.Any();

    public Formula(
        IEnumerable<string> tags,
        string input,
        IEnumerable<SignedTag> tActions,
        IEnumerable<StackAction> sActions)
    {
        if (tags != null)
            foreach (var tag in tags)
                Tags.Add(new TagUp(tag));
        Tags.Sort();
        Input = input;
        if (tActions != null)
            TagActions.AddRange(tActions);
        if (sActions != null)
            StackActions.AddRange(sActions);
    }

    public Formula(Formula f1, Formula f2)
    {
        Tags.AddRange(f1.Tags); // assume f2.Tags are the same
        Input = f1.Input; // assume f2.Input is the same
        TagActions.AddRange(f1.TagActions);
        TagActions.AddRange(f2.TagActions);
        StackActions.AddRange(f1.StackActions);
        StackActions.AddRange(f2.StackActions);
    }

    internal bool LeftEquals(Formula other)
        => Tags.SequenceEqual(other.Tags) && InputEquals(other);

    internal bool InputEquals(Formula other)
        => Input.Equals(other.Input);

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

    public override bool Equals(object obj)
    {
        var other = obj as FC.Formula;
        if (other == null)
            return false;
        return Tags.SequenceEqual(other.Tags)
               && Input.Equals(other.Input)
               && TagActions.SequenceEqual(other.TagActions)
               && StackActions.SequenceEqual(other.StackActions);
    }

    public override int GetHashCode()
    {
        return Tags.GetHashCode() + Input.GetHashCode() + TagActions.GetHashCode() + StackActions.GetHashCode();
    }
}