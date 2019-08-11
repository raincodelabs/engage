using System;
using System.Collections.Generic;

namespace AB
{
    internal class ManualParser
    {
        // flags
        private bool DCL, BRACKET, CHAR, MAP, IF;

        private Stack<Object> Main = new Stack<Object>();

        private Dictionary<System.Type, Queue<Action<object>>> Pending = new Dictionary<System.Type, Queue<Action<object>>>();

        private void Push(object x)
        {
            System.Type t = x.GetType();
            if (Pending.ContainsKey(t) && Pending[t].Count > 0)
            {
                Action<object> a = Pending[t].Dequeue();
                a(x);
            }
            else
                Main.Push(x);
        }

        private void LetWait(System.Type type, Action<object> action)
        {
            if (Main.Peek().GetType() == type)
            {
                action(Main.Pop());
                return;
            }
            if (!Pending.ContainsKey(type))
                Pending[type] = new Queue<Action<object>>();
            Pending[type].Enqueue(action);
        }

        private string _input;
        private int _pos;

        public ManualParser(string input)
        {
            _input = input;
            _pos = 0;
        }

        public object Parse()
        {
            string ERROR = "";
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

                    case TokenType.Treserved:
                        switch (lexeme)
                        {
                            case "dcl":
                                DCL = true;
                                break;

                            case "enddcl":
                                DCL = false;
                                break;

                            case "char":
                                CHAR = true;
                                LetWait(typeof(Num), _n =>
                                {
                                    var n = _n as Num;
                                    CHAR = false;
                                    if (BRACKET)
                                        Push(new String(n));
                                    else
                                        ERROR = "Flag BRACKET is not raised";
                                }
                                );
                                break;

                            case "map":
                                MAP = true;
                                LetWait(typeof(Expr), x =>
                                {
                                    var source = x as Expr;
                                    MAP = false;
                                    MAP = true;
                                    LetWait(typeof(Var), y =>
                                    {
                                        var target = y as Var;
                                        Push(new MapStmt(source, target));
                                        MAP = false;
                                    });
                                });
                                break;
                        }
                        break;
                }
                if (!System.String.IsNullOrEmpty(ERROR))
                {
                    Console.WriteLine("Parser error: " + ERROR);
                    return null;
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
                t = TokenType.Treserved;
                s = "dcl";
                _pos += 3;
            }
            else if (_input.StartsWith("enddcl"))
            {
                // reserved
                t = TokenType.Treserved;
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

        private enum TokenType
        {
            TUndefined,
            Treserved,
            TNum,
            TVar,
            TEOF,
        }
    }
}