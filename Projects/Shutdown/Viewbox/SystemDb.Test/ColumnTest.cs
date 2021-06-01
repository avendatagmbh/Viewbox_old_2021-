using SystemDb.Internal;
using DbAccess.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DbAccess;
using System.Collections.Generic;

namespace SystemDb.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ColumnTest" und soll
    ///alle ColumnTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ColumnTest {


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


        public static DbConfig DbConfig = new DbConfig("MySQL"){Hostname = "dbelectrolux", Username = "root", Password = "avendata", DbName = "viewbox"};
        //static DbConfig DbConfig = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", Password = "avendata", DbName = "viewbox2" };
        /// <summary>
        ///Ein Test für "FastLoad"
        ///</summary>
        //[TestMethod()]
        ////Checks if the fast load method of the columns works as expected
        //public void FastLoadTest() {
        //    using (IDatabase conn = ConnectionManager.CreateConnection(DbConfig)) {
        //        conn.Open();
        //        List<Column> expected = conn.DbMapping.Load<Column>(); // TODO: Passenden Wert initialisieren
        //        List<Column> actual = Column.FastLoad(conn);
        //        Assert.AreEqual(expected.Count, actual.Count);
        //        for (int i = 0; i < expected.Count; ++i) {
        //            foreach (var property in typeof(Column).GetProperties()) {
        //                if (property.GetCustomAttributes(typeof(DbColumnAttribute),true).Length > 0)
        //                    Assert.AreEqual(property.GetValue(expected[i], null), property.GetValue(actual[i], null));
        //            }
        //        }
        //    }
        //}
    }
}
