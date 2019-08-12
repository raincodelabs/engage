using System;
using System.IO;

namespace AB
{
    internal class Program
    {
        private const string TestPath = @"..\..\..\..\tests\";

        private static void Main(string[] args)
        {
            var gen = new TestGenerator();
            for (uint i = 0; i < 101; i++)
                File.WriteAllLines(Path.Combine(TestPath, $"deep{i}.ab"), gen.GenerateNestedIfs(i));
            Console.WriteLine("100 test programs generated");
            for (uint i = 0; i < 5; i++)
                File.WriteAllLines(Path.Combine(TestPath, $"deep10e{i}.ab"), gen.GenerateNestedIfs((ulong)Math.Pow(10, i)));
            Console.WriteLine("5 more test programs generated");
            for (uint i = 0; i < 101; i++)
                File.WriteAllLines(Path.Combine(TestPath, $"long{i}.ab"), gen.GenerateFlatVersatile(i));
            Console.WriteLine("100 more test programs generated");
            for (uint i = 0; i < 8; i++)
                File.WriteAllLines(Path.Combine(TestPath, $"long10e{i}.ab"), gen.GenerateFlatVersatile((ulong)Math.Pow(10, i)));
            Console.WriteLine("8 more test programs generated");
        }
    }
}