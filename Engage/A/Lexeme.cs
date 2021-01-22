namespace Engage.A
{
    public class Lexeme
    {
        public bool Special = false;
    }

    public class LiteralLex : Lexeme
    {
        public string Literal;

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
    }

    public class StringLex : Lexeme
    {
    }
}