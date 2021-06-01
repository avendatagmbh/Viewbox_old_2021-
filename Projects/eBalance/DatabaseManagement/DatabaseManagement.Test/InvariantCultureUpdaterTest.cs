using System.Globalization;
using System.Threading;
using DatabaseManagement.DbUpgrade;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DbAccess;

namespace DatabaseManagement.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "InvariantCultureUpdaterTest" und soll
    ///alle InvariantCultureUpdaterTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class InvariantCultureUpdaterTest {


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
        public void ChangeValueTest_PositiveNumber() {
            InvariantCultureUpdater target = new InvariantCultureUpdater(conn:null);
            string value = "123,56";
            string newValue = null;
            string newValueExpected = "123.56";
            bool actual = false;
            actual = target.ChangeValue(value, out newValue);
            Assert.AreEqual(newValueExpected, newValue);
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        public void ChangeValueTest_PositiveNumberDifferentCulture() {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            InvariantCultureUpdater target = new InvariantCultureUpdater(conn: null);
            string value = "123,56";
            string newValue = null;
            string newValueExpected = "123.56";
            bool actual = false;
            actual = target.ChangeValue(value, out newValue);
            Assert.AreEqual(newValueExpected, newValue);
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        public void ChangeValueTest_NegativeNumber() {
            InvariantCultureUpdater target = new InvariantCultureUpdater(conn: null);
            string value = "-123,56";
            string newValue = null;
            string newValueExpected = "-123.56";
            bool actual = false;
            actual = target.ChangeValue(value, out newValue);
            Assert.AreEqual(newValueExpected, newValue);
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        public void ChangeValueTest_NaturalNumber() {
            InvariantCultureUpdater target = new InvariantCultureUpdater(conn: null);
            string value = "-123";
            string newValue = null;
            string newValueExpected = "-123";
            bool actual = false;
            actual = target.ChangeValue(value, out newValue);
            Assert.AreEqual(newValueExpected, newValue);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        public void ChangeValueTest_NumberInXml() {
            InvariantCultureUpdater target = new InvariantCultureUpdater(conn: null);
            string value = "<c ref=123,56 />";
            string newValue = null;
            string newValueExpected = value;
            bool actual = false;
            actual = target.ChangeValue(value, out newValue);
            Assert.AreEqual(newValueExpected, newValue);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        public void ChangeValueTest_GermanDate() {
            InvariantCultureUpdater target = new InvariantCultureUpdater(conn: null);
            string value = "05.02.2012 00:00:00";
            string newValue = null;
            string newValueExpected = "02/05/2012 00:00:00";
            bool actual = false;
            actual = target.ChangeValue(value, out newValue);
            Assert.AreEqual(newValueExpected, newValue);
            Assert.AreEqual(true, actual);
        }
    }
}
