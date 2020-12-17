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
        public void ParseOneTagOpen()
        {
            var parser = new Parser("<tag>");
            var result = parser.Parse() as EngagedXmlDoc;
            Assert.IsNotNull(result);
            Assert.AreEqual(1,result.tags.Count);
            var tag = result.tags[0] as TagOpen;
            Assert.IsNotNull(tag);
            Assert.AreEqual("tag",tag.n?.value);
        }

        [TestMethod]
        [TestCategory("EAX")]
        public void ParseOneTagOpenClose()
        {
            var parser = new Parser("<tag></tag>");
            var result = parser.Parse() as EngagedXmlDoc;
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.tags.Count);
            var tag1 = result.tags[0] as TagOpen;
            Assert.IsNotNull(tag1);
            var tag2 = result.tags[1] as TagClose;
            Assert.IsNotNull(tag2);
            Assert.AreEqual(tag1.n?.value, tag2.n?.value);
        }
    }
}