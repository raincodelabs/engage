using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace EngageTests
{
    [TestClass]
    public class Tests
    {
        private const string AppBuilderSpec = @"..\..\..\..\test\appbuilder.eng";
        private const string AppBuilderRule = @"..\..\..\..\test\simple.ab";

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
            Assert.AreEqual(1, if1.code.Count);
            var map1 = if1.code[0] as AB.MapStmt;
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
            Assert.AreEqual(2, if1.code.Count);
            var map1 = if1.code[0] as AB.MapStmt;
            Assert.IsNotNull(map1);
            var map2 = if1.code[1] as AB.MapStmt;
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
            Assert.AreEqual(2, if1.code.Count);
            var map1 = if1.code[0] as AB.MapStmt;
            Assert.IsNotNull(map1);
            var map2 = if1.code[1] as AB.MapStmt;
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
            Assert.AreEqual(1, if1.code.Count);
            Console.WriteLine($"[(2)] : {if1.code[0]}");
            var if2 = if1.code[0] as AB.IfStmt; // if b
            Assert.IsNotNull(if2);
            Assert.AreEqual(1, if2.code.Count);
            Console.WriteLine($"[(3)] : {if2.code[0]}");
            var ret1 = if2.code[0] as AB.ReturnStmt;
            Assert.IsNotNull(ret1);
        }

        [TestMethod]
        public void TryExperimental5()
        {
            var parser = new AB.Parser("if a if b map c to d endif endif");
            AB.ABProgram spec = parser.Parse() as AB.ABProgram;
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.data.Count);
            Assert.AreEqual(1, spec.code.Count);
            Console.WriteLine($"[(1)] : {spec.code[0]}");
            var if1 = spec.code[0] as AB.IfStmt; // if a
            Assert.IsNotNull(if1);
            Assert.AreEqual(1, if1.code.Count);
            Console.WriteLine($"[(2)] : {if1.code[0]}");
            var if2 = if1.code[0] as AB.IfStmt; // if b
            Assert.IsNotNull(if2);
            Assert.AreEqual(1, if2.code.Count);
            Console.WriteLine($"[(3)] : {if2.code[0]}");
            var map1 = if2.code[0] as AB.MapStmt; // map c to d
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
            Assert.AreEqual(2, if1.code.Count);
            Console.WriteLine($"[(2)] : {if1.code[0]} + {if1.code[1]}");
            var if2 = if1.code[0] as AB.IfStmt; // if b
            Assert.IsNotNull(if2);
            Assert.AreEqual(2, if2.code.Count);
            Console.WriteLine($"[(3)] : {if2.code[0]} + {if2.code[1]}");
            var map1 = if2.code[0] as AB.MapStmt; // map c to d
            Assert.IsNotNull(map1);
            var map2 = if2.code[1] as AB.MapStmt; // map e to f
            Assert.IsNotNull(map2);
            var map3 = if1.code[1] as AB.MapStmt; // map g to h
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
    }
}