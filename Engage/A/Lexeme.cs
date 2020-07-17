namespace Engage.A
{
    public class Lexeme
    {
        public bool Special;
    }

    public class LiteralLex : Lexeme
    {
        public string Literal;
    }

    public class NumberLex : Lexeme
    {
    }

    public class StringLex : Lexeme
    {
    }
}