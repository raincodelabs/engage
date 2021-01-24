﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Engage.parsing;

namespace Engage
{
    public static class FrontEnd
    {
        public static A.EngSpec EngSpecFromText(string code)
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

        /// <summary>
        ///     This method doesn't do anything useful any more: it used to call the old parser
        ///     (the second one, from Takmela), but now calls the new one (the third one)
        ///     because the first (PEG) and the second are removed from the codebase.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static A.EngSpec LegacyEngSpecFromText(string code)
        {
            //EngageMetaParser parser = new EngageMetaParser();
            //return parser.ParseGrammar(code);
            return EngSpecFromText(code);
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