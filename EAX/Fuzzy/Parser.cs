// Engage! generated this file, please do not edit manually
using EngageRuntime;
using System;
using System.Collections.Generic;

namespace EaxFuzzy
{
    public partial class Parser : BaseParser
    {
        private enum TokenType
        {
            TUndefined,
            TEOF,
            Tmark,
            Tword,
            TId,
        }

        private bool CLOSE, OPEN, PARSE, TAG;

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
                        switch (lexeme[0])
                        {
                            case 'a':
                                if (TAG && CLOSE)
                                {
                                    PARSE = false;
                                    TAG = false;
                                    CLOSE = false;
                                }
                                else if (TAG)
                                {
                                    PARSE = true;
                                    TAG = false;
                                    OPEN = false;
                                }
                                else
                                    ERROR = "neither of the flags TAG_CLOSE, TAG are lifted when expected";
                                break;
                        }
                        break;
                    case TokenType.Tmark:
                        switch (lexeme[0])
                        {
                            case '<':
                                TAG = true;
                                OPEN = true;
                                CLOSE = false;
                                break;
                            case '/':
                                if (TAG)
                                {
                                    CLOSE = true;
                                    OPEN = false;
                                }
                                else
                                    ERROR = "flag TAG is not lifted when expected";
                                break;
                            case '>':
                                if (PARSE && OPEN)
                                {
                                    Name n;
                                    if (Main.Peek() is Name)
                                        n = Main.Pop() as Name;
                                    else
                                    {
                                        ERROR = "the top of the stack is not of type Name";
                                        n = null;
                                    }
                                    Push(new TagOpen(n));
                                }
                                else if (PARSE && CLOSE)
                                {
                                    Name n;
                                    if (Main.Peek() is Name)
                                        n = Main.Pop() as Name;
                                    else
                                    {
                                        ERROR = "the top of the stack is not of type Name";
                                        n = null;
                                    }
                                    Push(new TagClose(n));
                                }
                                break;
                        }
                        break;
                    case TokenType.TEOF:
                        Flush();
                        var tags = new List<TagEvent>();
                        while (Main.Count > 0)
                            if (Main.Peek() is TagEvent)
                                tags.Add(Main.Pop() as TagEvent);
                            else
                                Main.Pop();
                        tags.Reverse();
                        Push(new EngagedXmlDoc(tags));
                        break;
                    case TokenType.TId:
                        Push(new Name(lexeme));
                        break;
                }
                if (!System.String.IsNullOrEmpty(ERROR))
                {
                    Console.WriteLine("Parser error: " + ERROR);
                    return null;
                }
            }
            while (type != TokenType.TEOF);
            if (Main.Peek() is EngagedXmlDoc)
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
            else if (Input[Pos] == '<')
            {
                t = TokenType.Tmark;
                s = "<";
                Pos++;
            }
            else if (Input[Pos] == '>')
            {
                t = TokenType.Tmark;
                s = ">";
                Pos++;
            }
            else if (Input[Pos] == '!')
            {
                t = TokenType.Tmark;
                s = "!";
                Pos++;
            }
            else if (Input[Pos] == '/')
            {
                t = TokenType.Tmark;
                s = "/";
                Pos++;
            }
            else if ((Input[Pos] == 'a') && (Pos + 1 == Input.Length || Input[Pos + 1] == ' ' || Input[Pos + 1] == '\r' || Input[Pos + 1] == '\n' || Input[Pos + 1] == '<' || Input[Pos + 1] == '>' || Input[Pos + 1] == '!' || Input[Pos + 1] == '/'))
            {
                t = TokenType.Tword;
                s = "a";
                Pos++;
            }
            else
            {
                t = TokenType.TId;
                while (Pos < Input.Length && Input[Pos] != ' ' && Input[Pos] != '\r' && Input[Pos] != '\n' && Input[Pos] != '<' && Input[Pos] != '>' && Input[Pos] != '!' && Input[Pos] != '/')
                    s += Input[Pos++];
            }
            return new Tuple<TokenType, string>(t, s);
        }

    }
}
