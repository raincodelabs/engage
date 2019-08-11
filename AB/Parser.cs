// Engage! generated this file, please do not edit manually
using System;
using System.Collections.Generic;

namespace AB
{
    public class Parser
    {
        private enum TokenType
        {
            TUndefined,
            TEOF,
            Treserved,
            TNum,
            TVar,
        }

        private bool DCL, BRACKET, CHAR, MAP;
        private int IF;
        private Stack<Object> Main = new Stack<Object>();
        private string input;
        private int pos;
        private Dictionary<System.Type, Queue<Action<object>>> Pending = new Dictionary<System.Type, Queue<Action<object>>>();

        public Parser(string _input)
        {
            input = _input;
            pos = 0;
        }

        public object Parse()
        {
            string ERROR = "";
            TokenType type;
            string lexeme;
            do
            {
                var _token = NextToken();
                lexeme = _token.Item2;
                type = _token.Item1;
                switch (type)
                {
                    case TokenType.TEOF:
                        var data = new List<Decl>();
                        while (Main.Peek() is Decl)
                        {
                            data.Add(Main.Pop() as Decl);
                        }
                        var code = new List<Stmt>();
                        while (Main.Peek() is Stmt)
                        {
                            code.Add(Main.Pop() as Stmt);
                        }
                        Push(new ABProgram(data, code));
                        break;

                    case TokenType.Treserved:
                        switch (lexeme)
                        {
                            case "integer":
                                if (!DCL)
                                {
                                    ERROR = "flag DCL not lifted when expected";
                                }
                                Push(new Integer());
                                break;

                            case "enddcl":
                                DCL = false;
                                break;

                            case "endif":
                                IF--;
                                break;

                            case "char":
                                if (!DCL)
                                {
                                    ERROR = "flag DCL not lifted when expected";
                                }
                                CHAR = true;
                                LetWait(typeof(Num), _n =>
                                {
                                    var n = _n as Num;
                                    CHAR = false;
                                    if (!BRACKET)
                                    {
                                        ERROR = "flag BRACKET was not raised when expected";
                                    }
                                    Push(new String(n));
                                }
                                );
                                break;

                            case "dcl":
                                DCL = true;
                                break;

                            case "map":
                                MAP = true;
                                LetWait(typeof(Expr), _source =>
                                {
                                    var source = _source as Expr;
                                    MAP = false;
                                    MAP = true;
                                    LetWait(typeof(Var), _target =>
                                    {
                                        var target = _target as Var;
                                        MAP = false;
                                        Push(new MapStmt(source, target));
                                    }
                                    );
                                }
                                );
                                break;

                            case "if":
                                IF++;
                                LetWait(typeof(Expr), _cond =>
                                {
                                    var cond = _cond as Expr;
                                    IF--;
                                }
                                );
                                break;

                            case ";":
                                if (!DCL)
                                {
                                    ERROR = "flag DCL not lifted when expected";
                                }
                                Type t;
                                if (Main.Peek() is Type)
                                {
                                    t = Main.Pop() as Type;
                                }
                                else
                                {
                                    ERROR = "the top of the stack is not of type Type";
                                    t = null;
                                }
                                Var v;
                                if (Main.Peek() is Var)
                                {
                                    v = Main.Pop() as Var;
                                }
                                else
                                {
                                    ERROR = "the top of the stack is not of type Var";
                                    v = null;
                                }
                                Push(new Decl(v, t));
                                break;

                            case "(":
                                if (!CHAR)
                                {
                                    ERROR = "flag CHAR not lifted when expected";
                                }
                                BRACKET = true;
                                break;

                            case ")":
                                if (!CHAR)
                                {
                                    ERROR = "flag CHAR not lifted when expected";
                                }
                                BRACKET = false;
                                break;

                        }
                        break;

                    case TokenType.TNum:
                        Push(new Num(System.Int32.Parse(lexeme)));
                        break;

                    case TokenType.TVar:
                        Push(new Var(lexeme));
                        break;

                }
                if (!System.String.IsNullOrEmpty(ERROR))
                {
                    Console.WriteLine("Parser error: " + ERROR);
                    return null;
                }
            }
            while (type != TokenType.TEOF);
            return null;
        }

        private void LetWait(System.Type _type, Action<object> _action)
        {
            if (Main.Peek().GetType() == _type)
            {
                _action(Main.Pop());
                return;
            }
            if (!Pending.ContainsKey(_type))
            {
                Pending[_type] = new Queue<Action<object>>();
            }
            Pending[_type].Enqueue(_action);
        }

        private void Push(object _x)
        {
            System.Type _t = _x.GetType();
            if (Pending.ContainsKey(_t) && Pending[_t].Count > 0)
            {
                Action<object> _a = Pending[_t].Dequeue();
                _a(_x);
            }
            else
            {
                Main.Push(_x);
            }
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
            else if (pos + 4 < input.Length && input[pos] == 'e' && input[pos + 1] == 'n' && input[pos + 2] == 'd' && input[pos + 3] == 'i' && input[pos + 4] == 'f')
            {
                t = TokenType.Treserved;
                s = "endif";
                pos += 5;
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

    }
}
