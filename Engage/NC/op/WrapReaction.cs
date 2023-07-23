using System.Collections.Generic;
using System.Linq;

namespace Engage.NC;

public class WrapReaction : Reaction
{
    public readonly List<string> Args = new();

    public override bool Equals(object obj)
    {
        var other = obj as WrapReaction;
        if (other == null)
            return false;
        return Name == other.Name
               && Args.SequenceEqual(other.Args);
    }

    // NB: in this case the "target" argument is actually the type since we know the [intermediate] target from the call
    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.WrapOne(Name, target, Args[0]);

    internal override IEnumerable<FC.SignedFlag> ToTagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
    {
        throw new System.NotImplementedException();
    }
}