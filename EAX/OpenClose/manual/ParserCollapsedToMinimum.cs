using System;
using System.Collections.Generic;

namespace EaxOpenClose
{
    public class ParserCollapsedToMinimum //: BaseParser
    {
        private enum ParserState
        {
            SawStart,
            SawLess,
            SawSlash,
            SawSlashWillClose,
            SawTagName,
        }

        private readonly string _input;
        private ParserState _state;

        public ParserCollapsedToMinimum(string input)
        {
            _input = input;
            _state = ParserState.SawStart;
        }

        public EngagedXmlDoc Parse()
        {
            var events = new List<TagEvent>();
            int pos = 0, end = 0;
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
                        if (_input[pos] == '/')
                            _state = ParserState.SawSlash;
                        else
                        {
                            end = pos;
                            while (Char.IsLetter(_input[end++])) ;
                            if (end == pos)
                                _state = ParserState.SawStart;
                            else
                            {
                                name = _input.Substring(pos, end - pos - 1);
                                pos = end - 2;
                                _state = ParserState.SawTagName;
                            }
                        }

                        break;
                    case ParserState.SawSlash:
                        end = pos;
                        _state = ParserState.SawSlashWillClose;
                        break;
                    case ParserState.SawSlashWillClose:
                        if (_input[pos] == '>')
                        {
                            events.Add(new TagClose(new Name(_input.Substring(end, pos - end))));
                            _state = ParserState.SawStart;
                        }

                        break;
                    case ParserState.SawTagName:
                        if (_input[pos] == '>')
                        {
                            events.Add(new TagOpen(new Name(name)));
                            _state = ParserState.SawStart;
                        }

                        break;
                }

                pos++;
            }

            return new EngagedXmlDoc(events);
        }
    }
}