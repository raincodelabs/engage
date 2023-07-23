using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engage.NC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngageTests
{
    [TestClass]
    public class Parsing
    {
        private static string FourUp { get; } = Path.Combine("..", "..", "..", "..");

        private static List<string> KnownGrammars = Runner.Program.CompilationList.Keys.ToList();

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserSimple()
        {
            string input = "namespace abc " +
                           "types X; Y <: Z; " +
                           "tokens 'x'::skip string :: Id " +
                           "handlers EOF -> lift TAG";
            EngSpec result = Engage.FrontEnd.EngSpecFromText(input);
            Assert.IsNotNull(result);
            Assert.AreEqual("abc", result.NS);

            Assert.AreEqual(2, result.Types.Count);
            Assert.AreEqual(1, result.Types[0].Names.Count);
            Assert.AreEqual("X", result.Types[0].Names[0]);
            Assert.IsTrue(String.IsNullOrEmpty(result.Types[0].Super));
            Assert.AreEqual(1, result.Types[1].Names.Count);
            Assert.AreEqual("Y", result.Types[1].Names[0]);
            Assert.AreEqual("Z", result.Types[1].Super);

            Assert.AreEqual(2, result.Tokens.Count);
            var t1 = result.Tokens[0];
            Assert.IsNotNull(t1);
            var t2 = result.Tokens[1];
            Assert.IsNotNull(t2);
            Assert.AreEqual(1, t1.Names.Count);
            Assert.IsFalse(t1.Names[0].Special);
            var t1c = t1.Names[0] as LiteralLex;
            Assert.IsNotNull(t1c);
            Assert.AreEqual("x", t1c.Literal);
            Assert.AreEqual("skip", t1.Type);
            var t2c = t2.Names[0] as StringLex;
            Assert.IsNotNull(t2c);
            Assert.IsTrue(t2.Names[0].Special);
            Assert.AreEqual("Id", t2.Type);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserAwait()
        {
            string input = "namespace X " +
                           "types tokens handlers " +
                           "EOF -> push String(n) " +
                           "where x := await (Lit upon BRACKET) with CHAR, n := tear x";
            EngSpec result = Engage.FrontEnd.EngSpecFromText(input);
            Assert.IsNotNull(result);
            Assert.AreEqual("X", result.NS);

            Assert.AreEqual(0, result.Types.Count);
            Assert.AreEqual(0, result.Tokens.Count);
            Assert.AreEqual(1, result.Handlers.Count);
            // There was a specific bug
            Assert.AreEqual("Lit", result.Handlers[0].Context[0].RHS.Name);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserAllFiles()
        {
            TraverseDir(FourUp);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserOnAppbuilder()
        {
            ProcessFile(KnownGrammars[0]);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserOnOpenClose()
        {
            ProcessFile(KnownGrammars[1]);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserOnFuzzy()
        {
            ProcessFile(KnownGrammars[2]);
        }

        private void TraverseDir(string folder)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(folder))
                {
                    //Console.WriteLine("Searching in: "+ folder);
                    foreach (string f in Directory.GetFiles(d, "*.eng"))
                    {
                        string extension = Path.GetExtension(f);
                        if (extension != null && (extension.Equals(".eng")))
                            ProcessFile(f);
                    }

                    TraverseDir(d);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ProcessFile(string file)
        {
            Console.WriteLine("Found Engage! spec: " + file);
            EngSpec result = Engage.FrontEnd.EngSpecFromFile(file);
            Assert.IsNotNull(result);
            Console.WriteLine("Parsed successfully");
        }
    }
}