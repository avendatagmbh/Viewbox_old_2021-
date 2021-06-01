using System.IO;
using DbSearchLogic.Structures.TableRelated;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DbSearchLogic.SearchCore.Structures.Db;
using System.Xml;

namespace DbSearchLogic.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "SearchTableDeciderTest" und soll
    ///alle SearchTableDeciderTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class SearchTableDeciderTest {


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
        public void IsTableAllowedTest_WhitelistAllowed() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("", "allowed1;allowed");
            TableInfo table = new TableInfo("allowed", 0);
            
            Assert.AreEqual(true, target.IsTableAllowed(table));
        }

        [TestMethod()]
        public void IsTableAllowedTest_BlacklistAllowed() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("notallowed","");
            TableInfo table = new TableInfo("allowed", 0);

            Assert.AreEqual(true, target.IsTableAllowed(table));
        }

        [TestMethod()]
        public void IsTableAllowedTest_BlacklistNotAllowed() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("notallowed", "");
            TableInfo table = new TableInfo("notallowed", 0);

            Assert.AreEqual(false, target.IsTableAllowed(table));
        }

        [TestMethod()]
        public void IsTableAllowedTest_WhitelistNotAllowed() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("", "allowed1;allowed");
            TableInfo table = new TableInfo("notallowed", 0);

            Assert.AreEqual(false, target.IsTableAllowed(table));
        }

        [TestMethod()]
        public void ToXmlTest_NoTables() {
            SearchTableDecider target = new SearchTableDecider();
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter writer = new XmlTextWriter(stringWriter)) {
                target.ToXml(writer);
            }
            string actual = stringWriter.ToString();
            Assert.AreEqual("<STD><BL /><WL /></STD>", actual);
        }

        [TestMethod()]
        public void ToXmlTest_BlacklisteTables() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("t1;t2", "");
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter writer = new XmlTextWriter(stringWriter)) {
                target.ToXml(writer);
            }
            string actual = stringWriter.ToString();
            //<STD><BL><T N="t1" /><T N="t2" /></BL><WL /></STD>
            Assert.AreEqual("<STD><BL><T N=\"t1\" /><T N=\"t2\" /></BL><WL /></STD>", actual);
        }

        [TestMethod()]
        public void ToXmlTest_WhitelisteTables() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("","t1;t2");
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter writer = new XmlTextWriter(stringWriter)) {
                target.ToXml(writer);
            }
            string actual = stringWriter.ToString();
            //<STD><BL><T N="t1" /><T N="t2" /></BL><WL /></STD>
            Assert.AreEqual("<STD><BL /><WL><T N=\"t1\" /><T N=\"t2\" /></WL></STD>", actual);
        }

        [TestMethod()]
        public void ToXmlTest_MixedTables() {
            SearchTableDecider target = new SearchTableDecider();
            target.FromString("t3;t4", "t1;t2");
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter writer = new XmlTextWriter(stringWriter)) {
                target.ToXml(writer);
            }
            string actual = stringWriter.ToString();
            //<STD><BL><T N="t1" /><T N="t2" /></BL><WL /></STD>
            Assert.AreEqual("<STD><BL><T N=\"t3\" /><T N=\"t4\" /></BL><WL><T N=\"t1\" /><T N=\"t2\" /></WL></STD>", actual);
        }
    }
}
