using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EngageTests
{
    [TestClass]
    public class Tests
    {
        private const string AppBuilderSpec = @"..\..\..\..\example\appbuilder.eng";
        private const string AppBuilderRule = @"..\..\..\..\example\simple.ab";
        private const string AppBuilderCode = @"..\..\..\..\tests";

        [TestMethod]
        public void TryAllStackedTests()
        {
            // warmup
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long{r.Next(0, 100)}.ab");
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
            }
            // actual measurement
            List<long> measures = new List<long>();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 100; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"stack{i}.ab");
                sw.Start();
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
                sw.Stop();
                Assert.IsNotNull(spec);
                Assert.AreEqual(0, spec.data.Count);
                Assert.AreEqual(i + 1 + i, spec.code.Count);
                var if1 = spec.code[i ] as AB.IfStmt;
                Assert.IsNotNull(if1);
                Assert.AreEqual(i + 1 + i, if1.branch.Count);
                var if2 = if1.branch[i] as AB.IfStmt;
                Assert.IsNotNull(if2);
                Assert.AreEqual(i, if2.branch.Count);
                Console.WriteLine($"Tested 'stack{i}.ab': OK in {sw.ElapsedTicks} ticks");
                measures.Add(sw.ElapsedTicks);
                sw.Reset();
            }
            Console.WriteLine($"AVERAGE time: {measures.Average()}");
        }

        [TestMethod]
        public void TryAllDeepTests()
        {
            // warmup
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long{r.Next(0, 100)}.ab");
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
            }
            // actual measurement
            List<long> measures = new List<long>();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 100; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"deep{i}.ab");
                sw.Start();
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
                sw.Stop();
                Assert.IsNotNull(spec);
                Assert.AreEqual(0, spec.data.Count);
                Assert.AreEqual(1, spec.code.Count);
                Console.WriteLine($"Tested 'deep{i}.ab': OK in {sw.ElapsedTicks} ticks");
                measures.Add(sw.ElapsedTicks);
                sw.Reset();
            }
            Console.WriteLine($"AVERAGE time: {measures.Average()}");
        }

        [TestMethod]
        public void TryAllDeepExpTests()
        {
            // warmup
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long{r.Next(0, 100)}.ab");
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
            }
            // actual measurement
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 5; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"deep10e{i}.ab");
                sw.Start();
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
                sw.Stop();
                Assert.IsNotNull(spec);
                Assert.AreEqual(0, spec.data.Count);
                Assert.AreEqual(1, spec.code.Count);
                Console.WriteLine($"Tested 'deep10e{i}.ab': OK in {sw.ElapsedTicks} ticks");
                sw.Reset();
            }
        }

        [TestMethod]
        public void TryAllLongTests()
        {
            // warmup
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long{r.Next(0, 100)}.ab");
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
            }
            // actual measurement
            List<long> measures = new List<long>();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 100; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long{i}.ab");
                sw.Start();
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
                sw.Stop();
                Assert.IsNotNull(spec);
                Assert.AreEqual(0, spec.data.Count);
                Assert.AreEqual(i, spec.code.Count);
                Console.WriteLine($"Tested 'long{i}.ab': OK in {sw.ElapsedTicks} ticks");
                measures.Add(sw.ElapsedTicks);
                sw.Reset();
            }
            Console.WriteLine($"AVERAGE time: {measures.Average()}");
        }

        [TestMethod]
        public void TryAllLongExpTests()
        {
            // warmup
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long{r.Next(0, 100)}.ab");
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
            }
            // actual measurement
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 8; i++)
            {
                string fname = Path.Combine(AppBuilderCode, $"long10e{i}.ab");
                sw.Start();
                var parser = new AB.Parser(File.ReadAllText(fname));
                var spec = parser.Parse() as AB.ABProgram;
                sw.Stop();
                Assert.IsNotNull(spec);
                Assert.AreEqual(0, spec.data.Count);
                Assert.AreEqual((int)Math.Pow(10, i), spec.code.Count);
                Console.WriteLine($"Tested 'long10e{i}.ab': OK in {sw.ElapsedTicks} ticks");
                sw.Reset();
            }
        }

        [TestMethod]
        public void TryFailingExample1()
        {
            var parser = new AB.Parser(@"
                clear akpjke
                map pwrmgtq to hjturvlsikt
                map 1519307387 to ejxegwgv
                if 1880563650 return endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(4, spec.code.Count);
            Assert.IsInstanceOfType(spec.code[0], typeof(AB.ClearStmt));
            Assert.IsInstanceOfType(spec.code[1], typeof(AB.MapStmt));
            Assert.IsInstanceOfType(spec.code[2], typeof(AB.MapStmt));
            Assert.IsInstanceOfType(spec.code[3], typeof(AB.IfStmt));
            Assert.IsInstanceOfType((spec.code[3] as AB.IfStmt).branch[0], typeof(AB.ReturnStmt));
        }

        [TestMethod]
        public void TryFailingExample2()
        {
            var parser = new AB.Parser(@"
                if abzhnyfsobo return endif
                if 99773466 return endif
                if iwksk return endif
                map ifnaqtipf to bqnl
                return
                return");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(6, spec.code.Count);
            Assert.IsInstanceOfType(spec.code[0], typeof(AB.IfStmt));
            Assert.IsInstanceOfType(spec.code[1], typeof(AB.IfStmt));
            Assert.IsInstanceOfType(spec.code[2], typeof(AB.IfStmt));
            Assert.IsInstanceOfType(spec.code[3], typeof(AB.MapStmt));
            Assert.IsInstanceOfType(spec.code[4], typeof(AB.ReturnStmt));
            Assert.IsInstanceOfType(spec.code[5], typeof(AB.ReturnStmt));
        }

        [TestMethod]
        public void TryFailingExample3()
        {
            var parser = new AB.Parser(@"
                return
                if COND1
                    clear mvgdm
                    if COND2
                        clear btiyxe
                    endif
                    clear gq
                endif
                if gwv return endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(3, spec.code.Count);
            Assert.IsInstanceOfType(spec.code[0], typeof(AB.ReturnStmt));
            Assert.IsInstanceOfType(spec.code[1], typeof(AB.IfStmt));
            Assert.IsInstanceOfType(spec.code[2], typeof(AB.IfStmt));
            var if1 = spec.code[1] as AB.IfStmt;
            Assert.IsInstanceOfType(if1.branch[0], typeof(AB.ClearStmt));
            Assert.IsInstanceOfType(if1.branch[1], typeof(AB.IfStmt));
            Assert.IsInstanceOfType(if1.branch[2], typeof(AB.ClearStmt));
            var if2 = if1.branch[1] as AB.IfStmt;
            Assert.IsInstanceOfType(if1.branch[0], typeof(AB.ClearStmt));
        }

        [TestMethod]
        public void TryAlmostBadTokens()
        {
            var parser = new AB.Parser(@"map mapper to iffer");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
            var map = spec.code[0] as AB.MapStmt;
            Assert.IsNotNull(map);
            var src = map.source as AB.Var;
            Assert.IsNotNull(src);
            Assert.AreEqual("mapper", src.s);
            var tgt = map.target as AB.Var;
            Assert.IsNotNull(src);
            Assert.AreEqual("iffer", tgt.s);
        }

        [TestMethod]
        public void TryExperimental1()
        {
            var parser = new AB.Parser("if x map y to z endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
            var if1 = spec.code[0] as AB.IfStmt;
            Assert.IsNotNull(if1);
            Assert.AreEqual(1, if1.branch.Count);
            var map1 = if1.branch[0] as AB.MapStmt;
            Assert.IsNotNull(map1);
        }

        [TestMethod]
        public void TryExperimental2()
        {
            var parser = new AB.Parser("if a map b to c map d to e endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
            var if1 = spec.code[0] as AB.IfStmt;
            Assert.IsNotNull(if1);
            Assert.AreEqual(2, if1.branch.Count);
            var map1 = if1.branch[0] as AB.MapStmt;
            Assert.IsNotNull(map1);
            var map2 = if1.branch[1] as AB.MapStmt;
            Assert.IsNotNull(map2);
        }

        [TestMethod]
        public void TryExperimental3()
        {
            var parser = new AB.Parser("if a map b to c map d to e endif map f to g");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(2, spec.code.Count);
            Console.WriteLine($"[AB] : {spec.code[0]} + {spec.code[1]}");
            var if1 = spec.code[0] as AB.IfStmt;
            Assert.IsNotNull(if1);
            Assert.AreEqual(2, if1.branch.Count);
            var map1 = if1.branch[0] as AB.MapStmt;
            Assert.IsNotNull(map1);
            var map2 = if1.branch[1] as AB.MapStmt;
            Assert.IsNotNull(map2);
            var map3 = spec.code[1] as AB.MapStmt;
            Assert.IsNotNull(map3);
        }

        [TestMethod]
        public void TryExperimental4()
        {
            var parser = new AB.Parser("if a if b return endif endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
            Console.WriteLine($"[(1)] : {spec.code[0]}");
            var if1 = spec.code[0] as AB.IfStmt; // if a
            Assert.IsNotNull(if1);
            Assert.AreEqual(1, if1.branch.Count);
            Console.WriteLine($"[(2)] : {if1.branch[0]}");
            var if2 = if1.branch[0] as AB.IfStmt; // if b
            Assert.IsNotNull(if2);
            Assert.AreEqual(1, if2.branch.Count);
            Console.WriteLine($"[(3)] : {if2.branch[0]}");
            var ret1 = if2.branch[0] as AB.ReturnStmt;
            Assert.IsNotNull(ret1);
        }

        [TestMethod]
        public void TryExperimental5()
        {
            var parser = new AB.Parser("if a if b map c to d endif endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            //Console.WriteLine($"[(1)] : {spec.code[0]} + {spec.code[1]}");
            Assert.AreEqual(1, spec.code.Count);
            Console.WriteLine($"[(1)] : {spec.code[0]}");
            var if1 = spec.code[0] as AB.IfStmt; // if a
            Assert.IsNotNull(if1);
            Assert.AreEqual(1, if1.branch.Count);
            Console.WriteLine($"[(2)] : {if1.branch[0]}");
            var if2 = if1.branch[0] as AB.IfStmt; // if b
            Assert.IsNotNull(if2);
            Assert.AreEqual(1, if2.branch.Count);
            Console.WriteLine($"[(3)] : {if2.branch[0]}");
            var map1 = if2.branch[0] as AB.MapStmt; // map c to d
            Assert.IsNotNull(map1);
        }

        [TestMethod]
        public void TryExperimental6()
        {
            var parser = new AB.Parser("if a if b map c to d map e to f endif map g to h endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
            Console.WriteLine($"[(1)] : {spec.code[0]}");
            var if1 = spec.code[0] as AB.IfStmt; // if a
            Assert.IsNotNull(if1);
            Assert.AreEqual(2, if1.branch.Count);
            Console.WriteLine($"[(2)] : {if1.branch[0]} + {if1.branch[1]}");
            var if2 = if1.branch[0] as AB.IfStmt; // if b
            Assert.IsNotNull(if2);
            Assert.AreEqual(2, if2.branch.Count);
            Console.WriteLine($"[(3)] : {if2.branch[0]} + {if2.branch[1]}");
            var map1 = if2.branch[0] as AB.MapStmt; // map c to d
            Assert.IsNotNull(map1);
            var map2 = if2.branch[1] as AB.MapStmt; // map e to f
            Assert.IsNotNull(map2);
            var map3 = if1.branch[1] as AB.MapStmt; // map g to h
            Assert.IsNotNull(map3);
        }

        [TestMethod]
        public void TryEngagedParser()
        {
            var parser = new AB.Parser(File.ReadAllText(AppBuilderRule));
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(1, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
        }

        [TestMethod]
        public void TryAppBuilder()
        {
            var spec = Engage.Parser.ParseEngSpec(File.ReadAllText(AppBuilderSpec));
        }

        [TestMethod]
        public void TryTypeDecl()
        {
            var spec = Engage.Parser.ParseTypeDecl("Program;");
        }

        [TestMethod]
        public void TryTokenDecl()
        {
            var spec = Engage.Parser.ParseTokenDecl("' ' :: skip");
        }

        [TestMethod]
        public void TryQuoted()
        {
            var spec = Engage.Parser.ParseQuoted("'foo'");
            Assert.AreEqual("foo", spec);
        }

        [TestMethod]
        public void TryDeep0()
        {
            string fname = Path.Combine(AppBuilderCode, $"deep0.ab");
            var parser = new AB.Parser(File.ReadAllText(fname));
            Console.WriteLine($"Parsing '{File.ReadAllText(fname)}'");
            var spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
        }

        [TestMethod]
        public void TryTokeniser()
        {
            //var parser = new AB.Parser(@"map mapper to iffer");
            //Tuple<AB.Parser.TokenType, string> token = null;
            //do
            //{
            //    token = parser.NextToken();
            //    Console.WriteLine($"'{token.Item2}' :: {token.Item1}");
            //}
            //while (token.Item1 != AB.Parser.TokenType.TEOF);
        }
    }
}