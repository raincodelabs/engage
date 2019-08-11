// Engage! generated this file, please do not edit manually
using System;
using System.Collections.Generic;

namespace AB
{
    public class Parser
    {
        private bool DCL, BRACKET, CHAR, MAP;
        private int IF;
        private Stack<Object> Main = new Stack<Object>();
        private string input;
        private int pos;

        public Parser(string _input)
        {
            input = _input;
            pos = 0;
        }
        public object Parse()
        {
            TokenType type;
            string lexeme;
            do
            {
                var t = NextToken();
                lexeme = t.Item2;
                type = t.Item1;
            }
            while (type != TokenType.TEOF);
            return null;
        }
        private Tuple<TokenType, string> NextToken()
        {
            TokenType t = TokenType.TUndefined;
            string s = "";
            if (pos >= input.Length)
            {
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            }
            while (input[pos] == ' ' && pos < input.Length)
            {
                pos++;
            }
            if (pos >= input.Length)
            {
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            }
            else if (pos + 2 < input.Length && input[pos] == 'd' && input[pos + 1] == 'c' && input[pos + 2] == 'l')
            {
                t = TokenType.Treserved;
                s = "dcl";
                pos += 3;
            }
            else if (pos + 5 < input.Length && input[pos] == 'e' && input[pos + 1] == 'n' && input[pos + 2] == 'd' && input[pos + 3] == 'd' && input[pos + 4] == 'c' && input[pos + 5] == 'l')
            {
                t = TokenType.Treserved;
                s = "enddcl";
                pos += 6;
            }
            else if (pos + 6 < input.Length && input[pos] == 'i' && input[pos + 1] == 'n' && input[pos + 2] == 't' && input[pos + 3] == 'e' && input[pos + 4] == 'g' && input[pos + 5] == 'e' && input[pos + 6] == 'r')
            {
                t = TokenType.Treserved;
                s = "integer";
                pos += 7;
            }
            else if (pos + 3 < input.Length && input[pos] == 'c' && input[pos + 1] == 'h' && input[pos + 2] == 'a' && input[pos + 3] == 'r')
            {
                t = TokenType.Treserved;
                s = "char";
                pos += 4;
            }
            else if (pos + 1 < input.Length && input[pos] == 'i' && input[pos + 1] == 'f')
            {
                t = TokenType.Treserved;
                s = "if";
                pos += 2;
            }
            else if (pos + 2 < input.Length && input[pos] == 'm' && input[pos + 1] == 'a' && input[pos + 2] == 'p')
            {
                t = TokenType.Treserved;
                s = "map";
                pos += 3;
            }
            else if (pos + 1 < input.Length && input[pos] == 't' && input[pos + 1] == 'o')
            {
                t = TokenType.Treserved;
                s = "to";
                pos += 2;
            }
            else if (input[pos] == ';')
            {
                t = TokenType.Treserved;
                s = ";";
                pos++;
            }
            else if (input[pos] == '(')
            {
                t = TokenType.Treserved;
                s = "(";
                pos++;
            }
            else if (input[pos] == ')')
            {
                t = TokenType.Treserved;
                s = ")";
                pos++;
            }
            else if (input[pos] == '0' || input[pos] == '1' || input[pos] == '2' || input[pos] == '3' || input[pos] == '4' || input[pos] == '5' || input[pos] == '6' || input[pos] == '7' || input[pos] == '8' || input[pos] == '9')
            {
                t = TokenType.TNum;
                while (pos < input.Length && (input[pos] == '0' || input[pos] == '1' || input[pos] == '2' || input[pos] == '3' || input[pos] == '4' || input[pos] == '5' || input[pos] == '6' || input[pos] == '7' || input[pos] == '8' || input[pos] == '9'))
                {
                    s += input[pos++];
                }
            }
            else
            {
                t = TokenType.TVar;
                while (pos < input.Length && input[pos] != ' ')
                {
                    s += input[pos++];
                }
            }
            return new Tuple<TokenType, string>(t, s);
        }
        // TODO
    }
}
