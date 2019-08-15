using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AB
{
    internal class Program
    {
        private const string TestPath = @"..\..\..\..\tests\";

        private const int LimitNormal = 1001;
        private const int LimitLongTests = LimitNormal;
        private const int LimitLongExpTests = 8;
        private const int LimitDeepTests = LimitNormal;
        private const int LimitDeepExpTests = 5;
        private const int LimitStackTests = LimitNormal;
        private const int LimitStackExpTests = 7;
        private const int LimitDeclTests = LimitNormal;
        private const int LimitDeclExpTests = 8;
        private const int LimitMixedTestsEach = 101;
        private const int LimitMixedTestsRep = 101;

        private static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            var gen = new TestGenerator();
            sw.Start();
            // long test cases: programs with N statements, each statement random
            Generate(gen.GenerateFlatVersatile, "long", LimitLongTests, LimitLongExpTests);
            // deep test cases: programs with N nested IF statements, with a single RETURN inside
            Generate(gen.GenerateNestedIfs, "deep", LimitDeepTests, LimitDeepExpTests);
            // stacked test cases- programs with:
            //      N statements before the first IF
            //      N statements after the first IF but before the second IF
            //      N statements inside the second IF
            //      N statements after the end of the second IF but before the end of the first IF
            //      N statements after the end of the first IF
            Generate(gen.GenerateStackedIfs, "stack", LimitStackTests, LimitStackExpTests);
            // sequential decl + code
            Generate(gen.GenerateDeclThenCode, "dcl", LimitDeclTests, LimitDeclExpTests);
            // mixed decl + code
            Generate2D(gen.GenerateMixedDeclCode, "mix", LimitMixedTestsEach, LimitMixedTestsRep);
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

        private static void Generate2D(Func<ulong, ulong, List<string>> gen, string name, uint limit1, uint limit2)
        {
            for (uint i = 0; i < limit1; i++)
                for (uint j = 0; j < limit2; j++)
                    File.WriteAllLines(Path.Combine(TestPath, $"{name}{i}x{j}.ab"), gen(i, j));
            Console.WriteLine($"{limit1 * limit2} test programs generated");
        }
    }
}