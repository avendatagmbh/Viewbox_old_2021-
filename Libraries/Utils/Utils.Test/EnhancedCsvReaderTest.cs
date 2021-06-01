using System.IO;
using Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Data;

namespace Utils.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "EnhancedCsvReaderTest" und soll
    ///alle EnhancedCsvReaderTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class EnhancedCsvReaderTest {


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
        ///Ein Test für "GetCsvData"
        ///</summary>
        [TestMethod()]
        public void GetCsvDataTest_MultiCharSeperator() {
            EnhancedCsvReader target = new EnhancedCsvReader("");
            string csv = "c1<<DIV>>c22<<EOL>>\r\n1_1<<DIV>>1_2<<EOL>>\r\n2_1<<DIV>>2_2<<EOL>>\r\n";
            
            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            target.Separator = "<<DIV>>";
            target.EndOfLine = "<<EOL>>\r\n";
            int limit = 5;
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(2, actual.Columns.Count);
            Assert.AreEqual(2, actual.Rows.Count);
            Assert.AreEqual("c1", actual.Columns[0].ColumnName);
            Assert.AreEqual("c22", actual.Columns[1].ColumnName);
            Assert.AreEqual("1_1", actual.Rows[0][0]);
            Assert.AreEqual("1_2", actual.Rows[0][1]);
            Assert.AreEqual("2_1", actual.Rows[1][0]);
            Assert.AreEqual("2_2", actual.Rows[1][1]);
        }

        [TestMethod()]
        public void GetCsvDataTest_Malformed() {
            EnhancedCsvReader target = new EnhancedCsvReader("");
            string csv = "c1<<DIV>>c22<<EOL>>\r\n1_1<<DIV>>1_2<<EOL>>\r\n2_1<<EOL>>\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            target.Separator = "<<DIV>>";
            target.EndOfLine = "<<EOL>>\r\n";
            int limit = 5;
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(1, actual.Columns.Count);
            Assert.AreEqual(3, actual.Rows.Count);
            Assert.AreEqual("Datenfehler (falsches Trennzeichen?)", actual.Columns[0].ColumnName);
        }
        //~CON==========================HTI<<DIV>>A<<DIV>>0<<DIV>><<DIV>>20091117<<DIV>>192041<<DIV>>0<<DIV>>0<<DIV>>20001113<<DIV>>151915<<DIV>>0<<DIV>>3<<DIV>><<DIV>>
        [TestMethod()]
        public void GetCsvDataTest_TwoEmptyFields() {
            EnhancedCsvReader target = new EnhancedCsvReader("");
            string csv = "c1<<DIV>>c22<<DIV>>c3<<EOL>>\r\n<<DIV>><<DIV>><<EOL>>\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            target.Separator = "<<DIV>>";
            target.EndOfLine = "<<EOL>>\r\n";
            int limit = 5;
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(3, actual.Columns.Count);
            Assert.AreEqual(1, actual.Rows.Count);
            Assert.AreEqual("c1", actual.Columns[0].ColumnName);
            Assert.AreEqual("c22", actual.Columns[1].ColumnName);
            Assert.AreEqual("c3", actual.Columns[2].ColumnName);
            Assert.AreEqual("", actual.Rows[0][0]);
            Assert.AreEqual("", actual.Rows[0][1]);
            Assert.AreEqual("", actual.Rows[0][2]);
        }

        [TestMethod()]
        public void GetCsvDataTest_FieldOfSpaces() {
            EnhancedCsvReader target = new EnhancedCsvReader("");
            string csv = "c1<<DIV>>c22<<EOL>>\r\n1_1<<DIV>>     <<EOL>>\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            target.Separator = "<<DIV>>";
            target.EndOfLine = "<<EOL>>\r\n";
            int limit = 5;
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(2, actual.Columns.Count);
            Assert.AreEqual(1, actual.Rows.Count);
            Assert.AreEqual("c1", actual.Columns[0].ColumnName);
            Assert.AreEqual("c22", actual.Columns[1].ColumnName);
            Assert.AreEqual("1_1", actual.Rows[0][0]);
            Assert.AreEqual("     ", actual.Rows[0][1]);
        }
    }
}
