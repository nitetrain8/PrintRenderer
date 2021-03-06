using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrintRenderer;

namespace DocRendererTests
{
    [TestClass]
    public class PrintStringReaderTest
    {
        public PrintStringReader reader;

        [TestInitialize]
        public void SetupTest()
        {
            reader = new PrintStringReader();
        }

        [TestMethod]
        public void TestMethod1()
        {
            //          012345678

            //          123456789
            //                12345678
            //                      123456789
            // basic split-at-lines test
            reader.Set("lorem ipsum dolorem");
            Assert.AreEqual(reader.Read(7), "lorem");
            Assert.AreEqual(reader.Read(8), "ipsum");
            Assert.AreEqual(reader.Read(10), "dolorem");
        }

        [TestMethod]
        public void TestMethod2()
        {
            // split at newline + trim leading space
            reader.Set(" lorem\nipsum dolorem");
            Assert.AreEqual(reader.Read(8), "lorem");
            Assert.AreEqual(reader.Read(8), "ipsum");
            Assert.AreEqual(reader.Read(10), "dolorem");
        }
        
        [TestMethod]
        public void TestMethod3(){
            // split at crlf
            reader.Set(" lorem\r\nipsum dolorem");
            Assert.AreEqual(reader.Read(8), "lorem");
            Assert.AreEqual(reader.Read(8), "ipsum");
            Assert.AreEqual(reader.Read(10), "dolorem");
        }

        [TestMethod]
        public void TestMethod4()
        {
            // split at cr
            reader.Set(" lorem\r\nipsum dolorem");
            Assert.AreEqual(reader.Read(8), "lorem");
            Assert.AreEqual(reader.Read(8), "ipsum");
            Assert.AreEqual(reader.Read(10), "dolorem");
        }

        [TestMethod]
        public void TestMethod5()
        {
            // split at newlines, first line should be blank
            reader.Set("\nlorem\r\nipsum dolorem");
            Assert.AreEqual(reader.Read(8), "");
            Assert.AreEqual(reader.Read(8), "lorem");
            Assert.AreEqual(reader.Read(8), "ipsum");
            Assert.AreEqual(reader.Read(10), "dolorem");
        }

        [TestMethod]
        public void TestMethod6()
        {
            //          123456789
            // return whole line if no spaces
            reader.Set("loremipsumdo lorem");
            Assert.AreEqual(reader.Read(7), "loremip");
            Assert.AreEqual(reader.Read(3), "sum");
            Assert.AreEqual(reader.Read(4), "do");
            Assert.AreEqual(reader.Read(5), "lorem");
        }

        [TestMethod]
        public void TestMethod7()
        {
            reader.Set("Dolorem aliquam");
            Assert.AreEqual(reader.Read(85), "Dolorem aliquam");
        }
    }
}
