using Engage.front;
using System;
using System.IO;

namespace Engage
{
    internal class Program
    {
        private const string AppBuilderSpec = @"..\..\..\..\test\appbuilder.eng";
        private const string Output = @"..\..\..\..\AB";

        private static void Main(string[] args)
        {
            Console.WriteLine("Engage!");
            var spec = Parser.ParseEngSpec(File.ReadAllText(AppBuilderSpec));
            Console.WriteLine("Spec read!");
            var plan = IntermediateFactory.Ast2ir(spec);
            Console.WriteLine("Plan made!");
            var css = plan.GenerateDataClasses();
            Console.WriteLine("Abstract code generated!");
            foreach (var cs in css)
                File.WriteAllLines(Path.Combine(Output, $"ast\\{cs.Name}.cs"), cs.GenerateFileCode());
            Console.WriteLine("Concrete code generated!");
            var p = plan.GenerateParser();
            File.WriteAllLines(Path.Combine(Output, "Parser.cs"), p.GenerateFileCode());
            Console.WriteLine("Parser generated!");
        }
    }
}