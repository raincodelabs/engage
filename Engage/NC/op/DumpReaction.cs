using System;
using System.Collections.Generic;

namespace Engage.NC;

public class DumpReaction : Reaction
{
    public DumpReaction(string type)
    {
        Name = type ?? String.Empty;
    }

    public bool IsUniversal()
        => String.IsNullOrEmpty(Name);

    public override bool Equals(object obj)
    {
        var other = obj as DumpReaction;
        if (other == null)
            return false;
        return Name == other.Name;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.DumpOne(Name);

    internal override IEnumerable<FC.SignedFlag> ToFlagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction> { new FC.StackPop(Name) };
}