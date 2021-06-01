using ViewBuilderCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ViewBuilderCommon.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "IndexTest" und soll
    ///alle IndexTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class IndexTest {


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
        public void IndexConstructorTest_NoSpacesInTableName() {
            string sql = "CREATE INDEX _dd02t_ddlanguage_tabname ON dd02t(tabname,ddlanguage);";
            Index target = new Index(sql);
            Assert.AreEqual(target.Columns[0], "ddlanguage");
            Assert.AreEqual(target.Columns[1], "tabname");
            Assert.AreEqual(target.Table, "dd02t");
        }

        [TestMethod()]
        public void IndexConstructorTest_WithSpacesInTableName() {
            string sql = "CREATE INDEX _dd02t_ddlanguage_tabname ON `dd02t with spaces`(tabname,ddlanguage);";
            Index target = new Index(sql);
            Assert.AreEqual(target.Columns[0], "ddlanguage");
            Assert.AreEqual(target.Columns[1], "tabname");
            Assert.AreEqual(target.Table, "dd02t with spaces");
        }

        [TestMethod()]
        public void Index_WithEnquotedKey() {
            string sql = "CREATE INDEX _dd02t_key_tabname ON dd02t(`key`,ddlanguage);";
            Index target = new Index(sql);
            Assert.AreEqual(target.Columns[0], "ddlanguage");
            Assert.AreEqual(target.Columns[1], "key");
            Assert.AreEqual(target.Table, "dd02t");
        }
    }
}
