using Business.Structures.MetadataAgents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Config.Interfaces.Config;

namespace Business.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "MetadataAgentCsvTest" und soll
    ///alle MetadataAgentCsvTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class MetadataAgentCsvTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        // 
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        //
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Ein Test für "GetBaseFilename"
        ///</summary>
        [TestMethod()]
        public void GetBaseFilename_NormalCsvFile() {
            string actual = MetadataAgentCsv.GetBaseFilename(@"C:\test.csv");
            string expected = @"C:\test.csv";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseFilename_NumberCsvFile() {
            string actual = MetadataAgentCsv.GetBaseFilename(@"C:\test_0.csv");
            string expected = @"C:\test.csv";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseFilename_LongNumberCsvFile() {
            string actual = MetadataAgentCsv.GetBaseFilename(@"C:\test_155.csv");
            string expected = @"C:\test.csv";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseFilename_DoubleNumberCsvFile() {
            string actual = MetadataAgentCsv.GetBaseFilename(@"C:\test_0_1.csv");
            string expected = @"C:\test_0.csv";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ExtractPartNumber_HasNumber() {
            int actual = MetadataAgentCsv.ExtractPartNumber(@"C:\test_15.csv");
            int expected = 15;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ExtractPartNumber_HasNoNumber() {
            int actual = MetadataAgentCsv.ExtractPartNumber(@"C:\test_.csv");
            int expected = -1;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReplacePartNumber_CorrectFormat() {
            string expected = @"c:\test_43.csv"; 
            string actual = MetadataAgentCsv.ReplacePartNumber(@"c:\test_55.CSV", 43);
            Assert.AreEqual(expected, actual);
            
        }

        [TestMethod()]
        public void ReplacePartNumber_IncorrectFormat() {
            string expected = @"c:\test55.CSV";
            string actual = MetadataAgentCsv.ReplacePartNumber(@"c:\test55.CSV", 43);
            Assert.AreEqual(expected, actual);

        }
    }
}
