using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace EngageTests
{
    [TestClass]
    public class Tests
    {
        private const string AppBuilderSpec = @"..\..\..\..\test\appbuilder.eng";
        private const string AppBuilderRule = @"..\..\..\..\test\simple.ab";

        [TestMethod]
        public void TryEngagedParser()
        {
            var parser = new AB.Parser(File.ReadAllText(AppBuilderRule));
            var spec = parser.Parse();
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