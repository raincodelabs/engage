﻿using System;
using System.Collections.Generic;

namespace Engage.NC;

public class LiftReaction : Reaction
{
    public string Flag = "";

    public override bool Equals(object obj)
    {
        var other = obj as LiftReaction;
        if (other == null)
        {
            Console.WriteLine("[x] LiftReaction compared to non-LiftReaction");
            return false;
        }

        if (Name != other.Name)
        {
            Console.WriteLine($"[x] LiftReaction: Name mismatch ({Name} vs {other.Name})");
            return false;
        }

        if (Flag != other.Flag)
        {
            Console.WriteLine("[x] LiftReaction: Flag mismatch");
            return false;
        }

        Console.WriteLine("[√] LiftReaction == LiftReaction");
        return true;
    }

    public override NA.HandleAction ToHandleAction(string target = "", NA.HandleAction prev = null)
        => new NA.LiftFlag { Flag = Flag };

    internal override IEnumerable<string> GetFlags()
        => new List<string> { Flag };

    internal override IEnumerable<FC.SignedFlag> ToFlagActions()
        => new List<FC.SignedFlag> { new FC.FlagUp(Flag) };

    internal override IEnumerable<FC.StackAction> ToStackActions()
        => new List<FC.StackAction>();
}