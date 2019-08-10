using Engage.front;
using Engage.mid;
using System;
using System.IO;

namespace Engage
{
    internal class Program
    {
        private const string AppBuilderSpec = @"..\..\..\..\test\appbuilder.eng";

        private static void Main(string[] args)
        {
            Console.WriteLine("Engage!");
            var spec = Parser.ParseEngSpec(File.ReadAllText(AppBuilderSpec));
            Console.WriteLine("Spec read!");
            var plan = IntermediateFactory.Ast2ir(spec);
            Console.WriteLine("Plan made!");
        }
    }
}