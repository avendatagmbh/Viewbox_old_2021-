using Business.CsvImporter.ExportLogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Business.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ExportLogTest" und soll
    ///alle ExportLogTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ExportLogTest {


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
        ///Ein Test für "ReadFile"
        ///</summary>
        [TestMethod()]
        public void ReadFileTest_ReadingWorks() {
            ExportLog target = new ExportLog();
            TextReader reader = new StringReader(
                @"USER;TableName;FileName;Filter;DateBefore;TimeBefore;CountBeforeStr;DateAfter;TimeAfter;CountAfterStr;Duration
BEH;allazymt;allazymt;;20120113;174549;62;20120113;174850;62;181
BEH;allazymt2;allazymt2;;20120113;174850;71;20120113;174850;71;71
"
                );
            target.ReadFile(reader);
            Assert.AreEqual(target.Count(), 2);
            Assert.AreEqual(target.GetEntry("allazymt").CountBefore, 62);
            Assert.AreEqual(target.GetEntry("allazymt").CountAfter, 62);
            Assert.AreEqual(target.GetEntry("allazymt").TableName, "allazymt");

            Assert.AreEqual(target.GetEntry("allazymt2").CountBefore, 71);
            Assert.AreEqual(target.GetEntry("allazymt2").CountAfter, 71);
            Assert.AreEqual(target.GetEntry("allazymt2").TableName, "allazymt2");

        }
    }
}
