using System;
using System.Collections.Generic;

namespace EaxOpenClose
{
    public class NonParser
    {
        private readonly string _input;

        public NonParser(string input)
        {
            _input = input;
        }

        public HashSet<string> Parse()
        {
            var result = new HashSet<string>();

            int pos = 0, end;

            while (pos < _input.Length)
            {
                if (_input[pos] == '<')
                    end = pos;
                pos++;
            }

            return result;
        }
    }
}