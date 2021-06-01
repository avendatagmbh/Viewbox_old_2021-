using Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Business.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "EnhancedCsvReaderTest" und soll
    ///alle EnhancedCsvReaderTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class EnhancedCsvReaderTest {


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

        [TestMethod()]
        [DeploymentItem("Utils.dll")]
        public void EndsWithTest_Valid() {
            StringBuilder line = new StringBuilder("Test.csv"); // TODO: Passenden Wert initialisieren
            string endOfLine = ".csv";
            bool expected = true; 
            bool actual;
            actual = EnhancedCsvReader.EndsWith(line, endOfLine);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("Utils.dll")]
        public void EndsWithTest_InValid() {
            StringBuilder line = new StringBuilder("Test.csv"); // TODO: Passenden Wert initialisieren
            string endOfLine = ".csv1";
            bool expected = false;
            bool actual;
            actual = EnhancedCsvReader.EndsWith(line, endOfLine);
            Assert.AreEqual(expected, actual);
        }
    }
}
