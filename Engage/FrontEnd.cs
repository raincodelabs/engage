﻿using System;
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
//      A = abstract (state machine)
namespace Engage;

public static class FrontEnd
{
    public static NC.EngSpec TextToNotationConcrete(string code)
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

    public static NC.EngSpec FileToNotationConcrete(string filename)
        => TextToNotationConcrete(File.ReadAllText(filename));

    public static FC.Specification TextToFormalConcrete(string specText, bool verbose = true)
    {
        NC.EngSpec specNC = TextToNotationConcrete(specText);
        FC.Specification specFC = specNC.Formalise(verbose);
        return specFC;
    }

    public static FA.StateMachine TextToFormalAbstract(string specText, bool verbose = true)
    {
        FC.Specification specFC = TextToFormalConcrete(specText, verbose);
        return new FA.StateMachine(specFC);
    }

    private static string NotationConcreteToDot(NC.EngSpec eventSpec, bool verbose = true)
    {
        if (verbose)
            Console.WriteLine("Engage the formalities!");
        FC.Specification spec = eventSpec.Formalise(verbose);
        if (verbose)
            Console.WriteLine("FC-level spec created from the NC-level spec!");
        FA.StateMachine sm = new(spec);
        if (verbose)
            Console.WriteLine("FA-level machine inferred!");
        return sm.ToDot();
    }

    public static void FullPipeline(string inputFile, string outputFolder, bool verbose = true)
    {
        if (verbose)
            Console.WriteLine("Engage!");
        NC.EngSpec spec = FileToNotationConcrete(inputFile);
        if (verbose)
            Console.WriteLine("NC-level spec read!");

        var dot = NotationConcreteToDot(spec);
        File.WriteAllText("machine.dot", dot);
        if (verbose)
            Console.WriteLine("Graphviz file written!");

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