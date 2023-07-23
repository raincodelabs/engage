using System.Collections.Generic;

namespace Engage.NC;

public class PopAction : Reaction
{
    public override bool Equals(object obj)
    {
        var other = obj as PopAction;
        if (other == null)
            return false;
        return Name == other.Name;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.PopOne { Name = NA.SystemPlan.Dealias(Name), Target = target };

    internal override IEnumerable<FC.SignedFlag> ToFlagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction> { new FC.StackPop(Name) };
}