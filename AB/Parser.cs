// Engage! generated this file, please do not edit manually
using EngageRuntime;
using System;
using System.Collections.Generic;

namespace AB
{
    public partial class Parser : BaseParser
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

        private bool BRACKET, CHAR, CONVERSE, DCL, DEC, HANDLER, IF, MAP, OVERLAY;

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
                                OVERLAY = true;
                                Schedule(typeof(Expr), _source =>
                                {
                                    var source = _source as Expr;
                                    OVERLAY = false;
                                    OVERLAY = true;
                                    Schedule(typeof(Var), _target =>
                                    {
                                        var target = _target as Var;
                                        OVERLAY = false;
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
                            case "dec":
                                if (DCL)
                                {
                                    DEC = true;
                                    Schedule(typeof(Lit), _x =>
                                    {
                                        var x = _x as Lit;
                                        DEC = false;
                                        if (!BRACKET)
                                            return Message.Misfire;
                                        Push(new Decimal(x.value));
                                        return Message.Perfect;
                                    }
                                    );
                                }
                                else
                                    ERROR = "flag DCL is not lifted when expected";
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
                                else if (DEC)
                                    BRACKET = true;
                                else if (HANDLER)
                                    BRACKET = true;
                                else
                                    ERROR = "neither of the flags CHAR, DEC, HANDLER are lifted when expected";
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
            if (Pos >= Input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            while (Pos < Input.Length && (Input[Pos] == ' ' || Input[Pos] == '\r' || Input[Pos] == '\n'))
                Pos++;
            if (Pos >= Input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            else if (Input[Pos] == ';')
            {
                t = TokenType.Tmark;
                s = ";";
                Pos++;
            }
            else if (Input[Pos] == '(')
            {
                t = TokenType.Tmark;
                s = "(";
                Pos++;
            }
            else if (Input[Pos] == ')')
            {
                t = TokenType.Tmark;
                s = ")";
                Pos++;
            }
            else if ((Pos + 2 < Input.Length && Input[Pos] == 'd' && Input[Pos + 1] == 'c' && Input[Pos + 2] == 'l') && (Pos + 3 == Input.Length || Input[Pos + 3] == ' ' || Input[Pos + 3] == '\r' || Input[Pos + 3] == '\n' || Input[Pos + 3] == ';' || Input[Pos + 3] == '(' || Input[Pos + 3] == ')'))
            {
                t = TokenType.Tword;
                s = "dcl";
                Pos += 3;
            }
            else if ((Pos + 5 < Input.Length && Input[Pos] == 'e' && Input[Pos + 1] == 'n' && Input[Pos + 2] == 'd' && Input[Pos + 3] == 'd' && Input[Pos + 4] == 'c' && Input[Pos + 5] == 'l') && (Pos + 6 == Input.Length || Input[Pos + 6] == ' ' || Input[Pos + 6] == '\r' || Input[Pos + 6] == '\n' || Input[Pos + 6] == ';' || Input[Pos + 6] == '(' || Input[Pos + 6] == ')'))
            {
                t = TokenType.Tword;
                s = "enddcl";
                Pos += 6;
            }
            else if ((Pos + 6 < Input.Length && Input[Pos] == 'i' && Input[Pos + 1] == 'n' && Input[Pos + 2] == 't' && Input[Pos + 3] == 'e' && Input[Pos + 4] == 'g' && Input[Pos + 5] == 'e' && Input[Pos + 6] == 'r') && (Pos + 7 == Input.Length || Input[Pos + 7] == ' ' || Input[Pos + 7] == '\r' || Input[Pos + 7] == '\n' || Input[Pos + 7] == ';' || Input[Pos + 7] == '(' || Input[Pos + 7] == ')'))
            {
                t = TokenType.Tword;
                s = "integer";
                Pos += 7;
            }
            else if ((Pos + 3 < Input.Length && Input[Pos] == 'c' && Input[Pos + 1] == 'h' && Input[Pos + 2] == 'a' && Input[Pos + 3] == 'r') && (Pos + 4 == Input.Length || Input[Pos + 4] == ' ' || Input[Pos + 4] == '\r' || Input[Pos + 4] == '\n' || Input[Pos + 4] == ';' || Input[Pos + 4] == '(' || Input[Pos + 4] == ')'))
            {
                t = TokenType.Tword;
                s = "char";
                Pos += 4;
            }
            else if ((Pos + 2 < Input.Length && Input[Pos] == 'd' && Input[Pos + 1] == 'e' && Input[Pos + 2] == 'c') && (Pos + 3 == Input.Length || Input[Pos + 3] == ' ' || Input[Pos + 3] == '\r' || Input[Pos + 3] == '\n' || Input[Pos + 3] == ';' || Input[Pos + 3] == '(' || Input[Pos + 3] == ')'))
            {
                t = TokenType.Tword;
                s = "dec";
                Pos += 3;
            }
            else if ((Pos + 4 < Input.Length && Input[Pos] == 'c' && Input[Pos + 1] == 'l' && Input[Pos + 2] == 'e' && Input[Pos + 3] == 'a' && Input[Pos + 4] == 'r') && (Pos + 5 == Input.Length || Input[Pos + 5] == ' ' || Input[Pos + 5] == '\r' || Input[Pos + 5] == '\n' || Input[Pos + 5] == ';' || Input[Pos + 5] == '(' || Input[Pos + 5] == ')'))
            {
                t = TokenType.Tword;
                s = "clear";
                Pos += 5;
            }
            else if ((Pos + 7 < Input.Length && Input[Pos] == 'c' && Input[Pos + 1] == 'o' && Input[Pos + 2] == 'n' && Input[Pos + 3] == 'v' && Input[Pos + 4] == 'e' && Input[Pos + 5] == 'r' && Input[Pos + 6] == 's' && Input[Pos + 7] == 'e') && (Pos + 8 == Input.Length || Input[Pos + 8] == ' ' || Input[Pos + 8] == '\r' || Input[Pos + 8] == '\n' || Input[Pos + 8] == ';' || Input[Pos + 8] == '(' || Input[Pos + 8] == ')'))
            {
                t = TokenType.Tword;
                s = "converse";
                Pos += 8;
            }
            else if ((Pos + 6 < Input.Length && Input[Pos] == 'h' && Input[Pos + 1] == 'a' && Input[Pos + 2] == 'n' && Input[Pos + 3] == 'd' && Input[Pos + 4] == 'l' && Input[Pos + 5] == 'e' && Input[Pos + 6] == 'r') && (Pos + 7 == Input.Length || Input[Pos + 7] == ' ' || Input[Pos + 7] == '\r' || Input[Pos + 7] == '\n' || Input[Pos + 7] == ';' || Input[Pos + 7] == '(' || Input[Pos + 7] == ')'))
            {
                t = TokenType.Tword;
                s = "handler";
                Pos += 7;
            }
            else if ((Pos + 1 < Input.Length && Input[Pos] == 'i' && Input[Pos + 1] == 'f') && (Pos + 2 == Input.Length || Input[Pos + 2] == ' ' || Input[Pos + 2] == '\r' || Input[Pos + 2] == '\n' || Input[Pos + 2] == ';' || Input[Pos + 2] == '(' || Input[Pos + 2] == ')'))
            {
                t = TokenType.Tword;
                s = "if";
                Pos += 2;
            }
            else if ((Pos + 4 < Input.Length && Input[Pos] == 'e' && Input[Pos + 1] == 'n' && Input[Pos + 2] == 'd' && Input[Pos + 3] == 'i' && Input[Pos + 4] == 'f') && (Pos + 5 == Input.Length || Input[Pos + 5] == ' ' || Input[Pos + 5] == '\r' || Input[Pos + 5] == '\n' || Input[Pos + 5] == ';' || Input[Pos + 5] == '(' || Input[Pos + 5] == ')'))
            {
                t = TokenType.Tword;
                s = "endif";
                Pos += 5;
            }
            else if ((Pos + 2 < Input.Length && Input[Pos] == 'm' && Input[Pos + 1] == 'a' && Input[Pos + 2] == 'p') && (Pos + 3 == Input.Length || Input[Pos + 3] == ' ' || Input[Pos + 3] == '\r' || Input[Pos + 3] == '\n' || Input[Pos + 3] == ';' || Input[Pos + 3] == '(' || Input[Pos + 3] == ')'))
            {
                t = TokenType.Tword;
                s = "map";
                Pos += 3;
            }
            else if ((Pos + 1 < Input.Length && Input[Pos] == 't' && Input[Pos + 1] == 'o') && (Pos + 2 == Input.Length || Input[Pos + 2] == ' ' || Input[Pos + 2] == '\r' || Input[Pos + 2] == '\n' || Input[Pos + 2] == ';' || Input[Pos + 2] == '(' || Input[Pos + 2] == ')'))
            {
                t = TokenType.Tword;
                s = "to";
                Pos += 2;
            }
            else if ((Pos + 6 < Input.Length && Input[Pos] == 'o' && Input[Pos + 1] == 'v' && Input[Pos + 2] == 'e' && Input[Pos + 3] == 'r' && Input[Pos + 4] == 'l' && Input[Pos + 5] == 'a' && Input[Pos + 6] == 'y') && (Pos + 7 == Input.Length || Input[Pos + 7] == ' ' || Input[Pos + 7] == '\r' || Input[Pos + 7] == '\n' || Input[Pos + 7] == ';' || Input[Pos + 7] == '(' || Input[Pos + 7] == ')'))
            {
                t = TokenType.Tword;
                s = "overlay";
                Pos += 7;
            }
            else if ((Pos + 4 < Input.Length && Input[Pos] == 'p' && Input[Pos + 1] == 'r' && Input[Pos + 2] == 'i' && Input[Pos + 3] == 'n' && Input[Pos + 4] == 't') && (Pos + 5 == Input.Length || Input[Pos + 5] == ' ' || Input[Pos + 5] == '\r' || Input[Pos + 5] == '\n' || Input[Pos + 5] == ';' || Input[Pos + 5] == '(' || Input[Pos + 5] == ')'))
            {
                t = TokenType.Tword;
                s = "print";
                Pos += 5;
            }
            else if ((Pos + 5 < Input.Length && Input[Pos] == 'r' && Input[Pos + 1] == 'e' && Input[Pos + 2] == 't' && Input[Pos + 3] == 'u' && Input[Pos + 4] == 'r' && Input[Pos + 5] == 'n') && (Pos + 6 == Input.Length || Input[Pos + 6] == ' ' || Input[Pos + 6] == '\r' || Input[Pos + 6] == '\n' || Input[Pos + 6] == ';' || Input[Pos + 6] == '(' || Input[Pos + 6] == ')'))
            {
                t = TokenType.Tword;
                s = "return";
                Pos += 6;
            }
            else if (Input[Pos] == '0' || Input[Pos] == '1' || Input[Pos] == '2' || Input[Pos] == '3' || Input[Pos] == '4' || Input[Pos] == '5' || Input[Pos] == '6' || Input[Pos] == '7' || Input[Pos] == '8' || Input[Pos] == '9')
            {
                t = TokenType.TNum;
                while (Pos < Input.Length && (Input[Pos] == '0' || Input[Pos] == '1' || Input[Pos] == '2' || Input[Pos] == '3' || Input[Pos] == '4' || Input[Pos] == '5' || Input[Pos] == '6' || Input[Pos] == '7' || Input[Pos] == '8' || Input[Pos] == '9'))
                    s += Input[Pos++];
            }
            else
            {
                t = TokenType.TId;
                while (Pos < Input.Length && Input[Pos] != ' ' && Input[Pos] != '\r' && Input[Pos] != '\n' && Input[Pos] != ';' && Input[Pos] != '(' && Input[Pos] != ')')
                    s += Input[Pos++];
            }
            return new Tuple<TokenType, string>(t, s);
        }

    }
}
