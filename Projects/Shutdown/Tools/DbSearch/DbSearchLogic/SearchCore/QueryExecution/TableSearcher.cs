using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.SearchCore.Config;
using DbSearchLogic.SearchCore.QueryCache;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;
using MySql.Data.Types;
using log4net;
using AV.Log;

//using Avd.Database.DbAccess.Config;

namespace DbSearchLogic.SearchCore.QueryExecution {
    
    /// <summary>
    /// Searches in the given database for the given parameters
    /// </summary>
    public static class TableSearcher {

        internal static ILog _log = LogHelper.GetLogger();

        #region Methods

        #region SearchTable
        /// <summary>
        /// Searches the given table for the search parameters
        /// </summary>
        /// <param name="tableInfo"> </param>
        /// <param name="conn">Database connection</param>
        /// <param name="Info"></param>
        /// <param name="globalSearchConfig"> </param>
        /// <param name="tableResult"></param>
        /// <param name="sTableName">Name of the table</param>
        public static TableResultSet SearchTable(TableInfo tableInfo, IDatabase conn, QueryInfo Info, GlobalSearchConfig globalSearchConfig, out TableResultSet tableResult //SearchValueMatrix oSVM,
            ) {

            SearchValueMatrix svm = Info.QueryConfig.SearchValueMatrix;

            // Create the ResultSet
            tableResult = new TableResultSet(tableInfo);
            //TableResultSet resultSet = new TableResultSet(sTableName);
            long tableCount = conn.CountTable(tableInfo.Name);
            // check, if query execution was cancelled
            if (tableCount == 0) {
                return tableResult;
            }

            // Create the list of field types
            List<TableColumn> tableColumns = new List<TableColumn>();

            // Get the field types of the current table
            //using (DbDataReader oReader = conn.ExecuteReader(
            //    "SELECT * FROM " + conn.Enquote(tableInfo.Name), System.Data.CommandBehavior.SchemaOnly)) {

            //    // Iterate through all columns
            //    for (int i = 0; i < oReader.FieldCount; i++) {
            //        // Create the list in the array
            //        if ((oReader.GetName(i) != "_row_no_") &&
            //            !oReader.GetDataTypeName(i).ToUpper().Contains("BLOB")) {

            //            oTableColumns.Add(new TableColumn(
            //                                  conn.ReplaceSpecialChars(oReader.GetName(i)),
            //                                  conn.ReplaceSpecialChars(oReader.GetFieldType(i).Name)));
            //        }
            //    }

            //    Info.State.TableStarted(tableInfo.Name, conn.DbConfig.Hostname, oReader.FieldCount, tableCount);
            //}

            List<DbColumnInfo> columnInfos = conn.GetColumnInfos(tableInfo.Name);
            foreach (var columnInfo in columnInfos) {
                if(columnInfo.Name != "_row_no_" && columnInfo.Type != DbColumnTypes.DbBinary)
                    tableColumns.Add(new TableColumn(conn.ReplaceSpecialChars(columnInfo.Name),columnInfo.Type));
            }
            Info.State.TableStarted(tableInfo.Name, conn.DbConfig.Hostname, tableColumns.Count, tableCount);
            // Init State

            /*
             * Search ColumnHits
             * 
             * First all unique entries of a table column are collected.
             * Afterwards every single entry will be checked against the SearchValues.
             */

            //List<SearchValueColumnMapping> lFullColumnHits = new List<SearchValueColumnMapping>();

            // Create all possible column mappings
            List<ColumnHit> columnHits = new List<ColumnHit>();

            // Iterate through all table columns
            foreach (TableColumn tableColumn in tableColumns) {
                if (Info.Cancel) break;

                List<ColumnHit> columnHitsForTableColumn = new List<ColumnHit>();

                // Update the state
                Info.State.NextColumn(tableInfo.Name, tableColumn.Name);
                SearchTableColumn(tableInfo.Name, tableColumn, conn, Info, columnHitsForTableColumn, tableResult, globalSearchConfig);

                //if (false && Info.QueryConfig.SearchParams.ExtendedSearchOnHit) {

                //    // doing something with lColumnHitsForTableColumn
                //    if (lColumnHitsForTableColumn.Count > 0) {
                //        // Some hits where found, look where all values where founded
                //        foreach (ColumnHit oColumnHit in lColumnHitsForTableColumn) {
                //            foreach (ColumnHitInfo oColumnHitInfo in oColumnHit.TableColumns) {
                //                if (oColumnHitInfo.MissingValues.Count == 0) {
                //                    // all values where found in one column -> look for more values
                //                    lFullColumnHits.Add(new SearchValueColumnMapping(sTableName, oTableColumn.Name, oTableColumn.Type, oColumnHit.SearchColumnName));
                //                }
                //            }

                //            // Writing columnhits in global list
                //            lColumnHits.Add(oColumnHit);
                //        }
                //    }
                //} else {
                    foreach (ColumnHit oColumnHit in columnHitsForTableColumn) {
                        columnHits.Add(oColumnHit);
                    }
                //}

                // Update the state
                Info.State.ColumnFinished(tableInfo.Name);
            }

            //ExtendedSearch(conn, Info, lFullColumnHits, lColumnHits);

            // Check the search column hits
            bool addColumnHits = true;
            //if (false && Info.QueryConfig.SearchParams.OnlyAllSearchColumns) {
            //    List<string> lSearchColumnHits = new List<string>();
            //    foreach (ColumnHit oColumnHit in lColumnHits) {
            //        lSearchColumnHits.Add(oColumnHit.SearchColumnName);
            //    }
            //    lSearchColumnHits = lSearchColumnHits.Distinct().ToList();

            //    if (lSearchColumnHits.Count < svm.NumberOfColumns) {
            //        addColumnHits = false;
            //    }
            //}

            // Add the hits to the ResultSet
            if (addColumnHits) {
                foreach (ColumnHit columnHit in columnHits) {
                    //ChangeEmptyStringsAsHit(columnHit);
                    // Add the column hit to the result set
                    tableResult.ColumnHits.Add(columnHit);
                }
            }
            Info.State.TableFinished(tableInfo.Name);
            // Return the result set
            if (tableResult.NumberOfColumnHits > 0) {
                return tableResult;
            }

            return null;
        }
        #endregion SearchTable

        #region ChangeEmptyStringsAsHit
        //private static void ChangeEmptyStringsAsHit(ColumnHit colHit) {
        //    foreach (ColumnHitInfo colHitInfo in colHit.TableColumns) {
        //        List<SearchValue> lEmptyValues = new List<SearchValue>();
        //        foreach (SearchValue searchValue in colHitInfo.MissingValues) {
        //            if (searchValue.String.Trim().Length == 0) {
        //                lEmptyValues.Add(searchValue);
        //            }
        //        }

        //        foreach (SearchValue oSearchVal in lEmptyValues) {
        //            // Removing value from missing-list
        //            // Add HitCount
        //            colHitInfo.MissingValues.Remove(oSearchVal);
        //            colHitInfo.HitCount++;
        //        }
        //    }
        //}
        #endregion

        #region ExtendedSearch
        //private static void ExtendedSearch(
        //    IDatabase conn, 
        //    QueryInfo Info, 
        //    List<SearchValueColumnMapping> lFullColumnHits, 
        //    List<ColumnHit> lColumnHits) {
            
        //    IDatabase Db = null;
        //    DbConfig DbConfig = null;

        //    try {
        //        // only look for more search-values in access-db, if filename is valid
        //        if (Info.Query.OriginDatabase != null &&
        //            !Info.Query.OriginDatabase.Equals(string.Empty) &&
        //            Info.Query.OriginTable != null &&
        //            !Info.Query.OriginTable.Equals(string.Empty)) {

        //            if (Info.Query.SearchValues.ExtendedSearchValues == null) {
        //                Info.Query.SearchValues.ExtendedSearchValues = new SerializableDictionary<string, List<string>>();
        //            }

        //            DbConfig = new ConfigDatabase(
        //                DatabaseTypes.Access, Info.Query.OriginDatabase, "Admin", string.Empty, string.Empty, 0, string.Empty);
        //            Db = DatabaseBuilder.CreateDatabase(DbConfig);
        //            Db.Open();

        //            foreach (SearchValueColumnMapping oSearch in lFullColumnHits) {

        //                List<string> lValues = null;

        //                if (Info.Query.SearchValues.ExtendedSearchValues.TryGetValue(oSearch.ColumnNameSearchValueMatrix, out lValues)) {
        //                    // values already saved, look if limit is equal
        //                    if (Info.Query.SearchValues.NumberExtraSearchvalues < Info.Query.SearchParams.MaximumDatasetsExtendedSearch) {
        //                        // need to load more data
        //                        lValues.Clear();
        //                        LoadDataFromAccess(Info, Db, oSearch, lValues);
        //                        Info.Query.SearchValues.ExtendedSearchValues[oSearch.ColumnNameSearchValueMatrix] = lValues;
        //                        Info.Query.SearchValues.NumberExtraSearchvalues = Info.Query.SearchParams.MaximumDatasetsExtendedSearch;
        //                    }
        //                } else {
        //                    // values not loaded until yet, so load
        //                    lValues = new List<string>();
        //                    LoadDataFromAccess(Info, Db, oSearch, lValues);
        //                    Info.Query.SearchValues.ExtendedSearchValues[oSearch.ColumnNameSearchValueMatrix] = lValues;
        //                    Info.Query.SearchValues.NumberExtraSearchvalues = Info.Query.SearchParams.MaximumDatasetsExtendedSearch;
        //                }                        

        //                List<ColumnHit> lColHits = new List<ColumnHit>();
        //                SearchValueMatrix matrix = new SearchValueMatrix(lValues.Count, 1);

        //                System.Globalization.CultureInfo oLanguage;
        //                if (Info.Query.SearchParams.UsePrimaryInterpretationLanguage == "Englisch") {
        //                    oLanguage = Global.English;
        //                } else {
        //                    oLanguage = Global.German;
        //                }

        //                for (int i = 0; i < lValues.Count; i++) {
        //                    matrix.Values[i, 0] = new SearchValue(lValues[i], oLanguage);
        //                }
        //                matrix.InitTypes();

        //                TableColumn tabCol = new TableColumn(oSearch.ColumnName, oSearch.ColumnType);

        //                SearchTableColumn(oSearch.TableName, tabCol, conn, matrix, Info, lColHits);

        //                if (lColHits.Count == 0 || lColHits[0].TableColumns.Count == 0) {
        //                    continue;
        //                }
        //                //if (lColHits.Count > 0) {
        //                //    // values found
        //                //    if (lColHits[0].TableColumns[0].MissingValues.Count == 0) {
        //                foreach (ColumnHit colHit in lColumnHits) {
        //                    if (colHit.SearchColumnName.Equals(oSearch.ColumnNameSearchValueMatrix)) {
        //                        foreach (ColumnHitInfo colHitInfo in colHit.TableColumns) {
        //                            if (colHitInfo.ColumnName.Equals(oSearch.ColumnName)) {
        //                                colHitInfo.HitCount = lColHits[0].TableColumns[0].HitCount;
        //                                colHitInfo.MissingValues = lColHits[0].TableColumns[0].MissingValues;
        //                            }
        //                        }
        //                    }
        //                }
        //                // all values found
        //                //MessageBox.Show("Alle Werte gefunden für: " + oSearch.ColumnNameSearchValueMatrix + " aus " + oSearch.TableName + "." + tabCol.Name);
        //                //    } else {
        //                //        // not all values found

        //                //    }
        //                //}

        //                if (lColHits.Count > 1) {
        //                    MessageBox.Show("Mehrere ColumnHits festgestellt.");
        //                }
        //            }
        //            Db.Close();
        //        }
        //    } catch (Exception ex) {
        //        FileLogger.LogException(ex, "ExtendedSearch");
        //        // search for more values went wrong, 100% hits are not sure over all search values
        //        System.Diagnostics.Debug.WriteLine("Fehler bei der erweiterten Suche der 100% Spaltentreffer." + System.Environment.NewLine +
        //                                           ex.Message + " " + ex.StackTrace);
        //        OnError("Access Error: ExtendedSearch");
        //    } finally {
        //        if (Db != null) {
        //            Db.Close();
        //        }
        //    }
        //}
        #endregion

        #region LoadDataFromAccess
        //private static void LoadDataFromAccess(QueryInfo Info, IDatabase Db, SearchValueColumnMapping oSearch, List<string> lValues) {
        //    string sSQL =
        //        "SELECT DISTINCT TOP " + Info.Query.SearchParams.MaximumDatasetsExtendedSearch.ToString() + " " +
        //        Db.Enquote(oSearch.ColumnNameSearchValueMatrix) + " FROM " + Db.Enquote(Info.Query.OriginTable);

        //    DbDataReader dbReader = Db.ExecuteReader(sSQL);
        //    while (dbReader.Read()) {
        //        if (dbReader.IsDBNull(0)) {
        //            lValues.Add(string.Empty);
        //        } else {
        //            lValues.Add(dbReader.GetValue(0).ToString());
        //        }
        //    }
        //    dbReader.Close();
        //}
        #endregion

        #region SearchTableColumn
        /// <summary>
        /// Extendeds the search.
        /// </summary>
        /// <param name="tableName"> </param>
        /// <param name="tableColumn"> </param>
        /// <param name="db"> </param>
        /// <param name="info"> </param>
        /// <param name="columnHits"> </param>
        /// <param name="tableResult"> </param>
        /// <param name="globalSearchConfig"> </param>
        /// <param name="conn">The o database.</param>
        /// <param name="Info">The info.</param>
        /// <param name="oDbSpecific">The o db specific.</param>
        /// <param name="lFullColumnHits">The l full column hits.</param>
        /// <param name="lColumnHits">The l column hits.</param>
        /// <summary>
        /// Searches the table columns
        /// </summary>
        private static void SearchTableColumn(string tableName, TableColumn tableColumn, IDatabase db, QueryInfo info, List<ColumnHit> columnHits, TableResultSet tableResult, GlobalSearchConfig globalSearchConfig) {

            SearchValueMatrix svm = info.QueryConfig.SearchValueMatrix;
            IDbColumnEnumerator columnValues = DbCache.GetColumn(tableName, tableColumn, db, svm, info);
            if (columnValues == null) return;

            // Create a temporary matrix to store the hits.
            SearchValueResult[] searchValueResults = new SearchValueResult[svm.AllValues.Count()];
            for(int i = 0; i < svm.AllValues.Count(); ++i)
                searchValueResults[i] = new SearchValueResult(svm.AllValues[i]);

            try {
                // Analyse column, depending on column type
                SearchColumnGeneric(info, columnValues, searchValueResults, tableColumn.Type, globalSearchConfig);
            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Error, "Fehler bei der Suche in Spalte " + tableName + "." + tableColumn.Name + Environment.NewLine + ex.Message);
                throw;
            } finally {
                columnValues.Close();
            }

            try {
                //Add results
                Dictionary<Column, List<SearchValueResult>> columnToSearchValueResults = new Dictionary<Column, List<SearchValueResult>>();

                for (int i = 0; i < svm.NumberOfValues; i++) {
                    if (!columnToSearchValueResults.ContainsKey(svm.AllValues[i].Column))
                        columnToSearchValueResults.Add(svm.AllValues[i].Column, new List<SearchValueResult>());
                    columnToSearchValueResults[svm.AllValues[i].Column].Add(searchValueResults[i]);
                }

                //Remove column results with no hits at all
                for (int i = 0; i < svm.NumberOfValues; i++) {
                    if (columnToSearchValueResults.ContainsKey(svm.AllValues[i].Column)) {
                        int hits = 0;
                        foreach (var result in columnToSearchValueResults[svm.AllValues[i].Column])
                            hits += result.Hits;
                        if (hits == 0)
                            columnToSearchValueResults.Remove(svm.AllValues[i].Column);
                    }
                }

                foreach (var pair in columnToSearchValueResults) {
                    bool found = false;
                    foreach (ColumnHit columnHit in columnHits) {
                        if (columnHit.SearchColumnName == pair.Key.Name) {
                            found = true;
                            //oColumnHit.TableColumns.Add(new ColumnHitInfo(tableResult.TableInfo, tableColumn.Name, tableColumn.Type, pair.Value) { ColumnHit = oColumnHit });
                            columnHit.TableColumns.Add(new ColumnHitInfo(tableResult.TableInfo, tableColumn.Name, tableColumn.Type, pair.Value));
                        }
                    }

                    if (!found) {
                        ColumnHit columnHit = new ColumnHit(tableResult, pair.Key.Name);

                        columnHit.TableColumns.Add(new ColumnHitInfo(tableResult.TableInfo, tableColumn.Name, tableColumn.Type, pair.Value));
                        columnHits.Add(columnHit);
                    }
                }
                
                // Check the result
                //using (IDatabase profileDb = info.ProfileDb.GetOpenConnection()) {
                //    for (int i = 0; i < svm.NumberOfColumns; i++) {
                //        int nHits = 0;
                //        List<SearchValue> lMissingValues = new List<SearchValue>();

                //        //DbSearch_ColumnInfo ci;
                //        //if (info.ProfileDb.TableInfos[tableName.ToLower()].Columns.ContainsKey(tableColumn.Name))
                //        //    ci = info.ProfileDb.TableInfos[tableName.ToLower()].Columns[tableColumn.Name];
                //        //else if (info.ProfileDb.TableInfos[tableName.ToLower()].Columns.ContainsKey(tableColumn.Name.ToLower()))
                //        //    ci = info.ProfileDb.TableInfos[tableName.ToLower()].Columns[tableColumn.Name.ToLower()];
                //        //else if (info.ProfileDb.TableInfos[tableName.ToLower()].Columns.ContainsKey(tableColumn.Name.ToUpper()))
                //        //    ci = info.ProfileDb.TableInfos[tableName.ToLower()].Columns[tableColumn.Name.ToUpper()];
                //        //else throw new ArgumentException("Konnte die Spalte \"" + tableColumn.Name + "\" nicht im Wörterbuch finden.");
                //        for (int j = 0; j < svm.NumberOfRows; j++) {

                //            if (oTempMatrix[j, i]) {
                //                nHits++;

                //                string value = svm.Values[j, i].String;
                //                if (!addedResultValues.ContainsKey(value)) {
                                    
                //                    addedResultValues.Add(value, true);

                //                    //DbSearch_Result result = new DbSearch_Result(
                //                    //    info.DbQueryInfo, info.DbQueryInfo.SearchValues.Values[value], ci);

                //                    //info.DbQueryInfo.AddResult(result);

                //                    if (idValues[j, i].Count < MaxDistinctTableIds && idValues[j,i].Count > 0) {
                //                        using (System.IO.MemoryStream tmp = new System.IO.MemoryStream()) {
                //                            foreach (UInt32 id in idValues[j, i]) {
                //                                tmp.Write(BitConverter.GetBytes(id), 0, 4);
                //                            }
                //                            //result.Save(profileDb, tmp);
                //                        }
                //                    } else {
                //                        //result.Save(profileDb);
                //                    }
                //                }

                //                idValues[j, i].Clear();

                //            } else {
                //                if(svm.Values[j,i].UseEntry)
                //                    lMissingValues.Add(svm.Values[j, i]);
                //            }
                //        }

                //        if (nHits > 0) {
                //            // Add a column hit
                //            bool flag = false;
                //            foreach (ColumnHit oColumnHit in columnHits) {
                //                if (oColumnHit.SearchColumnName == svm.GetColumnName(i) &&
                //                    oColumnHit.SearchColumnIndex == i) {
                //                    flag = true;

                //                    oColumnHit.TableColumns.Add(
                //                        new ColumnHitInfo(tableColumn.Name, tableColumn.Type, nHits, lMissingValues));
                //                }
                //            }

                //            if (!flag) {
                //                ColumnHit oColumnHit = new ColumnHit(svm.GetColumnName(i), i);

                //                oColumnHit.TableColumns.Add(
                //                    new ColumnHitInfo(tableColumn.Name, tableColumn.Type, nHits, lMissingValues));

                //                columnHits.Add(oColumnHit);
                //            }
                //        }
                //    }
                //}

                // cleanup
                //Array.Clear(idValues, 0, idValues.Length);
                //idValues = null;
                //System.GC.Collect();

            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Error, "Ein Fehler ist bei der Suche in Tabelle " + tableName + " aufgetreten: " + ex.Message);
                //FileLogger.LogException(ex, "SearchTableColumn II");
                throw;
            }
        }

        #endregion SearchTableColumn

        #region SearchColumnGeneric
        private static void SearchColumnGeneric(QueryInfo info, IDbColumnEnumerator columnValues, SearchValueResult[] searchValueResults, DbColumnTypes type, GlobalSearchConfig globalSearchConfig) {
            SearchValueMatrix svm = info.QueryConfig.SearchValueMatrix;

            while (columnValues.Next()) {
                // check wether the user cancelled the operation
                if (info.Cancel) {
                    columnValues.Close();
                    break;
                }

                switch (type) {
                    case DbColumnTypes.DbNumeric:
                        CompareNumeric(columnValues, svm, searchValueResults);
                        CompareStringForRequested(columnValues, svm, searchValueResults, globalSearchConfig);
                        break;
                    case DbColumnTypes.DbInt:
                    case DbColumnTypes.DbBigInt:
                    case DbColumnTypes.DbBool:
                        CompareInt(columnValues, svm, searchValueResults);
                        CompareStringForRequested(columnValues, svm, searchValueResults, globalSearchConfig);
                        break;
                    case DbColumnTypes.DbText:
                    case DbColumnTypes.DbLongText:
                        CompareString(columnValues, svm, searchValueResults, globalSearchConfig);
                        break;
                    case DbColumnTypes.DbDate:
                    case DbColumnTypes.DbTime:
                    case DbColumnTypes.DbDateTime:
                        CompareDateTime(columnValues, svm, searchValueResults);
                        CompareStringForRequested(columnValues, svm, searchValueResults, globalSearchConfig);
                        break;
                        //case DbColumnTypes.DbBinary:
                        //    break;
                        //case DbColumnTypes.DbUnknown:
                        //    break;
                    default:
                        throw new NotImplementedException("SearchTableColumn: Unbekannter Typ gefunden");
                }
            }
        }

        private static void CompareStringForRequested(IDbColumnEnumerator columnValues, SearchValueMatrix svm, SearchValueResult[] searchValueResults, GlobalSearchConfig globalSearchConfig) {
            object obj = columnValues.GetValue();
            if (obj == DBNull.Value) return;
            string dbValue = obj.ToString();
            if (globalSearchConfig.MaxStringLength != 0 && dbValue.Length > globalSearchConfig.MaxStringLength)
                return;

            // compare dbValue to each search value
            //StringComparison comparsionType = StringComparison.CurrentCulture;
            int nAllFields = svm.AllValues.GetLength(0);
            for (int i = 0; i < nAllFields; i++) {
                if (!svm.AllValues[i].SearchParams.UseStringSearch || searchValueResults[i].HasResults) continue;

                //if (svm.AllValues[i].SearchParams.UseCaseIgnore)
                //    comparsionType = StringComparison.CurrentCultureIgnoreCase;

                if (svm.AllValues[i].SearchParams.UseInStringSearch) {
                    if (dbValue.IndexOf(svm.AllValues[i].String, svm.AllValues[i].SearchParams.StringComparison) != -1) {
                        searchValueResults[i].AddHit(columnValues.GetId(), SearchValueResultEntryType.InString);
                    }
                }
                else {
                    if (dbValue.Equals(svm.AllValues[i].String, svm.AllValues[i].SearchParams.StringComparison)) {
                        searchValueResults[i].AddHit(columnValues.GetId(), SearchValueResultEntryType.Exact);
                    }
                }

            }
        }

        #region CompareInt
        private static void CompareInt(IDbColumnEnumerator columnValues, SearchValueMatrix svm, SearchValueResult[] searchValueResults) {
            Object obj = columnValues.GetValue();
            if (obj == DBNull.Value) return;
            Int64 dbValue = Convert.ToInt64(obj);


            int nIntegerFields = svm.IntegerValues.GetLength(0);
            // compare dbValue to each search value
            for (int i = 0; i < nIntegerFields; i++) {
                if (dbValue == svm.IntegerValues[i].Integer) {
                    searchValueResults[svm.IntegerCellsIndices[i]].AddHit(columnValues.GetId());
                }
            }
        }
        #endregion CompareInt

        #region CompareDateTime
        private static TimeSpan _nullTimeSpan = new TimeSpan(0);
        private static void CompareDateTime(IDbColumnEnumerator columnValues, SearchValueMatrix svm, SearchValueResult[] searchValueResults) {
            Object obj = columnValues.GetValue();
            if (obj == DBNull.Value) return;
            DateTime dbValue;
            try {
                if (obj is MySqlDateTime) {
                    MySqlDateTime mdt = (MySqlDateTime)obj;
                    if (mdt.IsValidDateTime) {
                        dbValue = mdt.GetDateTime();
                    } else {
                        return;
                    }
                } else {
                    try {
                        //TODO:Handle Timespan
                        if (obj is TimeSpan) return;
                        dbValue = (DateTime)obj;
                    } catch (Exception exCast) {
                        _log.Log(LogLevelEnum.Error, "Error converting to DateTime: " + exCast.Message); 
                        return;
                    }
                }
            } catch (Exception exIn) {
                _log.Log(LogLevelEnum.Error, "Error converting to DateTime: " + exIn.Message);
                throw;
            }

            int nDateTimeFields = svm.DateTimeValues.GetLength(0);
            // compare dbValue to each search value
            for (int i = 0; i < nDateTimeFields; i++) {
                if (dbValue == svm.DateTimeValues[i].DateTime.Value) {
                    searchValueResults[svm.DateTimeCellsIndices[i]].AddHit(columnValues.GetId());
                } else if (dbValue.Date == svm.DateTimeValues[i].DateTime.Value || (dbValue.TimeOfDay == svm.DateTimeValues[i].DateTime.Value.TimeOfDay && dbValue.TimeOfDay != _nullTimeSpan)) {
                    searchValueResults[svm.DateTimeCellsIndices[i]].AddHit(columnValues.GetId(), SearchValueResultEntryType.DateTimePartial);
                }
            }
            
        }
        #endregion CompareDateTime

        #region CompareString
        private static void CompareString(IDbColumnEnumerator columnValues, SearchValueMatrix svm, SearchValueResult[] searchValueResults, GlobalSearchConfig globalSearchConfig) {
            object obj = columnValues.GetValue();
            if (obj == DBNull.Value) return;
            string dbValue = obj.ToString();
            if (globalSearchConfig.MaxStringLength != 0 && dbValue.Length > globalSearchConfig.MaxStringLength)
                return;
            // compare dbValue to each search value
            int nStringFields = svm.TextValues.GetLength(0);
            //StringComparison comparisonType = StringComparison.CurrentCulture;
            for (int i = 0; i < nStringFields; i++) {
                //if (svm.TextValues[i].SearchParams.UseCaseIgnore) comparisonType = StringComparison.CurrentCultureIgnoreCase;

                if (svm.TextValues[i].SearchParams.UseInStringSearch) {

                    if (dbValue.IndexOf(svm.TextValues[i].String, svm.TextValues[i].SearchParams.StringComparison) != -1) {
                        searchValueResults[svm.TextCellsIndices[i]].AddHit(columnValues.GetId(), SearchValueResultEntryType.InString);
                    }
                } else {
                    if (dbValue.Equals(svm.TextValues[i].String, svm.TextValues[i].SearchParams.StringComparison)) {
                        searchValueResults[svm.TextCellsIndices[i]].AddHit(columnValues.GetId(), SearchValueResultEntryType.Exact);
                    }
                }
            }
        }
        #endregion CompareString

        #region CompareNumeric
        private static void CompareNumeric(IDbColumnEnumerator columnValues, SearchValueMatrix svm, SearchValueResult[] searchValueResults) {
            int nNumericCells = svm.NumericValues.GetLength(0);

            Object obj = columnValues.GetValue();
            if (obj == DBNull.Value) return;

            decimal dbValue = 0;
            try{
                dbValue = Convert.ToDecimal(obj);
            }catch(Exception exIn){
                _log.Log(LogLevelEnum.Error, "Error converting to decimal: " + exIn.Message);
            }
            // compare dbValue to each search value
            for (int i = 0; i < nNumericCells; i++) {
                decimal value = svm.NumericValues[i].Numeric.Value;

                if (dbValue == value) {
                    searchValueResults[svm.NumericCellsIndices[i]].AddHit(columnValues.GetId());
                }
                else if (svm.NumericValues[i].SearchParams.UseSearchRoundedValues) {
                    if (
                        Math.Round(dbValue, svm.NumericValues[i].SearchParams.NumericPrecision) ==
                        Math.Round(value, svm.NumericValues[i].SearchParams.NumericPrecision)) {

                        searchValueResults[svm.NumericCellsIndices[i]].AddHit(columnValues.GetId(), SearchValueResultEntryType.Rounded);
                    }
                } 

            }

        }
        #endregion CompareNumeric

        #endregion SearchColumnGeneric

        #endregion Methods

    }
}