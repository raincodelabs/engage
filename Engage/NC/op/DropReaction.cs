﻿using System;
using System.Collections.Generic;
using Engage.FC;

namespace Engage.NC;

public class DropReaction : Reaction
{
    public string Flag = "";

    public override bool Equals(object obj)
    {
        var other = obj as DropReaction;
        if (other == null)
        {
            Console.WriteLine("[x] DropReaction compared to non-DropReaction");
            return false;
        }

        if (Name != other.Name)
        {
            Console.WriteLine("[x] DropReaction: Name mismatch");
            return false;
        }

        if (Flag != other.Flag)
        {
            Console.WriteLine("[x] DropReaction: Flag mismatch");
            return false;
        }

        Console.WriteLine("[√] DropReaction == DropReaction");
        return true;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.DropFlag(Flag);

    internal override IEnumerable<string> GetFlags()
        => new List<string> { Flag };

    internal override IEnumerable<FC.SignedFlag> ToFlagActions()
        => new List<FC.SignedFlag> { new FlagDown(Flag) };

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction>();
}