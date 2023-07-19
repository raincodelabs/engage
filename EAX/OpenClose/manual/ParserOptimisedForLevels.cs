using System;
using System.Collections.Generic;
using EngageRuntime;

namespace EaxOpenClose
{
    public class ParserOptimisedForLevels : BaseParser
    {
        private enum TokenType
        {
            TUndefined,
            TEOF,
            Tlt,
            Tgt,
            Tslash,
            TId,
        }

        private bool CLOSE, OPEN, TAG;

        public ParserOptimisedForLevels(string _input) : base(_input)
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
                    case TokenType.Tslash:
                        if (TAG)
                        {
                            CLOSE = true;
                            OPEN = false;
                        }
                        else
                            ERROR = "flag TAG is not lifted when expected";

                        break;
                    case TokenType.TEOF:
                        Flush();
                        var tags = new List<TagEvent>();
                        while (Main.Count > 0 && Main.Peek() is TagEvent)
                            tags.Add(Main.Pop() as TagEvent);
                        tags.Reverse();
                        Push(new EngagedXmlDoc(tags));
                        break;
                    case TokenType.Tlt:
                        TAG = true;
                        OPEN = true;
                        break;
                    case TokenType.TId:
                        Push(new Name(lexeme));
                        break;
                    case TokenType.Tgt:
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

                            Push(new TagOpen2(n.value));
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

                            Push(new TagClose2(n.value));
                        }

                        break;
                }

                if (!System.String.IsNullOrEmpty(ERROR))
                {
                    Console.WriteLine("Parser error: " + ERROR);
                    return null;
                }
            } while (type != TokenType.TEOF);

            if (Main.Peek() is EngagedXmlDoc)
                return Main.Pop();
            return null;
        }

        private Tuple<TokenType, string> NextToken()
        {
            TokenType t = TokenType.TUndefined;
            var s = String.Empty;
            if (Pos >= Input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            while (Pos < Input.Length && (Input[Pos] == ' ' || Input[Pos] == '\r' || Input[Pos] == '\n'))
                Pos++;
            if (Pos >= Input.Length)
                return new Tuple<TokenType, string>(TokenType.TEOF, "");
            if (Input[Pos] == '<')
            {
                t = TokenType.Tlt;
                Pos++;
            }
            else if (Input[Pos] == '>')
            {
                t = TokenType.Tgt;
                Pos++;
            }
            else if (Input[Pos] == '/')
            {
                t = TokenType.Tslash;
                Pos++;
            }
            else
            {
                t = TokenType.TId;
                var endPos = Pos;
                while (endPos < Input.Length && Input[endPos] != ' ' && Input[endPos] != '\r' &&
                       Input[endPos] != '\n' && Input[endPos] != '<' && Input[endPos] != '>' &&
                       Input[endPos] != '/')
                    endPos++;
                s = Input.Substring(Pos, endPos - Pos);
                Pos = endPos;
            }

            return new Tuple<TokenType, string>(t, s);
        }
    }
}