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
            return null;
        }
        // TODO
    }
}
