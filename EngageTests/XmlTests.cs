using EAX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngageTests
{
    [TestClass]
    public class XmlTests
    {
        [TestMethod]
        [TestCategory("EAX")]
        public void ParseEmpty()
        {
            var parser = new Parser("");
            var result = parser.Parse() as EngagedXmlDoc;
            Assert.IsNotNull(result);
            Assert.AreEqual(0,result.tags.Count);
        }
        
        [TestMethod]
        [TestCategory("EAX")]
        public void ParseOneOpenTag()
        {
            var parser = new Parser("<tag>");
            var result = parser.Parse() as EngagedXmlDoc;
            Assert.IsNotNull(result);
            Assert.AreEqual(1,result.tags.Count);
            var tag = result.tags[0] as TagOpen;
            Assert.IsNotNull(tag);
            Assert.AreEqual("tag",tag.n);
        }
    }
}