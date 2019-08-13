// Engage! generated this file, please do not edit manually
using EngageRuntime;
using System;
using System.Collections.Generic;

namespace AB
{
    public class Parser : BaseParser
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

        public Parser(string _input) : base(_input)
        {
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
                        Flush();
                        var code = new List<Stmt>();
                        var data = new List<Decl>();
                        while (Main.Count > 0)
                        {
                            if (Main.Peek() is Stmt)
                                code.Add(Main.Pop() as Stmt);
                            else if (Main.Peek() is Decl)
                                data.Add(Main.Pop() as Decl);
                            else
                                break;
                        }
                        code.Reverse();
                        data.Reverse();
                        Push(new ABProgram(data, code));
                        break;

                    case TokenType.Treserved:
                        switch (lexeme.ToLower())
                        {
                            case "integer":
                                if (!DCL)
                                    ERROR = "flag DCL not lifted when expected";
                                Push(new Integer());
                                break;

                            case "enddcl":
                                DCL = false;
                                break;

                            case "return":
                                Push(new ReturnStmt());
                                break;

                            case "endif":
                                Trim(typeof(Stmt));
                                break;

                            case "clear":
                                Schedule(typeof(Var), _view =>
                                {
                                    var view = _view as Var;
                                    Push(new ClearStmt(view));
                                    return Message.Perfect;
                                }
                                );
                                break;

                            case "char":
                                if (!DCL)
                                    ERROR = "flag DCL not lifted when expected";
                                CHAR = true;
                                Schedule(typeof(Num), _n =>
                                {
                                    var n = _n as Num;
                                    CHAR = false;
                                    if (!BRACKET)
                                        return Message.Misfire;
                                    Push(new String(n));
                                    return Message.Perfect;
                                }
                                );
                                break;

                            case "dcl":
                                DCL = true;
                                break;

                            case "map":
                                MAP = true;
                                Schedule(typeof(Expr), _source =>
                                {
                                    var source = _source as Expr;
                                    MAP = false;
                                    MAP = true;
                                    Schedule(typeof(Var), _target =>
                                    {
                                        var target = _target as Var;
                                        MAP = false;
                                        Push(new MapStmt(source, target));
                                        return Message.Perfect;
                                    }
                                    );
                                    return Message.Perfect;
                                }
                                );
                                break;

                            case "if":
                                Schedule(typeof(Expr), _cond =>
                                {
                                    var cond = _cond as Expr;
                                    List<Stmt> branch = new List<Stmt>();
                                    Schedule(typeof(Stmt), _branch =>
                                    {
                                        if (_branch == null)
                                        {
                                            Push(new IfStmt(cond, branch));
                                            return Message.Perfect;
                                        }
                                        var branch1 = _branch as Stmt;
                                        branch.Add(branch1);
                                        return Message.Consume;
                                    }
                                    );
                                    return Message.Perfect;
                                }
                                );
                                break;

                            case ";":
                                if (!DCL)
                                    ERROR = "flag DCL not lifted when expected";
                                Type t;
                                if (Main.Peek() is Type)
                                    t = Main.Pop() as Type;
                                else
                                {
                                    ERROR = "the top of the stack is not of type Type";
                                    t = null;
                                }
                                Var v;
                                if (Main.Peek() is Var)
                                    v = Main.Pop() as Var;
                                else
                                {
                                    ERROR = "the top of the stack is not of type Var";
                                    v = null;
                                }
                                Push(new Decl(v, t));
                                break;

                            case "(":
                                if (!CHAR)
                                    ERROR = "flag CHAR not lifted when expected";
                                BRACKET = true;
                                break;

                            case ")":
                                if (!CHAR)
                                    ERROR = "flag CHAR not lifted when expected";
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
            if (Main.Peek() is ABProgram)
                return Main.Pop();
            return null;
        }

        private Tuple<TokenType, string> NextToken()
        {
            TokenType t = TokenType.TUndefined;
            string s = "";
            if (pos >= input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            while (pos < input.Length && (input[pos] == ' ' || input[pos] == '\r' || input[pos] == '\n'))
                pos++;
            if (pos >= input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
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
            else if (pos + 4 < input.Length && input[pos] == 'c' && input[pos + 1] == 'l' && input[pos + 2] == 'e' && input[pos + 3] == 'a' && input[pos + 4] == 'r')
            {
                t = TokenType.Treserved;
                s = "clear";
                pos += 5;
            }
            else if (pos + 5 < input.Length && input[pos] == 'r' && input[pos + 1] == 'e' && input[pos + 2] == 't' && input[pos + 3] == 'u' && input[pos + 4] == 'r' && input[pos + 5] == 'n')
            {
                t = TokenType.Treserved;
                s = "return";
                pos += 6;
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
                    s += input[pos++];
            }
            else
            {
                t = TokenType.TVar;
                while (pos < input.Length && input[pos] != ' ' && input[pos] != '\r' && input[pos] != '\n')
                    s += input[pos++];
            }
            return new Tuple<TokenType, string>(t, s);
        }

    }
}
