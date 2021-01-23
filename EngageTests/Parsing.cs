using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engage.A;
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
            EngSpec result = Engage.parsing.TheParser.Parse(input);
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
        public void TestAntlrParserFiles()
        {
            TraverseDir(FourUp);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserOnAppbuilder()
        {
            CompareGrammar(KnownGrammars[0]);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserOnOpenClose()
        {
            CompareGrammar(KnownGrammars[1]);
        }

        [TestMethod]
        [TestCategory("Engage")]
        public void TestAntlrParserOnFuzzy()
        {
            CompareGrammar(KnownGrammars[2]);
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

        private void CompareGrammar(string file)
        {
            Console.WriteLine("Found Engage! spec: " + file);
            string input = File.ReadAllText(file);
            EngSpec result1 = Engage.FrontEnd.EngSpecFromText(input);
            EngSpec result2 = Engage.FrontEnd.LegacyEngSpecFromText(input);
            Assert.IsTrue(result1.Equals(result2));
            Console.WriteLine("Compared successfully");
        }

        private void ProcessFile(string file)
        {
            Console.WriteLine("Found Engage! spec: " + file);
            string input = File.ReadAllText(file);
            EngSpec result = Engage.parsing.TheParser.Parse(input);
            Assert.IsNotNull(result);
            Console.WriteLine("Parsed successfully");
        }
    }
}