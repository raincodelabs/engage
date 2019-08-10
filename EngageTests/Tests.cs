using Engage.front;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace EngageTests
{
    [TestClass]
    public class Tests
    {
        private const string AppBuilderSpec = @"..\..\..\..\test\appbuilder.eng";

        [TestMethod]
        public void TryAppBuilder()
        {
            var spec = Parser.ParseEngSpec(File.ReadAllText(AppBuilderSpec));
        }

        [TestMethod]
        public void TryTypeDecl()
        {
            var spec = Parser.ParseTypeDecl("Program;");
        }

        [TestMethod]
        public void TryTokenDecl()
        {
            var spec = Parser.ParseTokenDecl("' ' :: skip");
        }
        [TestMethod]
        public void TryQuoted()
        {
            var spec = Parser.ParseQuoted("'foo'");
            Assert.AreEqual("foo", spec);
        }

    }
}