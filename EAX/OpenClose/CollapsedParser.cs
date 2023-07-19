using System;
using System.Collections.Generic;

namespace EaxOpenClose
{
    public class CollapsedParser //: BaseParser
    {
        private enum ParserState
        {
            SawStart,
            SawLess,
            SawTagName,
        }

        private readonly string _input;
        private ParserState _state;

        public CollapsedParser(string input)
        {
            _input = input;
            _state = ParserState.SawStart;
        }

        public EngagedXmlDoc Parse()
        {
            var events = new List<TagEvent>();
            int pos = 0, end;
            string name = "";

            while (pos < _input.Length)
            {
                switch (_state)
                {
                    case ParserState.SawStart:
                        if (_input[pos] == '<')
                            _state = ParserState.SawLess;
                        break;
                    case ParserState.SawLess:
                        end = pos;
                        while (Char.IsLetter(_input[end++])) ;
                        if (end == pos)
                            _state = ParserState.SawStart;
                        else
                        {
                            name = _input.Substring(pos, end - pos);
                            pos = end;
                            _state = ParserState.SawTagName;
                        }

                        break;
                    case ParserState.SawTagName:
                        switch (_input[pos])
                        {
                            case '>':
                                events.Add(new TagOpen(new Name(name)));
                                _state = ParserState.SawStart;
                                break;
                            case '/' when pos + 1 < _input.Length && _input[pos + 1] == '>':
                                pos++;
                                events.Add(new TagClose(new Name(name)));
                                _state = ParserState.SawStart;
                                break;
                        }

                        break;
                }

                pos++;
            }

            return new EngagedXmlDoc(events);
        }
    }
}