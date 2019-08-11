using System;
using System.Collections.Generic;

namespace AB
{
    internal class ManualParser
    {
        // flags
        private bool DCL, BRACKET, CHAR, MAP, IF;

        private Stack<Object> Main = new Stack<Object>();

        private string _input;
        private int _pos;

        public ManualParser(string input)
        {
            _input = input;
            _pos = 0;
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

                switch (type)
                {
                    case TokenType.TEOF:
                        List<Decl> data = new List<Decl>();
                        while (Main.Peek() is Decl)
                            data.Add(Main.Pop() as Decl);
                        List<Stmt> code = new List<Stmt>();
                        while (Main.Peek() is Stmt)
                            code.Add(Main.Pop() as Stmt);
                        Main.Push(new ABProgram(data, code));
                        break;
                    case TokenType.TReserved:
                        switch (lexeme)
                        {
                            case "dcl":
                                DCL = true;
                                break;
                            case "enddcl":
                                DCL = false;
                                break;
                        }
                        break;
                }
            } while (type != TokenType.TEOF);

            return null;
        }

        private Tuple<TokenType, string> NextToken()
        {
            TokenType t = TokenType.TUndefined;
            string s = "";
            // EOF
            if (_pos > _input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            // skip
            while (_input[_pos] == ' ' && _pos < _input.Length)
                _pos++;
            // EOF after skip
            if (_pos > _input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            if (_input.StartsWith("dcl"))
            {
                // reserved
                t = TokenType.TReserved;
                s = "dcl";
                _pos += 3;
            }
            else if (_input.StartsWith("enddcl"))
            {
                // reserved
                t = TokenType.TReserved;
                s = "enddcl";
                _pos += 6;
            }
            // ...
            else if (_input[_pos] == '0' || _input[_pos] == '1' /* || ... */)
            {
                // Num
                t = TokenType.TNum;
                while (_pos < _input.Length && (_input[_pos] == '0' || _input[_pos] == '1' /* || ... */))
                    s += _input[_pos++];
            }
            else
            {
                // Var
                t = TokenType.TVar;
                while (_pos < _input.Length && _input[_pos] != ' ')
                    s += _input[_pos++];
            }
            return new Tuple<TokenType, string>(t, s);
        }
    }

    internal enum TokenType
    {
        TUndefined,
        TReserved,
        TNum,
        TVar,
        TEOF,
    }
}