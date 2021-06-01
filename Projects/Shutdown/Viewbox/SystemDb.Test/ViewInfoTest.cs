using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SystemDb.Test
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


        //[TestMethod()]
        //public void SplitStatementsTest_NoDelimiter() {
        //    string sql = "DROP DATABASE test;INSERT INTO bukrs ('VALUE')";
        //    List<string> actual = ViewInfo.SplitStatements(sql);
        //    Assert.AreEqual(2, actual.Count);
        //    Assert.AreEqual("DROP DATABASE test", actual[0]);
        //    Assert.AreEqual("INSERT INTO bukrs ('VALUE')", actual[1]);
        //}

        //[TestMethod()]
        //public void SplitStatementsTest_DelimiterInsideString() {
        //    string sql = "DROP DATABASE test;INSERT INTO bukrs ('VALUE;')";
        //    List<string> actual = ViewInfo.SplitStatements(sql);
        //    Assert.AreEqual(2, actual.Count);
        //    Assert.AreEqual("DROP DATABASE test", actual[0]);
        //    Assert.AreEqual("INSERT INTO bukrs ('VALUE;')", actual[1]);
        //}

        //[TestMethod()]
        //public void SplitStatementsTest_DelimiterInsideStringWithStringsFollowing() {
        //    string sql = "DROP DATABASE test;INSERT INTO bukrs ('VALUE;', 'Normal)\r\n';";
        //    List<string> actual = ViewInfo.SplitStatements(sql);
        //    Assert.AreEqual(2, actual.Count);
        //    Assert.AreEqual("DROP DATABASE test", actual[0]);
        //    Assert.AreEqual("INSERT INTO bukrs ('VALUE;', 'Normal)\r\n'", actual[1]);
        //}

        //[TestMethod()]
        //public void SplitStatementsTest_DelimiterInsideComment() {
        //    string sql = "DROP DATABASE test;INSERT INTO bukrs ('VALUE') -- ; Problem";
        //    List<string> actual = ViewInfo.SplitStatements(sql);
        //    Assert.AreEqual(2, actual.Count);
        //    Assert.AreEqual("DROP DATABASE test", actual[0]);
        //    Assert.AreEqual("INSERT INTO bukrs ('VALUE') -- ; Problem", actual[1]);
        //}

        //[TestMethod()]
        //public void SplitStatementsTest_MultilineDelimiterInsideComment() {
        //    string sql = "INSERT INTO bukrs ('VALUE') -- ; Problem\r\n;DROP DATABASE test;\r\nDROP DATABASE test2;\r\n";
        //    List<string> actual = ViewInfo.SplitStatements(sql);
        //    Assert.AreEqual(3, actual.Count);
        //    Assert.AreEqual("INSERT INTO bukrs ('VALUE') -- ; Problem", actual[0]);
        //    Assert.AreEqual("DROP DATABASE test", actual[1]);
        //    Assert.AreEqual("DROP DATABASE test2", actual[2]);
        //}
    }
}
