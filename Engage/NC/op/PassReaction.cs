using System.Collections.Generic;

namespace Engage.NC;

public class PassReaction : Reaction
{
    public override bool Equals(object obj)
        => obj is PassReaction;

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.DoNothing();

    internal override IEnumerable<FC.SignedFlag> ToTagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction>();
}