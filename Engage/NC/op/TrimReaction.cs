using System.Collections.Generic;

namespace Engage.NC;

public class TrimReaction : Reaction
{
    public bool Starred { get; init; }

    public override bool Equals(object obj)
    {
        var other = obj as TrimReaction;
        if (other == null)
            return false;
        return Name == other.Name
               && Starred == other.Starred;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.TrimStream { Type = Name, Starred = Starred };

    internal override IEnumerable<FC.SignedFlag> ToFlagActions()
        => new List<FC.SignedFlag>();

    // TODO: refine!
    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction>();
}