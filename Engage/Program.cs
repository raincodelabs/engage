using Engage.front;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engage
{
    internal class Program
    {
#if Windows
        private const string AppBuilderSpec = @"..\..\..\..\example\appbuilder.eng";
        private const string Output = @"..\..\..\..\AB";
#else
        private const string AppBuilderSpec = @"../../../../example/appbuilder.eng";
        private const string Output = @"../../../../AB";
#endif

        private static void Main(string[] args)
        {
            Console.WriteLine("Engage!");
            A.EngSpec spec = Parser.ParseEngSpec(File.ReadAllText(AppBuilderSpec));
            Console.WriteLine("A-level spec read!");
            B.SystemPlan plan = spec.MakePlan();
            Console.WriteLine("B-level plan made!");
            IEnumerable<C.CsClass> data = plan.GenerateDataClasses();
            Console.WriteLine("C-level abstract code for data classes generated!");
            C.CsClass cp = plan.GenerateParser();
            Console.WriteLine("C-level abstract code for the parser generated!");

            IEnumerable<D.CsTop> css = data.Select(c => c.Concretize());
            Console.WriteLine("D-level abstract code for data classes generated!");
            D.CsClass dp = cp.Concretize() as D.CsClass;
            Console.WriteLine("D-level abstract code for the parser generated!");

            foreach (D.CsClass cs in css)
                File.WriteAllLines(Path.Combine(Path.Combine(Output, "ast"), $"{cs.Name}.cs"), cs.GenerateFileCode());
            File.WriteAllLines(Path.Combine(Output, "Parser.cs"), dp.GenerateFileCode());
            Console.WriteLine("Final code generated and saved!");
        }
    }
}