using ViewBuilderCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ViewBuilderCommon.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ViewInfoTest" und soll
    ///alle ViewInfoTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ViewInfoTest {


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
        ///Ein Test für "SetCompleteProcedure"
        ///</summary>
        [TestMethod()]
        public void SetCompleteProcedureTest() {
            ViewInfo target = new ViewInfo();
            string proc =
                @"DELIMITER $$
DROP PROCEDURE IF EXISTS blackrock_jun2005.saldenliste_fonds $$
second statement
DELIMITER ;
";
            target.SetCompleteProcedure(proc);
            Assert.AreEqual(2, target.ProcedureStatements.Count);
            Assert.AreEqual("DROP PROCEDURE IF EXISTS blackrock_jun2005.saldenliste_fonds", target.ProcedureStatements[0]);
            Assert.AreEqual("second statement", target.ProcedureStatements[1]);
        }
    
        [TestMethod()]
        public void DelimiterSwitchTest() {
            ViewInfo target = new ViewInfo();
            string proc =
                @"DELIMITER $$
DROP PROCEDURE IF EXISTS blackrock_jun2005.saldenliste_fonds $$
second statement
over two lines$$
third_statement$$
DELIMITER ;
first with new delimiter;
second with new delimiter;
";
            target.SetCompleteProcedure(proc);
            Assert.AreEqual(5, target.ProcedureStatements.Count);
            Assert.AreEqual("DROP PROCEDURE IF EXISTS blackrock_jun2005.saldenliste_fonds", target.ProcedureStatements[0]);
            Assert.AreEqual("second statement\r\nover two lines", target.ProcedureStatements[1]);
            Assert.AreEqual("third_statement", target.ProcedureStatements[2]);
            Assert.AreEqual("first with new delimiter", target.ProcedureStatements[3]);
            Assert.AreEqual("second with new delimiter", target.ProcedureStatements[4]);
        }

        [TestMethod()]
        public void NoDelimiter() {
            ViewInfo target = new ViewInfo();
            string proc =
                @"first statement;
second statement;
";
            target.SetCompleteProcedure(proc);
            Assert.AreEqual(2, target.ProcedureStatements.Count);
            Assert.AreEqual("first statement", target.ProcedureStatements[0]);
            Assert.AreEqual("second statement", target.ProcedureStatements[1]);
        }

        [TestMethod()]
        public void SplitStatements_DelimiterAtEnd() {
            ViewInfo target = new ViewInfo();
            string proc =
                "DROP PROCEDURE IF EXISTS prod.tax_detail_report_gbw_proc;\r\nDELIMITER $$\r\nCREATE PROCEDURE prod.tax_detail_report_gbw_proc(\r\nEND \t$$\t\r\n\t   ";
            target.SetCompleteProcedure(proc);
            Assert.AreEqual(2, target.ProcedureStatements.Count);
            Assert.AreEqual("DROP PROCEDURE IF EXISTS prod.tax_detail_report_gbw_proc", target.ProcedureStatements[0]);
            Assert.AreEqual("CREATE PROCEDURE prod.tax_detail_report_gbw_proc(\r\nEND", target.ProcedureStatements[1]);
        }

        [TestMethod()]
        public void DelimiterNotInFirstLine() {
            ViewInfo target = new ViewInfo();
            string proc =
                @"first statement;
DELIMITER $$
DROP PROCEDURE IF EXISTS blackrock_jun2005.saldenliste_fonds $$
second statement
over two lines$$
";
            target.SetCompleteProcedure(proc);
            Assert.AreEqual(3, target.ProcedureStatements.Count);
            Assert.AreEqual("first statement", target.ProcedureStatements[0]);
            Assert.AreEqual("DROP PROCEDURE IF EXISTS blackrock_jun2005.saldenliste_fonds", target.ProcedureStatements[1]);
            Assert.AreEqual("second statement\r\nover two lines", target.ProcedureStatements[2]);
        }
    }
}
