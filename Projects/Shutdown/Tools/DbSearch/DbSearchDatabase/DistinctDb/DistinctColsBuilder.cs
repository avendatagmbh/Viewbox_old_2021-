using System;
using System.Collections.Generic;
using System.Data;
using DbAccess;

namespace DbSearchDatabase.DistinctDb {

    public class DistinctColsBuilder {

        

        private bool ColumnEqual(object A, object B) {

            // Compares two values to see if they are equal. Also compares DBNULL.Value.
            // Note: If your DataTable contains object fields, then you must extend this
            // function to handle them in a meaningful way if you intend to group on them.

            if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
                return true;
            if (A == DBNull.Value || B == DBNull.Value) //  only one is DBNull.Value
                return false;
            return (A.Equals(B));  // value type standard comparison
        }


        public DataTable SelectDistinct(string TableName, DataTable SourceTable, string FieldName) {
            DataTable dt = new DataTable(TableName);
            dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

            object LastValue = null;
            foreach (DataRow dr in SourceTable.Select("", FieldName)) {
                if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName]))) {
                    LastValue = dr[FieldName];
                    dt.Rows.Add(new object[] { LastValue });
                }
            }
            
            return dt;
        }

        public List<DataRow> SelectEquals(string TableName, DataTable SourceTable, 
                            string FieldName, object oSearchValue) {

            List<DataRow> lstRows = new List<DataRow>();
            
            
            foreach (DataRow dr in SourceTable.Select("", FieldName)) {
                if (ColumnEqual(oSearchValue, dr[FieldName])) {

                    lstRows.Add(dr);
                }
            }

            return lstRows;
        }

        public int GetDistinctCount(string TableName, DataTable SourceTable, string FieldName) {

            int nDistCount = 0;

            //DataTable dt = new DataTable(TableName);
            //dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

            object LastValue = null;
            foreach (DataRow dr in SourceTable.Select("", FieldName)) {
                if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName]))) {
                    LastValue = dr[FieldName];
                    nDistCount++;
                }
            }

            return nDistCount;
        }

        public Dictionary<string, int> GetDistinctCountsDic(
                    string sTableName, DataTable oDataTable) {

            Dictionary<string, int> oDicCounts = new Dictionary<string, int>();

            DataColumnCollection oColl = oDataTable.Columns;

            for (int n = 0; n < oColl.Count; n++) {

                string sColName = oColl[n].ToString();
                int nCount = GetDistinctCount(sTableName, oDataTable, sColName);
                oDicCounts.Add(sColName, nCount);

            }

            return oDicCounts;
        }

        public string GetFieldMinDistinctNum(Dictionary<string, int> oDicCounts,
                            List<string> lstExcFields) {

            string sResField = String.Empty;
            int nMin = 1000000;
            string sMinKey = String.Empty;

            foreach (string sKey in oDicCounts.Keys) {
                if (oDicCounts[sKey] < nMin && !lstExcFields.Contains(sKey)) {
                    nMin = oDicCounts[sKey];
                    sMinKey = sKey;
                }
            }


            return sMinKey;
        }

        public int GetDistinctCountsSum(Dictionary<string, int> oDicCounts) {

            int nSum = 0;
            string sMinKey = String.Empty;

            foreach (int nNum in oDicCounts.Values) {
                nSum += nNum;
            }


            return nSum;
        }

        private DataRow GetRow2Exchange(DataTable oActualDataTable, string sTableName, string sFieldName) {

            DataRow oRow = null;

            // search for row with most duplicates in <sFieldName>
            // first get distinct values
            List<object> lstDistinctValues = new List<object>();

            DataTable oTable = SelectDistinct(sTableName, oActualDataTable, sFieldName);

            foreach (DataRow oRow1 in oTable.Rows) {

                object sVal = oRow1[0];
                lstDistinctValues.Add(sVal);
            }

            int nMaxCount = 0;
            DataTable oTab = new DataTable();

            // algorithm to find the row could be improved
            foreach (object oValue in lstDistinctValues){

                List<DataRow> lstRows = SelectEquals(sTableName, oActualDataTable, sFieldName, oValue);

                int nCount = lstRows.Count;
                if (nCount > nMaxCount) {
                    nMaxCount = nCount;
                    oRow = lstRows[0];
                }

            }

            //lstRows = SelectEquals(sTableName, oActualDataTable, sFieldName, oRow[0].ToString());


            return oRow;

        }

        private DataTable ExchangeRowDupl(DataTable oActualDataTable,
                            DataTable oNewRows, string sTableName, string sFieldName) {

            if (oNewRows.Rows.Count == 0)
                return oActualDataTable;

            //DataRowCollection oColl = oActualDataTable.Rows;

            //DataTable oTableClone = oActualDataTable.Clone();

            //foreach (DataRow row in oActualDataTable.Rows) {

            //    oTableClone.ImportRow(row);
                
            //}

            Dictionary<string, int> oDicCounts = new Dictionary<string, int>();
            oDicCounts = GetDistinctCountsDic(sTableName, oActualDataTable);

            // get total number of distinct values
            int nSumDist1 = GetDistinctCountsSum(oDicCounts);

            

            foreach (DataRow row in oNewRows.Rows) {

                DataRow oRow = GetRow2Exchange(oActualDataTable, sTableName, sFieldName);
                object[] arrValues = oRow.ItemArray;

                oActualDataTable.Rows.Remove(oRow);

                DataRow oNewRow = oActualDataTable.Rows.Add(row.ItemArray);

                oDicCounts = GetDistinctCountsDic(sTableName, oActualDataTable);

                int nSumDist2 = GetDistinctCountsSum(oDicCounts);

                // found a "good" row increasing the distincts
                if (nSumDist2 > nSumDist1)
                    break;
                else {
                    oActualDataTable.Rows.Remove(oNewRow);
                    oActualDataTable.Rows.Add(arrValues);

                    

                    oDicCounts = GetDistinctCountsDic(sTableName, oActualDataTable);

                    int nSumDist3 = GetDistinctCountsSum(oDicCounts);
                }

            }

            return oActualDataTable;

        }

        private DataTable GetNewDistinctRows(DataTable oActualDataTable, string sTableName, 
                        string sFieldName, IDatabase db) {

            List<object> lstDistinctValues = new List<object>();

            DataTable oTable = SelectDistinct(sTableName, oActualDataTable, sFieldName);

            bool bNullValueFound = false;

            foreach (DataRow oRow in oTable.Rows) {

                object oVal = oRow[0];

                if (oVal is DBNull)
                    bNullValueFound = true;
                else
                    lstDistinctValues.Add(oVal);
            }

            string sSel = "SELECT * FROM " + db.Enquote(sTableName) + " WHERE `" + sFieldName + "` NOT IN (";

            Type t = oTable.Columns[sFieldName].DataType;

            switch (t.Name) {

                case "String":

                    foreach (object sValue in lstDistinctValues) {
                        sSel += "'" + sValue + "', ";
                    }

                    sSel = sSel.Remove(sSel.Length - 2, 2);
                    break;

                default:

                    foreach (object sValue in lstDistinctValues) {

                        //if (sValue != "")
                            sSel += sValue + ", ";
                    }

                    sSel = sSel.Remove(sSel.Length - 2, 2);
                    break;

            }

            
            
            sSel += ")";

            if (bNullValueFound)
                sSel += " AND `" + sFieldName + "` IS NOT NULL";

            //oTable = db.GetDataTable(sSel, sTableName, 20);
            oTable = db.GetDataTable(sSel, 20);

            return oTable;

        }

        private DataTable OptimizeDistValues(string sTableName, IDatabase db, ref Dictionary<string, int> oDicCounts) {

            DataTable oDataTable = new DataTable();
            DataTable oTable = new DataTable();
            //Dictionary<string, int> oDicCounts = new Dictionary<string, int>();
            int nSumDist1 = 0;

            List<string> lstExcFields = new List<string>();

            try {

                //oDataTable = db.GetDataTable("SELECT * FROM " + db.Enquote(sTableName), sTableName, 30);
                oDataTable = db.GetDataTable("SELECT * FROM " + db.Enquote(sTableName), 30);
                oDicCounts = GetDistinctCountsDic(sTableName, oDataTable);
                nSumDist1 = GetDistinctCountsSum(oDicCounts);

                for (int n = 0; n < 30; n++) {

                    string sField = GetFieldMinDistinctNum(oDicCounts, lstExcFields);

                    DataTable oNewTable = GetNewDistinctRows(oDataTable, sTableName, sField, db);

                    if (oNewTable.Rows.Count == 0) {

                        lstExcFields.Add(sField);
                        continue;
                    }


                    // check new table for more distinct rows
                    
                    oDataTable = ExchangeRowDupl(oDataTable, oNewTable, sTableName, sField);

                    oDicCounts = GetDistinctCountsDic(sTableName, oDataTable);
                    int nSumDist2 = GetDistinctCountsSum(oDicCounts);

                    // no success, so ignore field in future
                    if (nSumDist2 <= nSumDist1)
                        lstExcFields.Add(sField);

                    nSumDist1 = nSumDist2;

                }

            }
            catch (Exception) {
                //LastErrorMessage = "Fehler bei Ausführung von: " + sql + ": " + ex.Message;
                //throw new Exception(LastErrorMessage);
            }

            return oDataTable;
        }

        public DataTable GetDataTable(string sTableName, IDatabase db, 
                        ref Dictionary<string, int> oDicCounts) {

            DataTable oDataTable = new DataTable();
            DataTable oTable = new DataTable();

            

            try {
                oDataTable = OptimizeDistValues(sTableName, db, ref oDicCounts);

                


            }
            catch (Exception) {
                //LastErrorMessage = "Fehler bei Ausführung von: " + sql + ": " + ex.Message;
                //throw new Exception(LastErrorMessage);
            }

            return oDataTable;
        }


    }
}
