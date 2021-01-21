using System;
using System.Text;

namespace EngageTests
{
    internal static class Generator
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly Random _random = new Random();

        internal static bool ArbitraryBoolean()
            => _random.NextDouble() < .5;

        internal static int ArbitraryNumber(int min = 0, int max = 10)
            => _random.Next(min, max);

        internal static char ArbitraryLetter()
            => Alphabet[_random.Next(Alphabet.Length)];

        internal static string ArbitraryWord()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < ArbitraryNumber(min: 1); i++)
                sb.Append(ArbitraryLetter());
            return sb.ToString();
        }

        internal static string ArbitrarySequence(int limit =-1)
        {
            if (limit == -1)
                limit = ArbitraryNumber(max: 100);
            var sb = new StringBuilder();
            for (var i = 0; i < limit; i++)
                sb.Append($"<{(ArbitraryBoolean() ? "/" : "")}{ArbitraryWord()}>");
            return sb.ToString();
        }

        /**
         * Generates a shallow balanced input, by using the recursive function below and adding one root element.
         */
        internal static string ArbitraryBalancedSequenceShallow(int limit = 10)
        {
            return $"<start>{ArbitraryBalancedSequenceShallowRec(limit)}</start>";
        }
        
        /**
         * Generates a shallow balanced input
         */
        internal static string ArbitraryBalancedSequenceShallowRec(int limit = 10)
        {
            //using limit < 2 because a root element is added before this function is called.
            if (limit < 2) 
                return "";
            var name = ArbitraryWord();
            return $"<{name}></{name}>{ArbitraryBalancedSequenceShallowRec(limit - 1)}";

        }
        
        /**
         * Generates a deep balanced input
         */
        internal static string ArbitraryBalancedSequenceDeep(int limit = 10)
        {
            if (limit < 0)
                return "";
            var name = ArbitraryWord();
            return $"<{name}>{ArbitraryBalancedSequenceDeep(limit - 1)}</{name}>";
        }
    }
}