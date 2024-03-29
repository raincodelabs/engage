using EngageRuntime;
using System;
using System.Collections.Generic;

namespace EaxOpenClose
{
    public class ParserOptimisedForStrings : BaseParser
    {
        private enum TokenType
        {
            TUndefined,
            TEOF,
            Tmark,
            TId,
        }

        private bool CLOSE, OPEN, TAG;

        public ParserOptimisedForStrings(string _input) : base(_input)
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
                    case TokenType.Tmark:
                        switch (lexeme[0])
                        {
                            case '<':
                                TAG = true;
                                OPEN = true;
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
                                TAG = false;
                                if (OPEN)
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
                                else if (CLOSE)
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
                        while (Main.Count > 0 && Main.Peek() is TagEvent)
                            tags.Add(Main.Pop() as TagEvent);
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
                return new Tuple<TokenType, string>(t, s);
            }
            else if (Input[Pos] == '>')
            {
                t = TokenType.Tmark;
                s = ">";
                Pos++;
                return new Tuple<TokenType, string>(t, s);
            }
            else if (Input[Pos] == '/')
            {
                t = TokenType.Tmark;
                s = "/";
                Pos++;
                return new Tuple<TokenType, string>(t, s);
            }
            else
            {
                t = TokenType.TId;
                var endPos = Pos;
                while (endPos < Input.Length && Input[endPos] != ' ' && Input[endPos] != '\r' &&
                       Input[endPos] != '\n' && Input[endPos] != '<' && Input[endPos] != '>' &&
                       Input[endPos] != '/')
                    endPos++;
                var sResult = Input.Substring(Pos, endPos - Pos);
                Pos = endPos;
                return new Tuple<TokenType, string>(t, sResult);
            }
            return new Tuple<TokenType, string>(t, s);
        }

    }
}
