using SystemDb.Helper;
using SystemDb.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DbAccess;
using System.Collections.Generic;

namespace SystemDb.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "LocalizedTextTest" und soll
    ///alle LocalizedTextTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class LocalizedTextTest {


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
        ///Ein Test für "FastLoad"
        ///</summary>
        //[TestMethod()]
        ////Checks if the fast load method of the column texts works as expected
        //public void FastLoadTest() {
        //    using (IDatabase conn = ConnectionManager.CreateConnection(ColumnTest.DbConfig)) {
        //        conn.Open();
        //        List<ColumnText> expected = conn.DbMapping.Load<ColumnText>(); 
        //        List<LocalizedText> actual = LocalizedText.FastLoad(conn, typeof(ColumnText));
        //        Assert.AreEqual(expected.Count, actual.Count);
        //        for (int i = 0; i < expected.Count; ++i) {
        //            foreach (var property in typeof(ColumnText).GetProperties()) {
        //                if (property.GetCustomAttributes(typeof(DbColumnAttribute), true).Length > 0)
        //                    Assert.AreEqual(property.GetValue(expected[i], null), property.GetValue(actual[i], null));
        //            }
        //        }
        //    }
        //}
    }
}
