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
            Tmark,
            Tword,
            TNum,
            TId,
        }

        private bool DCL, BRACKET, CHAR, CONVERSE, HANDLER, IF, MAP;

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
                    case TokenType.Tword:
                        switch (lexeme)
                        {
                            case "converse":
                                CONVERSE = true;
                                Schedule(typeof(Var), _win =>
                                {
                                    var win = _win as Var;
                                    CONVERSE = false;
                                    Push(new ConverseStmt(win));
                                    return Message.Perfect;
                                }
                                );
                                break;
                            case "integer":
                                if (DCL)
                                    Push(new Integer());
                                else
                                    ERROR = "flag DCL is not lifted when expected";
                                break;
                            case "handler":
                                Schedule(typeof(Var), _obj =>
                                {
                                    var obj = _obj as Var;
                                    HANDLER = true;
                                    Schedule(typeof(Var), _proc =>
                                    {
                                        var proc = _proc as Var;
                                        HANDLER = false;
                                        if (!BRACKET)
                                            return Message.Misfire;
                                        Push(new HandlerStmt(obj, proc));
                                        return Message.Perfect;
                                    }
                                    );
                                    return Message.Perfect;
                                }
                                );
                                break;
                            case "overlay":
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
                                        Push(new OverlayStmt(source, target));
                                        return Message.Perfect;
                                    }
                                    );
                                    return Message.Perfect;
                                }
                                );
                                break;
                            case "enddcl":
                                DCL = false;
                                break;
                            case "return":
                                Push(new ReturnStmt());
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
                            case "endif":
                                Trim(typeof(Stmt));
                                break;
                            case "print":
                                Schedule(typeof(Expr), _message =>
                                {
                                    var message = _message as Expr;
                                    Push(new PrintStmt(message));
                                    return Message.Perfect;
                                }
                                );
                                break;
                            case "char":
                                if (DCL)
                                {
                                    CHAR = true;
                                    Schedule(typeof(Lit), _x =>
                                    {
                                        var x = _x as Lit;
                                        CHAR = false;
                                        if (!BRACKET)
                                            return Message.Misfire;
                                        Push(new String(x.value));
                                        return Message.Perfect;
                                    }
                                    );
                                }
                                else
                                    ERROR = "flag DCL is not lifted when expected";
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
                                IF = true;
                                Schedule(typeof(Expr), _cond =>
                                {
                                    var cond = _cond as Expr;
                                    IF = false;
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
                        }
                        break;
                    case TokenType.Tmark:
                        switch (lexeme[0])
                        {
                            case ';':
                                if (DCL)
                                {
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
                                }
                                else
                                    ERROR = "flag DCL is not lifted when expected";
                                break;
                            case '(':
                                if (CHAR)
                                    BRACKET = true;
                                else if (HANDLER)
                                    BRACKET = true;
                                else
                                    ERROR = "neither of the flags CHAR, HANDLER are lifted when expected";
                                break;
                            case ')':
                                BRACKET = false;
                                break;
                        }
                        break;
                    case TokenType.TEOF:
                        Flush();
                        var code = new List<Stmt>();
                        var data = new List<Decl>();
                        while (Main.Count > 0)
                            if (Main.Peek() is Stmt)
                                code.Add(Main.Pop() as Stmt);
                            else if (Main.Peek() is Decl)
                                data.Add(Main.Pop() as Decl);
                            else
                                break;
                        code.Reverse();
                        data.Reverse();
                        Push(new ABProgram(data, code));
                        break;
                    case TokenType.TNum:
                        Push(new Lit(System.Int32.Parse(lexeme)));
                        break;
                    case TokenType.TId:
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
            else if (input[pos] == ';')
            {
                t = TokenType.Tmark;
                s = ";";
                pos++;
            }
            else if (input[pos] == '(')
            {
                t = TokenType.Tmark;
                s = "(";
                pos++;
            }
            else if (input[pos] == ')')
            {
                t = TokenType.Tmark;
                s = ")";
                pos++;
            }
            else if ((pos + 2 < input.Length && input[pos] == 'd' && input[pos + 1] == 'c' && input[pos + 2] == 'l') && (pos + 3 == input.Length || input[pos + 3] == ' ' || input[pos + 3] == '\r' || input[pos + 3] == '\n' || input[pos + 3] == ';' || input[pos + 3] == '(' || input[pos + 3] == ')'))
            {
                t = TokenType.Tword;
                s = "dcl";
                pos += 3;
            }
            else if ((pos + 5 < input.Length && input[pos] == 'e' && input[pos + 1] == 'n' && input[pos + 2] == 'd' && input[pos + 3] == 'd' && input[pos + 4] == 'c' && input[pos + 5] == 'l') && (pos + 6 == input.Length || input[pos + 6] == ' ' || input[pos + 6] == '\r' || input[pos + 6] == '\n' || input[pos + 6] == ';' || input[pos + 6] == '(' || input[pos + 6] == ')'))
            {
                t = TokenType.Tword;
                s = "enddcl";
                pos += 6;
            }
            else if ((pos + 6 < input.Length && input[pos] == 'i' && input[pos + 1] == 'n' && input[pos + 2] == 't' && input[pos + 3] == 'e' && input[pos + 4] == 'g' && input[pos + 5] == 'e' && input[pos + 6] == 'r') && (pos + 7 == input.Length || input[pos + 7] == ' ' || input[pos + 7] == '\r' || input[pos + 7] == '\n' || input[pos + 7] == ';' || input[pos + 7] == '(' || input[pos + 7] == ')'))
            {
                t = TokenType.Tword;
                s = "integer";
                pos += 7;
            }
            else if ((pos + 3 < input.Length && input[pos] == 'c' && input[pos + 1] == 'h' && input[pos + 2] == 'a' && input[pos + 3] == 'r') && (pos + 4 == input.Length || input[pos + 4] == ' ' || input[pos + 4] == '\r' || input[pos + 4] == '\n' || input[pos + 4] == ';' || input[pos + 4] == '(' || input[pos + 4] == ')'))
            {
                t = TokenType.Tword;
                s = "char";
                pos += 4;
            }
            else if ((pos + 4 < input.Length && input[pos] == 'c' && input[pos + 1] == 'l' && input[pos + 2] == 'e' && input[pos + 3] == 'a' && input[pos + 4] == 'r') && (pos + 5 == input.Length || input[pos + 5] == ' ' || input[pos + 5] == '\r' || input[pos + 5] == '\n' || input[pos + 5] == ';' || input[pos + 5] == '(' || input[pos + 5] == ')'))
            {
                t = TokenType.Tword;
                s = "clear";
                pos += 5;
            }
            else if ((pos + 7 < input.Length && input[pos] == 'c' && input[pos + 1] == 'o' && input[pos + 2] == 'n' && input[pos + 3] == 'v' && input[pos + 4] == 'e' && input[pos + 5] == 'r' && input[pos + 6] == 's' && input[pos + 7] == 'e') && (pos + 8 == input.Length || input[pos + 8] == ' ' || input[pos + 8] == '\r' || input[pos + 8] == '\n' || input[pos + 8] == ';' || input[pos + 8] == '(' || input[pos + 8] == ')'))
            {
                t = TokenType.Tword;
                s = "converse";
                pos += 8;
            }
            else if ((pos + 6 < input.Length && input[pos] == 'h' && input[pos + 1] == 'a' && input[pos + 2] == 'n' && input[pos + 3] == 'd' && input[pos + 4] == 'l' && input[pos + 5] == 'e' && input[pos + 6] == 'r') && (pos + 7 == input.Length || input[pos + 7] == ' ' || input[pos + 7] == '\r' || input[pos + 7] == '\n' || input[pos + 7] == ';' || input[pos + 7] == '(' || input[pos + 7] == ')'))
            {
                t = TokenType.Tword;
                s = "handler";
                pos += 7;
            }
            else if ((pos + 1 < input.Length && input[pos] == 'i' && input[pos + 1] == 'f') && (pos + 2 == input.Length || input[pos + 2] == ' ' || input[pos + 2] == '\r' || input[pos + 2] == '\n' || input[pos + 2] == ';' || input[pos + 2] == '(' || input[pos + 2] == ')'))
            {
                t = TokenType.Tword;
                s = "if";
                pos += 2;
            }
            else if ((pos + 4 < input.Length && input[pos] == 'e' && input[pos + 1] == 'n' && input[pos + 2] == 'd' && input[pos + 3] == 'i' && input[pos + 4] == 'f') && (pos + 5 == input.Length || input[pos + 5] == ' ' || input[pos + 5] == '\r' || input[pos + 5] == '\n' || input[pos + 5] == ';' || input[pos + 5] == '(' || input[pos + 5] == ')'))
            {
                t = TokenType.Tword;
                s = "endif";
                pos += 5;
            }
            else if ((pos + 2 < input.Length && input[pos] == 'm' && input[pos + 1] == 'a' && input[pos + 2] == 'p') && (pos + 3 == input.Length || input[pos + 3] == ' ' || input[pos + 3] == '\r' || input[pos + 3] == '\n' || input[pos + 3] == ';' || input[pos + 3] == '(' || input[pos + 3] == ')'))
            {
                t = TokenType.Tword;
                s = "map";
                pos += 3;
            }
            else if ((pos + 1 < input.Length && input[pos] == 't' && input[pos + 1] == 'o') && (pos + 2 == input.Length || input[pos + 2] == ' ' || input[pos + 2] == '\r' || input[pos + 2] == '\n' || input[pos + 2] == ';' || input[pos + 2] == '(' || input[pos + 2] == ')'))
            {
                t = TokenType.Tword;
                s = "to";
                pos += 2;
            }
            else if ((pos + 6 < input.Length && input[pos] == 'o' && input[pos + 1] == 'v' && input[pos + 2] == 'e' && input[pos + 3] == 'r' && input[pos + 4] == 'l' && input[pos + 5] == 'a' && input[pos + 6] == 'y') && (pos + 7 == input.Length || input[pos + 7] == ' ' || input[pos + 7] == '\r' || input[pos + 7] == '\n' || input[pos + 7] == ';' || input[pos + 7] == '(' || input[pos + 7] == ')'))
            {
                t = TokenType.Tword;
                s = "overlay";
                pos += 7;
            }
            else if ((pos + 4 < input.Length && input[pos] == 'p' && input[pos + 1] == 'r' && input[pos + 2] == 'i' && input[pos + 3] == 'n' && input[pos + 4] == 't') && (pos + 5 == input.Length || input[pos + 5] == ' ' || input[pos + 5] == '\r' || input[pos + 5] == '\n' || input[pos + 5] == ';' || input[pos + 5] == '(' || input[pos + 5] == ')'))
            {
                t = TokenType.Tword;
                s = "print";
                pos += 5;
            }
            else if ((pos + 5 < input.Length && input[pos] == 'r' && input[pos + 1] == 'e' && input[pos + 2] == 't' && input[pos + 3] == 'u' && input[pos + 4] == 'r' && input[pos + 5] == 'n') && (pos + 6 == input.Length || input[pos + 6] == ' ' || input[pos + 6] == '\r' || input[pos + 6] == '\n' || input[pos + 6] == ';' || input[pos + 6] == '(' || input[pos + 6] == ')'))
            {
                t = TokenType.Tword;
                s = "return";
                pos += 6;
            }
            else if (input[pos] == '0' || input[pos] == '1' || input[pos] == '2' || input[pos] == '3' || input[pos] == '4' || input[pos] == '5' || input[pos] == '6' || input[pos] == '7' || input[pos] == '8' || input[pos] == '9')
            {
                t = TokenType.TNum;
                while (pos < input.Length && (input[pos] == '0' || input[pos] == '1' || input[pos] == '2' || input[pos] == '3' || input[pos] == '4' || input[pos] == '5' || input[pos] == '6' || input[pos] == '7' || input[pos] == '8' || input[pos] == '9'))
                    s += input[pos++];
            }
            else
            {
                t = TokenType.TId;
                while (pos < input.Length && input[pos] != ' ' && input[pos] != '\r' && input[pos] != '\n')
                    s += input[pos++];
            }
            return new Tuple<TokenType, string>(t, s);
        }

    }
}
