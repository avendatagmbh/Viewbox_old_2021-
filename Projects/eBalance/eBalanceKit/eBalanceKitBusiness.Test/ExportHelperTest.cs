using eBalanceKitBusiness.Export;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace eBalanceKitBusiness.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ExportHelperTest" und soll
    ///alle ExportHelperTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ExportHelperTest {


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


        private string _fileNameNetworkShare = @"\\elzar\Users\sev\Documents\test.pdf";
        private string _fileNameNetworkFalse = "\\\\elzar\\Users\\sev\\!#'!§$&/()\"/*-+test.pdf";

        /// <summary>
        ///Ein Test für "GetValidFileName"
        ///</summary>
        [TestMethod()]
        public void GetValidFileNameTestNetworkShare() {
            string file = _fileNameNetworkShare; // TODO: Passenden Wert initialisieren
            string expected = "test.pdf"; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetValidFileName(file);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetValidFilePath"
        ///</summary>
        [TestMethod()]
        public void GetValidFilePathTestNetworkShare() {
            string filename = _fileNameNetworkShare; // TODO: Passenden Wert initialisieren
            bool stillTyping = false; // TODO: Passenden Wert initialisieren
            string expected = "\\\\elzar\\Users\\sev\\Documents"; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetValidFilePath(filename, stillTyping);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetValidFileName"
        ///</summary>
        [TestMethod()]
        public void GetValidFileNameTestFalse() {
            string file = "\\\\elzar\\Users\\sev\\!#'!§$&/()\"/*-+test.pdf"; // TODO: Passenden Wert initialisieren
            string expected = "____§$&_()___-_test.pdf"; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetValidFileName(file);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetValidFilePath"
        ///</summary>
        [TestMethod()]
        public void GetValidFilePathTestFalse() {
            string filename = _fileNameNetworkFalse; // TODO: Passenden Wert initialisieren
            bool stillTyping = false; // TODO: Passenden Wert initialisieren
            string expected = "\\\\elzar\\Users\\sev"; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetValidFilePath(filename, stillTyping);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetValidFileName"
        ///</summary>
        [TestMethod()]
        public void GetValidFileNameTest() {
            string file = string.Empty; // TODO: Passenden Wert initialisieren
            string expected = string.Empty; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetValidFileName(file);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetValidFilePath"
        ///</summary>
        [TestMethod()]
        public void GetValidFilePathTest() {
            string filename = string.Empty; // TODO: Passenden Wert initialisieren
            bool stillTyping = false; // TODO: Passenden Wert initialisieren
            string expected = string.Empty; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetValidFilePath(filename, stillTyping);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetFileDestination"
        ///</summary>
        [TestMethod()]
        public void GetFileDestinationTestUnc() {
            string file = "\\\\elzar\\Users\\sev\\test.pdf"; // TODO: Passenden Wert initialisieren
            string expected = file; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetFileDestination(file);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///Ein Test für "GetFileDestination"
        ///</summary>
        [TestMethod()]
        public void GetFileDestinationTestFalse() {
            string file = "C:\\te?s%t\\bl%a.pdf"; // TODO: Passenden Wert initialisieren
            string expected = "C:\\te_s_t\\bl_a.pdf"; // TODO: Passenden Wert initialisieren
            string actual;
            actual = ExportHelper.GetFileDestination(file);
            Assert.AreEqual(expected, actual);
        }
    }
}
