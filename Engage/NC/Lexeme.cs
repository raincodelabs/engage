using System;

namespace Engage.NC;

public class Lexeme
{
    public bool Special = false;
}

public class LiteralLex : Lexeme
{
    public readonly string Literal;

    public override bool Equals(object obj)
    {
        if (obj is not LiteralLex other)
        {
            Console.WriteLine("[x] LiteralLex compared to non-LiteralLex");
            return false;
        }

        if (Special != other.Special)
        {
            Console.WriteLine($"[x] LiteralLex: Special mismatch");
            return false;
        }

        if (Literal != other.Literal)
        {
            Console.WriteLine($"[x] LiteralLex: Literal mismatch ({Literal} vs {other.Literal})");
            return false;
        }

        return true;
    }

    public override int GetHashCode()
        => Literal.GetHashCode();

    public LiteralLex()
    {
        Literal = String.Empty;
    }

    public LiteralLex(string q)
    {
        if (q.StartsWith("'") && q.EndsWith("'"))
            Literal = q.Substring(1, q.Length - 2);
        else
            Literal = q;
    }
}

public class NumberLex : Lexeme
{
    public override bool Equals(object obj)
    {
        if (obj is not NumberLex other)
        {
            Console.WriteLine("[x] NumberLex compared to non-NumberLex");
            return false;
        }

        if (Special == other.Special) return true;

        Console.WriteLine("[x] NumberLex: Special mismatch");
        return false;
    }

    public override int GetHashCode()
        => Special.GetHashCode();
}

public class StringLex : Lexeme
{
    public override bool Equals(object obj)
    {
        if (obj is not StringLex other)
        {
            Console.WriteLine("[x] StringLex compared to non-StringLex");
            return false;
        }

        if (Special == other.Special) return true;
        
        Console.WriteLine("[x] StringLex: Special mismatch");
        return false;

    }

    public override int GetHashCode()
        => Special.GetHashCode();
}