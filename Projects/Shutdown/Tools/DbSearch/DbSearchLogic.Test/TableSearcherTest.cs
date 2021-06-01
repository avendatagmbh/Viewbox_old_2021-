using DbAccess.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbAccess;
using TestdataGenerator;

namespace DbSearchLogic.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "TableSearcherTest" und soll
    ///alle TableSearcherTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class TableSearcherTest {


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

        DbConfig ViewConfig = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", Password = "avendata", DbName = Generator.DbName };

        private void Init() {
            DbConfig viewConfigWithoutDatabaseName = (DbConfig) ViewConfig.Clone();
            viewConfigWithoutDatabaseName.DbName = string.Empty;

            using (IDatabase connView = ConnectionManager.CreateConnection(viewConfigWithoutDatabaseName)) {
                connView.Open();
                if (!connView.DatabaseExists(ViewConfig.DbName))
                    Assert.Fail("First run Testdata Generator, as no MySQL Database is present.");
            }
        }

        //private SearchValueMatrix CreateSearchValueMatrix(string table) {
        //    DbConfig validationConfig = new DbConfig("Access") { Hostname = System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\test_validation_database.mdb"};
        //    SearchValueMatrix matrix;
            
        //    using (IDatabase conn = ConnectionManager.CreateConnection(validationConfig)) {
        //        conn.Open();
        //        using (DbDataReader reader = conn.ExecuteReader("SELECT * FROM " + conn.Enquote(table))) {
        //            List<object[] > values = new List<object[]>();
        //            //columns = reader.FieldCount;
        //            while (reader.Read()) {
        //                object[] row = new object[reader.FieldCount];
        //                for (int i = 0; i < reader.FieldCount; ++i)
        //                    row[i] = reader[i];
        //                values.Add(row);
        //            }
        //            matrix = new SearchValueMatrix(values.Count, reader.FieldCount);
        //            for (int i = 0; i < values.Count; ++i) {
        //                for (int j = 0; j < values[i].Length; ++j)
        //                    matrix.Values[i, j] = new SearchValue(values[i][j].ToString(), CultureInfo.InvariantCulture, TODO);
        //            }
        //            matrix.InitTypes();
        //            return matrix;
        //        }
        //    }
            

        //}

        /// <summary>
        ///Ein Test für "SearchTable"
        ///</summary>
        [TestMethod()]
        public void SearchTableTest() {
            Init();
            
            //SearchValueMatrix matrix = new SearchValueMatrix(1, 1);
            //SearchValueMatrix matrix = CreateSearchValueMatrix("table10");
            ////matrix.Values[0, 0] = new SearchValue("CPRNZULRAJJICBRJVSSXDMVUVKJQPLGVGSHYDYGJUVEKXVKSFZNVBVHQTWSPLVCODNCJUSGWLIDIHLWAZQLQYKTKLRFGFRYZTPHJTBYM", CultureInfo.CurrentCulture);
            
            //QueryInfo info = new QueryInfo(
            //    new QueryConfig(ViewConfig, "table100",  matrix),
            //    new DbSearch_ProfileDb(ViewConfig, System.Guid.NewGuid().ToString()), 
            //    false
            //    );

            //ThreadManager.StartNewQuery(info);
            //using (IDatabase connView = ConnectionManager.CreateConnection(ViewConfig)) {
            //    connView.Open();
            //    TableResultSet actual = TableSearcher.SearchTable("table50", connView, Info);
            //}
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "SearchTableColumn"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DbSearchLogic.dll")]
        public void SearchTableColumnTest() {
            //string tableName = string.Empty; // TODO: Passenden Wert initialisieren
            //TableColumn tableColumn = null; // TODO: Passenden Wert initialisieren
            //IDatabase db = null; // TODO: Passenden Wert initialisieren
            //SearchValueMatrix svm = null; // TODO: Passenden Wert initialisieren
            //QueryInfo info = null; // TODO: Passenden Wert initialisieren
            //List<ColumnHit> columnHits = null; // TODO: Passenden Wert initialisieren
            //TableSearcher_Accessor.SearchTableColumn(tableName, tableColumn, db, svm, info, columnHits);
            //Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
        }
    }
}
