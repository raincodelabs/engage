namespace Engage.A
{
    public partial class Lexeme
    {
        public bool Special;
    }

    public partial class LiteralLex : Lexeme
    {
        public string Literal;
    }

    public partial class NumberLex : Lexeme
    {
    }

    public partial class StringLex : Lexeme
    {
    }
}