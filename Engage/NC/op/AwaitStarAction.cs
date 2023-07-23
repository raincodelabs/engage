using System.Collections.Generic;

namespace Engage.NC;

public class AwaitStarAction : Reaction
{
    public string TmpContext = "";

    public override bool Equals(object obj)
    {
        var other = obj as AwaitStarAction;
        if (other == null)
            return false;
        return Name == other.Name
               && TmpContext == other.TmpContext;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
    {
        var a = new NA.AwaitMany
        {
            Name = NA.SystemPlan.Dealias(Name),
            Target = target,
            Flag = TmpContext,
            BaseAction = prev
        };
        return a;
    }

    internal override IEnumerable<FC.SignedFlag> ToTagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction> { new FC.StackAwaitAll(Name) };
}