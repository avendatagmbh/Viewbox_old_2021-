using eBalanceKitBusiness.Export;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eBalanceKitBusiness.Test
{
    
    
    /// <summary>
    ///This is a test class for XlsxExporterTest and is intended
    ///to contain all XlsxExporterTest Unit Tests
    ///</summary>
    [TestClass]
    public class XlsxExporterTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for GetTillStringFromNumber
        ///</summary>
        [TestMethod]
        public void GetTillStringFromNumberTest()
        {
            XlsxExporter target = new XlsxExporter(); // TODO: Initialize to an appropriate value
            int maxColumnCount = 1; // TODO: Initialize to an appropriate value
            string expected = "A"; // TODO: Initialize to an appropriate value
            string actual = target.GetTillStringFromNumber(maxColumnCount);
            Assert.AreEqual(expected, actual);
            maxColumnCount = 26;
            expected = "Z";
            actual = target.GetTillStringFromNumber(maxColumnCount);
            Assert.AreEqual(expected, actual);
            maxColumnCount = 27;
            expected = "AA";
            actual = target.GetTillStringFromNumber(maxColumnCount);
            Assert.AreEqual(expected, actual);
            maxColumnCount = 52;
            expected = "AZ";
            actual = target.GetTillStringFromNumber(maxColumnCount);
            Assert.AreEqual(expected, actual);
            maxColumnCount = 53;
            expected = "BA";
            actual = target.GetTillStringFromNumber(maxColumnCount);
            Assert.AreEqual(expected, actual);
        }
    }
}
