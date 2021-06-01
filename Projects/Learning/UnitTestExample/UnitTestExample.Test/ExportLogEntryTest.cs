using UnitTestExample;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestExample.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ExportLogEntryTest" und soll
    ///alle ExportLogEntryTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ExportLogEntryTest {


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
        ///Ein Test für "ReadFromLine"
        ///</summary>
        [TestMethod()]
        public void ReadFromLineTest_ValidLine() {
            string line = @"BEH;allazymt;allazymt;;20120113;174549;62;20120113;174850;63;181";
            ExportLogEntry actual = ExportLogEntry.ReadFromLine(line);
            Assert.AreEqual("allazymt", actual.TableName, "Tablename is wrong");
            Assert.AreEqual(63, actual.CountAfter, "Count after is wrong");
            Assert.AreEqual(62, actual.CountBefore, "Count before is wrong");
        }

        [TestMethod()]
        public void ReadFromLineTest_SpecialCharacters() {
            string line = @"BEH;a#!üöä€|llazymt2;a#!üöä€|llazymt2;;20120113;174549;62;20120113;174850;63;181";
            ExportLogEntry actual = ExportLogEntry.ReadFromLine(line);
            Assert.AreEqual("a#!üöä€|llazymt2", actual.TableName, "Tablename is wrong");
        }

        [TestMethod()]
        public void ReadFromLineTest_LongCount() {
            string line = @"BEH;tablename;tablename;;20120113;174549;2200000000;20120113;174850;2200000000;181";
            ExportLogEntry actual = ExportLogEntry.ReadFromLine(line);
            Assert.AreEqual(2200000000, actual.CountBefore);
            Assert.AreEqual(2200000000, actual.CountAfter );
        }

        [TestMethod(), ExpectedException(typeof(InvalidOperationException))]
        public void ReadFromLineTest_BadLine() {
            string line = @"BEH;allazymt;allazymt;;20120113;174549;62;20120113;174850;63;181;shouldnotbehere";
            ExportLogEntry actual = ExportLogEntry.ReadFromLine(line);
        }
    }
}
