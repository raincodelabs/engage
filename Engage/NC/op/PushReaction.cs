using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.NC;

public class PushReaction : Reaction
{
    public readonly List<string> Args = new();

    public override bool Equals(object obj)
    {
        var other = obj as PushReaction;
        if (other == null)
        {
            Console.WriteLine("[x] PushReaction compared to non-PushReaction");
            return false;
        }

        if (Name != other.Name)
        {
            Console.WriteLine("[x] PushReaction: Name mismatch");
            return false;
        }

        if (!Args.SequenceEqual(other.Args))
        {
            Console.WriteLine("[x] PushReaction: Args mismatch");
            return false;
        }

        Console.WriteLine("[√] PushReaction == PushReaction");
        return true;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => prev is not NA.TearOne t
            ? new NA.PushNew(Name, Args, "")
            : new NA.PushNew(Name, Args, $"{t.Name}.{t.Target}");

    internal override IEnumerable<FC.SignedFlag> ToTagActions()
        => new List<FC.SignedFlag>();

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction> { new FC.StackPush(Name) };
}