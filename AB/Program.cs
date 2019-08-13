using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AB
{
    internal class Program
    {
        private const string TestPath = @"..\..\..\..\tests\";

        private static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            var gen = new TestGenerator();
            sw.Start();
            // long test cases: programs with N statements, each statement random
            Generate(gen.GenerateFlatVersatile, "long", 101, 8);
            // deep test cases: programs with N nested IF statements, with a single RETURN inside
            Generate(gen.GenerateNestedIfs, "deep", 101, 5);
            // stacked test cases- programs with:
            //      N statements before the first IF
            //      N statements after the first IF but before the second IF
            //      N statements inside the second IF
            //      N statements after the end of the second IF but before the end of the first IF
            //      N statements after the end of the first IF
            Generate(gen.GenerateStackedIfs, "stack", 101, 7);
            sw.Stop();
            Console.WriteLine($"Test generation took {sw.ElapsedMilliseconds} ms");
        }

        private static void Generate(Func<ulong, List<string>> gen, string name, uint limit1, uint limit2)
        {
            for (uint i = 0; i < limit1; i++)
                File.WriteAllLines(Path.Combine(TestPath, $"{name}{i}.ab"), gen(i));
            Console.WriteLine($"{limit1} test programs generated");
            for (uint i = 0; i < limit2; i++)
                File.WriteAllLines(Path.Combine(TestPath, $"{name}10e{i}.ab"), gen((ulong)Math.Pow(10, i)));
            Console.WriteLine($"{limit2} test programs generated");
        }
    }
}