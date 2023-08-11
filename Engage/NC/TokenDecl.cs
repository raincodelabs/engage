using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.NC;

public class TokenDecl
{
    public readonly List<Lexeme> Names = new();
    public string Type;

    public override bool Equals(object obj)
    {
        if (obj is not TokenDecl other)
        {
            Console.WriteLine("[x] TokenDecl compared to non-TokenDecl");
            return false;
        }

        if (Type != other.Type)
        {
            Console.WriteLine($"[x] TokenDecl: Type mismatch");
            return false;
        }

        if (Names.Count != other.Names.Count)
        {
            Console.WriteLine("[x] TokenDecl: Names count mismatch");
            return false;
        }

        if (!Names.SequenceEqual(other.Names))
        {
            Console.WriteLine("[x] TokenDecl: Names mismatch");
            return false;
        }

        return true;
    }

    public override int GetHashCode()
        => Type.GetHashCode()
           + Names.Select(name => name.GetHashCode()).Sum();

    internal List<NA.TokenPlan> MakePlans()
    {
        var ts = new List<NA.TokenPlan>();
        foreach (var a in Names)
            if (a is NC.NumberLex)
                ts.Add(new NA.TokenPlan { Special = true, Value = "number" });
            else if (a is NC.StringLex)
                ts.Add(new NA.TokenPlan { Special = true, Value = "string" });
            else if (a is NC.LiteralLex al)
                ts.Add(new NA.TokenPlan { Special = false, Value = al.Literal });
        return ts;
    }
}