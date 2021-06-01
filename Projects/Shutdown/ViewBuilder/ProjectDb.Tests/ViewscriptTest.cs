using ProjectDb.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ProjectDb.Tests
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ViewscriptTest" und soll
    ///alle ViewscriptTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ViewscriptTest {


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
        ///Ein Test für "DurationDisplayString"
        ///</summary>
        [TestMethod()]
        public void DurationDisplayString_1day() {
            Viewscript target = new Viewscript();
            target.Duration = new TimeSpan(1, 0,0,0);
            string actual = target.DurationDisplayString;
            Assert.AreEqual("1d 0h 0m 0s",actual);
        }

        [TestMethod()]
        public void DurationDisplayString_1hour() {
            Viewscript target = new Viewscript();
            target.Duration = new TimeSpan(0, 1, 0, 0);
            string actual = target.DurationDisplayString;
            Assert.AreEqual("1h 0m 0s", actual);
        }

        [TestMethod()]
        public void DurationDisplayString_1minute() {
            Viewscript target = new Viewscript();
            target.Duration = new TimeSpan(0, 0, 1, 0);
            string actual = target.DurationDisplayString;
            Assert.AreEqual("1m 0s", actual);
        }

        [TestMethod()]
        public void DurationDisplayString_1second() {
            Viewscript target = new Viewscript();
            target.Duration = new TimeSpan(0, 0, 0, 1);
            string actual = target.DurationDisplayString;
            Assert.AreEqual("1s", actual);
        }

        [TestMethod()]
        public void DurationDisplayString_mixed() {
            Viewscript target = new Viewscript();
            target.Duration = new TimeSpan(5, 11, 59, 1);
            string actual = target.DurationDisplayString;
            Assert.AreEqual("5d 11h 59m 1s", actual);
        }
    }
}
