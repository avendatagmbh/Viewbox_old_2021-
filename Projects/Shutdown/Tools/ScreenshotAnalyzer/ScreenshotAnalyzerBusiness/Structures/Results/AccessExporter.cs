// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AvdCommon.DataGridHelper;
using DbAccess;
using DbAccess.Structures;
using ScreenshotAnalyzerBusiness.Structures.Config;

namespace ScreenshotAnalyzerBusiness.Structures.Results {
    public static class AccessExporter {
        public static void Export(string path, Table table) {

            DbConfig dbConfig = new DbConfig("Access"){Hostname = path};
            DataTable result = table.RecognitionResult.TextTable;
            HashSet<string> columnNames = new HashSet<string>();
            for (int i = 0; i < result.Columns.Count; ++i) {
                string columnName = (result.Columns[i].Name);
                if (string.IsNullOrEmpty(ReplaceIllegalChars(columnName)))
                    throw new Exception(string.Format("Die Spalte \"{0}\" an Position {1} ist nach dem Entfernen von nicht in der Datenbank benutzbaren Zeichen leer. Bitte geben Sie einen gültigen Spaltennamen ein.", result.Columns[i].Name, i+1));
                while(columnNames.Contains(columnName)) {
                    columnName += "_1";
                }
                columnNames.Add(columnName);
                result.Columns[i].Name = columnName;
            }
            if(!new FileInfo(path).Exists)
                CreateDB(path);
            using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                //conn.CreateDatabaseIfNotExists("test");
                conn.DropTableIfExists(ReplaceIllegalChars(table.TableName));
                CreateTable(table, result, conn);
                foreach (var row in result.Rows) {
                    DbColumnValues rowValues = new DbColumnValues();
                    for (int i = 0; i < result.Columns.Count; ++i) {
                        rowValues[ReplaceIllegalChars(result.Columns[i].Name)] = ((ResultRowEntry) row[i]).EditedText;
                        //rowValues[""] = ((ResultRowEntry)row[i]).EditedText;
                    }
                    conn.InsertInto(ReplaceIllegalChars(table.TableName), rowValues);
                }
                
            }
        }


        #region CreateDB 
        public static void CreateDB(string path) {
            Stream objStream = null;
            FileStream objFileStream = null;
            try {
                byte[] abytResource;
                System.Reflection.Assembly objAssembly =
                System.Reflection.Assembly.GetExecutingAssembly();

                objStream = objAssembly.GetManifestResourceStream("ScreenshotAnalyzerBusiness.EmptyDatabase.mdb");
                abytResource = new Byte[objStream.Length];
                objStream.Read(abytResource, 0, (int)objStream.Length);
                objFileStream = new FileStream(path,
                FileMode.Create);
                objFileStream.Write(abytResource, 0, (int)objStream.Length);
                objFileStream.Close();

            } finally {
                if (objFileStream != null) {
                    objFileStream.Close();
                }
                if (objStream != null) {
                    objStream = null;
                }
            }
        }
        #endregion CreateDB

        #region CreateTable
        private static void CreateTable(Table table, DataTable result, IDatabase conn) {
            StringBuilder createTableSql = new StringBuilder("CREATE TABLE ");
            createTableSql.Append(conn.Enquote(ReplaceIllegalChars(table.TableName)));
            createTableSql.Append("(");

            foreach (ResultColumn column in result.Columns) {
                createTableSql.Append(conn.Enquote(ReplaceIllegalChars(column.Name))).Append(" LONGTEXT,");
            }
            //Remove last ,
            createTableSql.Remove(createTableSql.Length - 1, 1);

            createTableSql.Append(");");
            conn.ExecuteNonQuery(createTableSql.ToString());
        }
        #endregion CreateTable

        private static string ReplaceIllegalChars(string name) {
            const string illegalChars = ";:-,.!\"§$%&/()=?\\`´²³{[]}€*+~ ";
            string result = name;
            foreach(char sign in illegalChars)
                result = result.Replace(sign, '_');
            result = result.Replace("ö", "oe").Replace("ä", "ae").Replace("ü", "ue").Replace("Ä", "Ae").Replace("Ö", "Oe").
                    Replace("Ü", "Ue");
            result = result.Replace("___", "_").Replace("__", "_");
            return result;
        }
    }
}
