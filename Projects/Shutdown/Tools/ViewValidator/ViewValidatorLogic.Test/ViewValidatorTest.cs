using Utils;
using ViewValidatorLogic.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ViewValidatorLogic.Structures.InitialSetup;
using System.ComponentModel;
using DbAccess.Structures;
using DbAccess;
using System.IO;
using ViewValidatorLogic.Structures.Results;
using System.Collections.Generic;

namespace ViewValidatorLogic.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ViewValidatorTest" und soll
    ///alle ViewValidatorTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ViewValidatorTest {
        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext { get; set; }

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

        const string dbName = "visual_studio_unit_test_database";
        const string tableNameView = "test_table_view", tableNameValidation = "test_table_validation";
        DbConfig ViewConfig = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", Password = "avendata", DbName = dbName};
        DbConfig ValidationConfig = new DbConfig("Access") { Hostname = "test_validation_database.mdb"};


        private void CreateMysqlDatabase() {
            DbConfig config = (DbConfig) ViewConfig.Clone();
            config.DbName = "";
            using (IDatabase conn = ConnectionManager.CreateConnection(config)) {
                conn.Open();
                conn.CreateDatabaseIfNotExists(dbName);
                conn.ChangeDatabase(dbName);

                conn.DropTableIfExists(tableNameView);
                conn.ExecuteNonQuery("CREATE TABLE " + tableNameView +
                    " ( " + 
                    "id INT," +
                    " name VARCHAR(255)" +
                    ");"

                    );
            }
        }

        public static void CreateDB(string pstrDB) {
            Stream objStream = null;
            FileStream objFileStream = null;
            try {
                byte[] abytResource;
                System.Reflection.Assembly objAssembly =
                System.Reflection.Assembly.GetExecutingAssembly();

                objStream = objAssembly.GetManifestResourceStream("ViewValidatorLogic.Test.EmptyDatabase.mdb");
                abytResource = new Byte[objStream.Length];
                objStream.Read(abytResource, 0, (int)objStream.Length);
                objFileStream = new FileStream(pstrDB,
                FileMode.Create);
                objFileStream.Write(abytResource, 0, (int)objStream.Length);
                objFileStream.Close();

            } catch (Exception) {
                throw;
            } finally {
                if (objFileStream != null) {
                    objFileStream.Close();
                    objFileStream = null;
                }
                if (objStream != null) {
                    objStream = null;
                }
            }
        }

        private void CreateAccessDatabase() {
            //Assert.Inconclusive(System.IO.Directory.GetCurrentDirectory());
            System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory());
            CreateDB(ValidationConfig.Hostname);

            //using (StreamWriter writer = new StreamWriter(ValidationConfig.Hostname)) { }
            using (IDatabase conn = ConnectionManager.CreateConnection(ValidationConfig)) {
                conn.Open();
                conn.ExecuteNonQuery("CREATE TABLE " + tableNameValidation +
                    " ( " +
                    "id INT," +
                    " name VARCHAR(255)" +
                    ");"

                    );
            }

        }

        private void InsertData(IDatabase conn, string tableName, int? id, string name) {
            DbColumnValues values = new DbColumnValues();
            values["id"] = id;
            values["name"] = name;
            conn.InsertInto(tableName, values);
        }

        ValidationSetup CreateSetup() {
            ValidationSetup setup = new ValidationSetup();
            setup.DbConfigValidation = ValidationConfig;
            setup.DbConfigView = ViewConfig;

            TableMapping tableMapping = new TableMapping();
            tableMapping.TableValidation = new Table(tableNameValidation, ValidationConfig);
            //tableMapping.TableValidation.KeyEntriesColumnNames.Add("id");

            tableMapping.TableView = new Table(tableNameView, ViewConfig);
            //tableMapping.TableView.KeyEntriesColumnNames.Add("id");
            tableMapping.ColumnMappings.Add(new ColumnMapping("id", "id"));
            tableMapping.ColumnMappings.Add(new ColumnMapping("name", "name"));
            tableMapping.KeyEntryMappings.Add(new ColumnMapping("id", "id"));

            setup.TableMappings.Add(tableMapping);
            setup.ErrorLimit = 100;
            return setup;
        }

        public class Entry {
            public int? Id { get; set; }
            public string Name { get; set; }

            public Entry(int? id, string name) {
                this.Id = id;
                this.Name = name;
            }
        }

        private void Init(List<Entry> entriesValidation, List<Entry> entriesView) {
            CreateMysqlDatabase();
            CreateAccessDatabase();
            using (IDatabase connValidation = ConnectionManager.CreateConnection(ValidationConfig),
                connView = ConnectionManager.CreateConnection(ViewConfig)) {

                connValidation.Open();
                connView.Open();

                foreach(var entry in entriesValidation)
                    InsertData(connValidation, tableNameValidation, entry.Id, entry.Name);

                foreach (var entry in entriesView)
                    InsertData(connView, tableNameView, entry.Id, entry.Name);
            }

        }

        private TableValidationResult GetResults(SortMethod sortMethod) {
            using (IDatabase connValidation = ConnectionManager.CreateConnection(ValidationConfig),
                connView = ConnectionManager.CreateConnection(ViewConfig)) {
                ValidationSetup setup = CreateSetup();

                ViewValidator target = new ViewValidator(setup, new ProgressCalculator());
                target.StartValidation(sortMethod);
                return target.Results[setup.TableMappings[0]];
            }

        }

        private void CheckResults(ExpectedResults expected) {
            foreach(SortMethod method in Enum.GetValues(typeof(SortMethod))) {
                TableValidationResult result = GetResults(method);

                string pretext = "Sortiermethod: " + method.ToString() + " - ";

                Assert.AreEqual<int>(expected.RowDifferenceCount, result.RowDifferenceCount, pretext + "RowDifferenceCount ist falsch");
                Assert.AreEqual<int>(expected.RowsEqual, result.RowsEqual, pretext + "RowsEqual ist falsch");
                Assert.AreEqual<int>(expected.ValidationMissingRowsCount, result.ResultValidationTable.MissingRowsCount, pretext + "MissingRows Validation ist falsch");
                Assert.AreEqual<int>(expected.ViewMissingRowsCount, result.ResultViewTable.MissingRowsCount, pretext + "MissingRows View ist falsch");
            }
        }

        private class ExpectedResults {
            public ExpectedResults() {
                RowsEqual = RowDifferenceCount = ValidationMissingRowsCount = ViewMissingRowsCount = -1;
            }

            public int RowDifferenceCount { get; set; }
            public int RowsEqual { get; set; }
            public int ValidationMissingRowsCount { get; set; }
            public int ViewMissingRowsCount { get; set; }
        }


        /* The following special cases are tested:
         * 1. Both tables are empty
         * 2. One table is empty
         * 3. LastComparison result is 0, then one reader has no more rows (while the other has)
         * 4. LastComparison result is 0, both readers have no more rows
         * 5. LastComparison result is not 0, then one reader has no more rows (while the other has)
         * 6. LastComparison result is not 0, both readers have no more rows
        */

        #region Test 1
        [TestMethod()]
        public void StartValidationTest_1() {
            List<Entry> entriesValidation = new List<Entry>(){
            };
            List<Entry> entriesView = new List<Entry>(){
            };
            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 0,
                RowsEqual = 0,
                ValidationMissingRowsCount = 0,
                ViewMissingRowsCount = 0
            };
            CheckResults(expected);
            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(0, result.RowDifferenceCount);
            //Assert.AreEqual<int>(0, result.RowsEqual);
            //Assert.AreEqual<int>(0, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(0, result.ResultViewTable.MissingRowsCount);
        }
        #endregion

        #region Test 2
        [TestMethod()]
        public void StartValidationTest_2() {
            List<Entry> entriesValidation = new List<Entry>() {
                new Entry(1, "Herbert"),
            };
            List<Entry> entriesView = new List<Entry>() {
            };

            for (int i = 0; i < 2; ++i) {
                if(i == 0) Init(entriesValidation, entriesView);
                else Init(entriesView, entriesValidation);

                ExpectedResults expected = new ExpectedResults() {
                    RowDifferenceCount = 0,
                    RowsEqual = 0,
                    ValidationMissingRowsCount = i,
                    ViewMissingRowsCount = 1 - i
                };
                CheckResults(expected);
                //TableValidationResult result = GetResults();
                //Assert.AreEqual<int>(0, result.RowDifferenceCount);
                //Assert.AreEqual<int>(0, result.RowsEqual);
                //Assert.AreEqual<int>(i, result.ResultValidationTable.MissingRowsCount);
                //Assert.AreEqual<int>(1-i, result.ResultViewTable.MissingRowsCount);
            }
        }
        #endregion

        #region Test 3
        [TestMethod()]
        public void StartValidationTest_3() {
            List<Entry> entriesValidation = new List<Entry>() {
                new Entry(1, "Herbert"),
            };
            List<Entry> entriesView = new List<Entry>() {
                new Entry(1, "Herbert"),
                new Entry(2, "Herbert"),
            };

            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 0,
                RowsEqual = 1,
                ValidationMissingRowsCount = 1,
                ViewMissingRowsCount = 0
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(0, result.RowDifferenceCount);
            //Assert.AreEqual<int>(1, result.RowsEqual);
            //Assert.AreEqual<int>(1, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(0, result.ResultViewTable.MissingRowsCount);
        }
        #endregion

        #region Test 4
        [TestMethod()]
        public void StartValidationTest_4() {
            List<Entry> entriesValidation = new List<Entry>() {
                new Entry(1, "Herbert"),
            };
            List<Entry> entriesView = new List<Entry>() {
                new Entry(1, "Herbert"),
            };

            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 0,
                RowsEqual = 1,
                ValidationMissingRowsCount = 0,
                ViewMissingRowsCount = 0
            };
            CheckResults(expected);


            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(0, result.RowDifferenceCount);
            //Assert.AreEqual<int>(1, result.RowsEqual);
            //Assert.AreEqual<int>(0, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(0, result.ResultViewTable.MissingRowsCount);
        }
        #endregion

        #region Test 5
        [TestMethod()]
        public void StartValidationTest_5() {
            List<Entry> entriesValidation = new List<Entry>() {
                new Entry(0, "Herbert"),
            };
            List<Entry> entriesView = new List<Entry>() {
                new Entry(1, "Herbert"),
                new Entry(2, "Herbert2"),
            };

            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 0,
                RowsEqual = 0,
                ValidationMissingRowsCount = 2,
                ViewMissingRowsCount = 1
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(0, result.RowDifferenceCount);
            //Assert.AreEqual<int>(0, result.RowsEqual);
            //Assert.AreEqual<int>(2, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(1, result.ResultViewTable.MissingRowsCount);
        }
        #endregion

        #region Test 6
        [TestMethod()]
        public void StartValidationTest_6() {
            List<Entry> entriesValidation = new List<Entry>() {
                new Entry(0, "Herbert"),
            };
            List<Entry> entriesView = new List<Entry>() {
                new Entry(1, "Herbert"),
            };

            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 0,
                RowsEqual = 0,
                ValidationMissingRowsCount = 1,
                ViewMissingRowsCount = 1
            };
            CheckResults(expected);


            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(0, result.RowDifferenceCount);
            //Assert.AreEqual<int>(0, result.RowsEqual);
            //Assert.AreEqual<int>(1, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(1, result.ResultViewTable.MissingRowsCount);
        }
        #endregion

        /// <summary>
        ///Ein Test für "StartValidation"
        ///</summary>
        [TestMethod()]
        public void StartValidationTest_7() {
            List<Entry> entriesValidation = new List<Entry>(){
                new Entry(1, "Herbert"),
                new Entry(3, "Albert"),
                new Entry(4, "Romeo")
            };
            List<Entry> entriesView = new List<Entry>(){
                new Entry(2, "InViewNotInValidation"),
                new Entry(3, "Albert"),
                new Entry(4, "Wrong Row")
            };
            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 1,
                RowsEqual = 1,
                ValidationMissingRowsCount = 1,
                ViewMissingRowsCount = 1
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(1, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(1, result.ResultViewTable.MissingRowsCount);
            //Assert.AreEqual<int>(1, result.RowsEqual);
            //Assert.AreEqual<int>(1, result.RowDifferenceCount);
        }

        [TestMethod()]
        public void StartValidationTest_8() {
            List<Entry> entriesValidation = new List<Entry>(){
                new Entry(5, "Missing1"),
                new Entry(5, "Missing2"),
                new Entry(1, "Missing0"),
                new Entry(3, "Albert"),
                new Entry(4, "Romeo"),
                
            };
            List<Entry> entriesView = new List<Entry>(){
                new Entry(2, "InViewNotInValidation"),
                new Entry(3, "Albert"),
                new Entry(4, "Wrong Row")
            };
            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 1,
                RowsEqual = 1,
                ValidationMissingRowsCount = 1,
                ViewMissingRowsCount = 3
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(1, result.RowDifferenceCount);
            //Assert.AreEqual<int>(1, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(3, result.ResultViewTable.MissingRowsCount);
        }

        [TestMethod()]
        public void StartValidationTest_9() {
            List<Entry> entriesValidation = new List<Entry>(){
                new Entry(1, "Same"),
                new Entry(null, "Missing2"),
                new Entry(5, "Different"),
                
            };
            List<Entry> entriesView = new List<Entry>(){
                new Entry(1, "Same"),
                new Entry(5, "Wrong Row")
            };
            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 1,
                RowsEqual = 1,
                ValidationMissingRowsCount = 0,
                ViewMissingRowsCount = 1
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(1, result.RowDifferenceCount);
            
            //Assert.AreEqual<int>(0, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(1, result.ResultViewTable.MissingRowsCount);
        }

        [TestMethod()]
        public void StartValidationTest_10() {
            List<Entry> entriesValidation = new List<Entry>(){
                new Entry(1, "Same"),
                new Entry(5, "Wrong Row"),
                new Entry(null, "Different2"),
            };

            List<Entry> entriesView = new List<Entry>(){
                new Entry(1, "Same"),
                new Entry(null, "Missing2"),
                new Entry(5, "Different"),
                
            };
            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 2,
                RowsEqual = 1,
                ValidationMissingRowsCount = 0,
                ViewMissingRowsCount = 0
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(2, result.RowDifferenceCount);

            //Assert.AreEqual<int>(0, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(0, result.ResultViewTable.MissingRowsCount);
        }

        //Tests capability of sorting
        [TestMethod()]
        public void StartValidationTest_11() {
            List<Entry> entriesValidation = new List<Entry>(){
                new Entry(5, "Wrong Row"),
                new Entry(null, "Different2"),
                new Entry(1, "Same"),
            };

            List<Entry> entriesView = new List<Entry>(){
                new Entry(1, "Same"),
                new Entry(null, "Missing2"),
                new Entry(5, "Different"),
                
            };
            Init(entriesValidation, entriesView);

            ExpectedResults expected = new ExpectedResults() {
                RowDifferenceCount = 2,
                RowsEqual = 1,
                ValidationMissingRowsCount = 0,
                ViewMissingRowsCount = 0
            };
            CheckResults(expected);

            //TableValidationResult result = GetResults();
            //Assert.AreEqual<int>(2, result.RowDifferenceCount);

            //Assert.AreEqual<int>(0, result.ResultValidationTable.MissingRowsCount);
            //Assert.AreEqual<int>(0, result.ResultViewTable.MissingRowsCount);
        }
    }
}
