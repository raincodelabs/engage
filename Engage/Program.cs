using Engage.front;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engage
{
    internal class Program
    {
        private const string AppBuilderSpec = @"..\..\..\..\example\appbuilder.eng";
        private const string Output = @"..\..\..\..\AB";

        private static void Main(string[] args)
        {
            Console.WriteLine("Engage!");
            A.EngSpec spec = Parser.ParseEngSpec(File.ReadAllText(AppBuilderSpec));
            Console.WriteLine("A-level spec read!");
            B.SystemPlan plan = IntermediateFactory.Ast2ir(spec);
            Console.WriteLine("B-level plan made!");
            IEnumerable<C.CsClass> css = plan.GenerateDataClasses();
            Console.WriteLine("C-level abstract code for data classes generated!");
            C.CsClass p = plan.GenerateParser();
            Console.WriteLine("C-level abstract code for the parser generated!");
            foreach (C.CsClass cs in css)
                File.WriteAllLines(Path.Combine(Output, $"ast\\{cs.Name}.cs"), cs.GenerateFileCode());
            Console.WriteLine("Concrete code generated and saved!");
            File.WriteAllLines(Path.Combine(Output, "Parser.cs"), p.GenerateFileCode());
            Console.WriteLine("Parser generated and saved!");
        }
    }
}