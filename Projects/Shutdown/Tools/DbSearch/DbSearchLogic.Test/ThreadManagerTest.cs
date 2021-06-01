// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.Config;
using DbSearchLogic.Manager;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.SearchCore.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DbSearchLogic.Structures.TableRelated;
using TestdataGenerator;

namespace DbSearchLogic.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ThreadManagerTest" und soll
    ///alle ThreadManagerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ThreadManagerTest {


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


        class ExpectedResult {
            public string QueryColumn { get; set; }
            public string ViewColumn { get; set; }
            public string ViewTable { get; set; }
            public int Hits { get; set; }
            public bool WasFound { get; set; }

            public override string ToString() {
                return "query: " + QueryColumn + ", ViewColumn: " + ViewColumn + ", Hits: " + Hits;
            }
        }

        /// <summary>
        /// You need to execute TestdataGenerator.UnitTestDatabaseGenerator and copy the access database
        ///</summary>
        //[TestMethod()]
        //public void StartNewTableSearchTest() {
        //    Profile profile = ProfileManager.CreateNewProfile();
        //    profile.Name = "unit_test_profile";
        //    profile.DbConfigView.DbName = UnitTestDatabaseGenerator.DbName;

        //    Directory.CreateDirectory(Directory.GetCurrentDirectory());
        //    //profile.Path = System.IO.Directory.GetCurrentDirectory();

        //    ProfileManager.AddProfile(profile, false);
        //    /*profile.DbConfigValidation.Hostname = Directory.GetCurrentDirectory() + "\\..\\..\\unittest_validation_database.mdb";
        //    Assert.IsTrue(new FileInfo(profile.DbConfigValidation.Hostname).Exists, 
        //        "Die Access Datenbank konnte nicht gefunden werden, Pfad: " + profile.DbConfigValidation.Hostname);*/

        //    string accessDb = Directory.GetCurrentDirectory() + "\\..\\..\\unittest_validation_database.mdb";
        //    List<ImportTable> tables = new List<ImportTable>();
        //    using (IDatabase conn = ConnectionManager.CreateConnection(new DbConfig("Access"){Hostname = accessDb})) {
        //        conn.Open();
        //        foreach(var table in conn.GetTableList())
        //            tables.Add(new ImportTable(table,accessDb));
                    
        //    }
            
        //    Queries queries = new Queries(profile);
        //    queries.AddFromValidationDatabase(tables);
        //    foreach(var table in queries.Items)
        //        table.LoadData(2);

        //    Query query = queries.FindQuery(UnitTestDatabaseGenerator.TableNameValidation);
        //    Assert.IsNotNull(query, "Query Tabelle konnte nicht gefunden werden.");

        //    //Set value_int columns "search as string" to true
        //    query.Columns[4].SearchParams.StringSearch = true;
        //    query.Columns[4].SearchParams.InStringSearch = true;

        //    ThreadManager.StartNewTableSearch(query).Wait();
        //    while (ThreadManager.RunningThreads.Count > 0) {
        //        ThreadManager.RunningThreads[0].Task.Wait();
        //        Thread.Sleep(500);
        //    }

        //    //Results when inString search is active
        //    List<ExpectedResult> expectedResult =
        //        new List<ExpectedResult>() {
        //            new ExpectedResult(){QueryColumn = "text_query", ViewTable = "viewtable1", ViewColumn = "text", Hits = 1},
        //            new ExpectedResult(){QueryColumn = "date_search", ViewTable = "viewtable1", ViewColumn = "date", Hits = 1},
        //            //TODO: Need to check this
        //            //new ExpectedResult(){QueryColumn = "date_search", ViewTable = "viewtable1", ViewColumn = "stringdate", Hits = 0},
        //            new ExpectedResult(){QueryColumn = "date_string", ViewTable = "viewtable1", ViewColumn = "date", Hits = 1},
        //            new ExpectedResult(){QueryColumn = "date_string", ViewTable = "viewtable1", ViewColumn = "stringdate", Hits = 1},
        //            //No InString Search for int numeric datatype by default
        //            //new ExpectedResult(){QueryColumn = "value_search", ViewTable = "viewtable1", ViewColumn = "text", Hits = 1},
        //            new ExpectedResult(){QueryColumn = "value_search", ViewTable = "viewtable1", ViewColumn = "value", Hits = 1}, //only 1 Hit if round values is false
        //            new ExpectedResult(){QueryColumn = "value_int", ViewTable = "viewtable1", ViewColumn = "id", Hits = 2},
        //            new ExpectedResult(){QueryColumn = "value_int", ViewTable = "viewtable1", ViewColumn = "text", Hits = 1}, //Due to instring search
        //            new ExpectedResult(){QueryColumn = "value_int", ViewTable = "viewtable1", ViewColumn = "stringdate", Hits = 1}, //Due to instring search
        //            new ExpectedResult(){QueryColumn = "value_int", ViewTable = "viewtable1", ViewColumn = "value", Hits = 1}, //Due to instring search
        //            new ExpectedResult(){QueryColumn = "value_int", ViewTable = "viewtable1", ViewColumn = "date", Hits = 1}, //Due to instring search
        //        };

        //    Assert.AreEqual(1, query.ResultHistory.Results.Count);
        //    ResultSet resultSet = query.ResultHistory.Results[0];

        //    //Add all column hit infos to a hashset in order to check later if every expected hitinfo was removed from this list
        //    HashSet<ColumnHitInfo> allHitInfos = new HashSet<ColumnHitInfo>();
        //    foreach (var columnResult in resultSet.ColumnResults) {
        //        foreach (var columnHit in columnResult.ColumnHits)
        //                allHitInfos.Add(columnHit);
        //    }

        //    //ResultSet results = query.ResultSet;
        //    foreach (var columnResult in resultSet.ColumnResults) {
        //        foreach (var expected in expectedResult) {
        //            if (columnResult.Column.Name == expected.QueryColumn) {
        //                foreach (var columnHit in columnResult.ColumnHits) {
        //                    if (columnHit.TableInfo.Name == expected.ViewTable) {
        //                        if (columnHit.ColumnName == expected.ViewColumn) {
        //                            Assert.AreEqual(expected.Hits, columnHit.HitCount);
        //                            expected.WasFound = true;
        //                            allHitInfos.Remove(columnHit);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
            

        //    foreach(var expected in expectedResult)
        //        Assert.AreEqual(expected.WasFound, true, "Fehlendes Ergebnis: " + expected);

        //    foreach (var hitInfo in allHitInfos)
        //        Assert.Fail("Es wurde mindestens eine Spalte mehr gefunden als erwartet. " +
        //                                                                    //"Suchspalte: " + hitInfo.ColumnHit.SearchColumnName +
        //                                                                    " ViewSpalte: " + hitInfo.ColumnName);
        //}
    }
}
