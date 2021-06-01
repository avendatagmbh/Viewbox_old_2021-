using System.IO;
using Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Data;

namespace Utils.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "CsvReaderTest" und soll
    ///alle CsvReaderTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class CsvReaderTest {


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
        public void GetCsvDataTest_Malformed() {
            CsvReader target = new CsvReader("");
            string csv = "c1;c22\r\n1_1;1_2\r\n2_1\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            int limit = 5;
            target.Separator = ';';
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(1, actual.Columns.Count);
            Assert.AreEqual(3, actual.Rows.Count);
            Assert.AreEqual("Datenfehler (falsches Trennzeichen?)", actual.Columns[0].ColumnName);
        }

        [TestMethod()]
        public void GetCsvDataTest_LastColumnHeaderMissing() {
            CsvReader target = new CsvReader("");
            string csv = "c1;\r\n1_1;1_2\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            int limit = 5;
            target.Separator = ';';
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(2, actual.Columns.Count);
            Assert.AreEqual(1, actual.Rows.Count);
        }

        [TestMethod()]
        public void GetCsvDataTest_NoHeadLinesLastValueEmpty() {
            CsvReader target = new CsvReader("");
            string csv = "1_1;\r\n2_1;2_2\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            int limit = 5;
            target.Separator = ';';
            target.HeadlineInFirstRow = false;
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(2, actual.Columns.Count);
            Assert.AreEqual(2, actual.Rows.Count);
            Assert.AreEqual("1_1", actual.Rows[0][0]);
            Assert.AreEqual("", actual.Rows[0][1]);
            Assert.AreEqual("2_1", actual.Rows[1][0]);
            Assert.AreEqual("2_2", actual.Rows[1][1]);
        }

        [TestMethod()]
        public void GetCsvDataTest_TwoEmptyFields() {
            CsvReader target = new CsvReader("");
            string csv = "c1;c22;c3\r\n;;\r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            target.Separator = ';';
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
            CsvReader target = new CsvReader("");
            string csv = "c1;c22\r\n1_1;     \r\n";

            target.CreateReader = (filename, encoding1) => new StringReader(csv);
            target.Separator = ';';
            int limit = 5;
            DataTable actual = target.GetCsvData(limit, Encoding.UTF8);
            Assert.AreEqual(2, actual.Columns.Count);
            Assert.AreEqual(1, actual.Rows.Count);
            Assert.AreEqual("c1", actual.Columns[0].ColumnName);
            Assert.AreEqual("c22", actual.Columns[1].ColumnName);
            Assert.AreEqual("1_1", actual.Rows[0][0]);
            Assert.AreEqual("", actual.Rows[0][1]);
        }
    }
}
