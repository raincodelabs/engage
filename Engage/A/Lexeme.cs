using System;

namespace Engage.A
{
    public class Lexeme
    {
        public bool Special = false;
    }

    public class LiteralLex : Lexeme
    {
        public string Literal;

        public override bool Equals(object obj)
        {
            var other = obj as LiteralLex;
            if (other == null)
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

        public LiteralLex()
        {
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
            var other = obj as NumberLex;
            if (other == null)
            {
                Console.WriteLine("[x] NumberLex compared to non-NumberLex");
                return false;
            }

            if (Special != other.Special)
            {
                Console.WriteLine($"[x] NumberLex: Special mismatch");
                return false;
            }

            return true;
        }
    }

    public class StringLex : Lexeme
    {
        public override bool Equals(object obj)
        {
            var other = obj as StringLex;
            if (other == null)
            {
                Console.WriteLine("[x] StringLex compared to non-StringLex");
                return false;
            }

            if (Special != other.Special)
            {
                Console.WriteLine($"[x] StringLex: Special mismatch");
                return false;
            }

            return true;
        }
    }
}