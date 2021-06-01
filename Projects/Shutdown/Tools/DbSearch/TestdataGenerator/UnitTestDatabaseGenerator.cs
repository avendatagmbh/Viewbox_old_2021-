using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;

namespace TestdataGenerator {
    public class UnitTestDatabaseGenerator {
        #region CreateTables
        private string TableNameView = "viewtable1";
        public static string TableNameValidation = "querytable1";
        public static string DbName = "dbsearch_generated_data_unittest";

        public void Create() {
            DbConfig viewConfig = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", Password = "avendata" };
            DbConfig validationConfig = new DbConfig("Access") { Hostname = System.IO.Directory.GetCurrentDirectory() + "\\unittest_validation_database.mdb" };

            using (IDatabase connView = ConnectionManager.CreateConnection(viewConfig), 
                connValidation=ConnectionManager.CreateConnection(validationConfig)) {
                connView.Open();
                connView.CreateDatabaseIfNotExists(DbName);
                connView.ChangeDatabase(DbName);

                Generator.CreateDB(validationConfig.Hostname);
                connValidation.Open();
                CreateValidationTable(connValidation);
                CreateViewTable(connView);
                FillValidationTable(connValidation);
                FillViewTable(connView);
            }
        }



        private void FillViewTable(IDatabase connView) {
            DbColumnValues[] row = new DbColumnValues[4];
            for (int i = 0; i < 4; ++i) {
                row[i] = new DbColumnValues();
                row[i]["_row_no_"] = i+1;
            }

            
            row[0]["id"] = 1;
            row[0]["text"] = "test";
            row[0]["date"] = new DateTime(1986, 1, 1);
            row[0]["stringdate"] = "1.1.1986";
            row[0]["value"] = 100.11;

            row[1]["id"] = 3;
            row[1]["text"] = "123testxyz";
            row[1]["date"] = new DateTime(2011, 7, 5);
            row[1]["stringdate"] = "5.7.2011";
            row[1]["value"] = 123;

            row[2]["id"] = 4;
            row[2]["text"] = "different";
            row[2]["date"] = new DateTime(2010, 9, 3);
            row[2]["stringdate"] = "3.9.2010";
            row[2]["value"] = 100.12;

            row[3]["id"] = null;
            row[3]["text"] = null;
            row[3]["date"] = null;
            row[3]["stringdate"] = null;
            row[3]["value"] = null;

            connView.Delete(TableNameView, "");
            connView.InsertInto(TableNameView, row);
        }

        private void FillValidationTable(IDatabase connValidation) {
            DbColumnValues[] rows = new DbColumnValues[2];
            for (int i = 0; i < 2; ++i) {
                rows[i] = new DbColumnValues();
            }

            rows[0]["text_query"] = "test";
            rows[0]["date_search"] = new DateTime(2011, 7, 5);
            rows[0]["date_string"] = "5.7.2011";
            rows[0]["value_search"] = 123;
            rows[0]["value_int"] = 1;

            rows[1]["text_query"] = "notfound";
            rows[1]["date_search"] = new DateTime(1970, 1, 1);
            rows[1]["date_string"] = "1.1.1970";
            rows[1]["value_search"] = 100.1143;
            rows[1]["value_int"] = 4;

            connValidation.Delete(TableNameValidation, "");
            foreach(var row in rows)
                connValidation.InsertInto(TableNameValidation, row);
        }

        private void CreateValidationTable(IDatabase connValidation) {
            string sql = "CREATE TABLE " + TableNameValidation + "(" +
                            "text_query VARCHAR (255)," +
                            "date_search DATETIME," +
                            "date_string VARCHAR (40)," +
                            "value_search Double," +
                            "value_int INT" +
                            ");";
            connValidation.ExecuteNonQuery(sql);
        }

        public void CreateViewTable(IDatabase connView) {
            connView.DropTableIfExists(TableNameView);
            string sql = "CREATE TABLE " + TableNameView + "(" +
                            "_row_no_ INT," +
                            "id INT," +
                            "text VARCHAR(255)," +
                            "date DATETIME," +
                            "stringdate VARCHAR(255)," +
                            "value DECIMAL(10,2)" +
                            ");";
            connView.ExecuteNonQuery(sql);
        }
        #endregion
    }
}
