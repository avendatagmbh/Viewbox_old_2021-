using ViewBuilderCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ProjectDb.Tests
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
        public void IndexConstructorTest2() {
            string sql = string.Empty; // TODO: Passenden Wert initialisieren
            Index target = new Index(
            "CREATE INDEX _kdlieferadr_kdkey on kdlieferadr(kdkey)"
                );
            Assert.AreEqual(target.Table, "kdlieferadr");
            Assert.AreEqual(target.Columns.Count, 1);
            Assert.AreEqual(target.Columns[0], "kdkey");
        }
        
    }
}
