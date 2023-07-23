using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Engage.parsing;

// Notation-related syntax:
//      C = concrete (as parsed)
//      A = abstract (as processed)
// Generation-related syntax:
//      A = abstract (parser as produced)
//      C = concrete (parser code as serialised)
// Formal-related syntax:
//      C = concrete (specification as inferred)
namespace Engage;

public static class FrontEnd
{
    public static NC.EngSpec EngSpecFromText(string code)
    {
        ICharStream inputStream = CharStreams.fromString(code);
        EngageLexer lexer = new EngageLexer(inputStream);
        CommonTokenStream stream = new CommonTokenStream(lexer);
        EngageParser parser = new EngageParser(stream);
        EngageParser.EngSpecContext tree = parser.engSpec();
        // Console.WriteLine(tree.ToStringTree());
        EngageFullListener listener = new EngageFullListener();
        ParseTreeWalker walker = new ParseTreeWalker();
        walker.Walk(listener, tree);
        return listener.Root;
    }

    public static NC.EngSpec EngSpecFromFile(string filename)
        => EngSpecFromText(File.ReadAllText(filename));

    private static void FormalPipeline(NC.EngSpec eventSpec)
    {
        FC.Specification spec = eventSpec.Formalise();
        spec.PrintThis();

    }

    public static void FullPipeline(string inputFile, string outputFolder, bool verbose = true)
    {
        if (verbose)
            Console.WriteLine("Engage!");
        NC.EngSpec spec = EngSpecFromFile(inputFile);
        if (verbose)
            Console.WriteLine("NC-level spec read!");
        FormalPipeline(spec);
        NA.SystemPlan plan = spec.MakePlan();
        if (verbose)
            Console.WriteLine("NA-level plan made!");
        IEnumerable<GA.CsClass> data = plan.GenerateDataClasses();
        if (verbose)
            Console.WriteLine("GA-level abstract code for data classes generated!");
        GA.CsClass cp = plan.GenerateParser();
        if (verbose)
            Console.WriteLine("GA-level abstract code for the parser generated!");

        IEnumerable<GC.CsTop> css = data.Select(c => c.Concretise());
        if (verbose)
            Console.WriteLine("GC-level abstract code for data classes generated!");
        GC.CsClass dp = cp.Concretise() as GC.CsClass;
        if (verbose)
            Console.WriteLine("GC-level abstract code for the parser generated!");

        foreach (var csTop in css)
        {
            if (csTop is GC.CsClass cs)
                File.WriteAllLines(Path.Combine(outputFolder, "ast", $"{cs.Name}.cs"), cs.GenerateFileCode());
            else
                Console.WriteLine($"Unexpected type on the GC-level: {csTop.GetType().Name}");
        }

        if (dp == null)
            Console.WriteLine("GC-level class is empty, no code generated");
        else
        {
            File.WriteAllLines(Path.Combine(outputFolder, "Parser.cs"), dp.GenerateFileCode());
            if (verbose)
                Console.WriteLine("Final code generated and saved!");
        }
    }
}