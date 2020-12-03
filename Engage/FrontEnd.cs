using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engage.front
{
    public static class FrontEnd
    {
        public static A.EngSpec EngSpecFromText(string code)
        {
            EngageMetaParser parser = new EngageMetaParser();
            return parser.ParseGrammar(code);
        }

        public static A.EngSpec EngSpecFromFile(string filename)
            => EngSpecFromText(File.ReadAllText(filename));

        public static void FullPipeline(string inputFile, string outputFolder, bool verbose = true)
        {
            if (verbose)
                Console.WriteLine("Engage!");
            A.EngSpec spec = EngSpecFromFile(inputFile);
            if (verbose)
                Console.WriteLine("A-level spec read!");
            B.SystemPlan plan = spec.MakePlan();
            if (verbose)
                Console.WriteLine("B-level plan made!");
            IEnumerable<C.CsClass> data = plan.GenerateDataClasses();
            if (verbose)
                Console.WriteLine("C-level abstract code for data classes generated!");
            C.CsClass cp = plan.GenerateParser();
            if (verbose)
                Console.WriteLine("C-level abstract code for the parser generated!");

            IEnumerable<D.CsTop> css = data.Select(c => c.Concretize());
            if (verbose)
                Console.WriteLine("D-level abstract code for data classes generated!");
            D.CsClass dp = cp.Concretize() as D.CsClass;
            if (verbose)
                Console.WriteLine("D-level abstract code for the parser generated!");

            foreach (var csTop in css)
            {
                if (csTop is D.CsClass cs)
                    File.WriteAllLines(Path.Combine(outputFolder, "ast", $"{cs.Name}.cs"), cs.GenerateFileCode());
                else
                    Console.WriteLine($"Unexpected type on the D-level: {csTop.GetType().Name}");
            }

            if (dp == null)
                Console.WriteLine("D-level class is empty, no code generated");
            else
            {
                File.WriteAllLines(Path.Combine(outputFolder, "Parser.cs"), dp.GenerateFileCode());
                if (verbose)
                    Console.WriteLine("Final code generated and saved!");
            }
        }
    }
}