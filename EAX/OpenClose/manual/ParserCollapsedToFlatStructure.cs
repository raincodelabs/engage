using System;
using System.Collections.Generic;

namespace EaxOpenClose
{
    public class ParserCollapsedToFlatStructure //: BaseParser
    {
        private enum ParserState
        {
            SawStart,
            SawLess,
            SawSlash,
            SawTagName,
        }

        private readonly string _input;
        private ParserState _state;

        public ParserCollapsedToFlatStructure(string input)
        {
            _input = input;
            _state = ParserState.SawStart;
        }

        public IEnumerable<string> Parse()
        {
            var result = new List<string>();

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
                        if (_input[pos] == '>')
                            _state = ParserState.SawStart;
                        break;
                    case ParserState.SawTagName:
                        if (_input[pos] == '>')
                        {
                            result.Add(name);
                            _state = ParserState.SawStart;
                        }

                        break;
                }

                pos++;
            }

            return result;
        }
    }
}