using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DbAccess;
using DbSearchDatabase.Results;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.Structures.Result.DataMapper {
    class ColumnResultsLoader {
        internal static void LoadColumnResults(ResultSet resultSet) {
            
            Dictionary<int, ColumnResult> idToColumnResults = new Dictionary<int, ColumnResult>();
            foreach (var columnResult in resultSet.ColumnResults) {
                idToColumnResults[resultSet.SnapshotQuery.IndexOfColumn(columnResult.Column)] = columnResult;
            }

            using (IDatabase conn = ConnectionManager.CreateConnection(resultSet.SnapshotQuery.Profile.DbProfile.GetProfileConfig())) {
                conn.Open();
                conn.SetHighTimeout();

                //Load all SearchValueResults for a resultSet, will later be used to put them in the right ColumnHitInfo
                Dictionary<int, List<DbSearchValueResult>> columnHitIdToSearchValueResult =
                    GetColumnHitIdToSearchValueResult(resultSet, conn);

                //First set up a dictionary to get a search value from a row id
                Dictionary<int, RowEntry> idToRowEntry = resultSet.SnapshotQuery.IdToRowEntry();
                Dictionary<int, SearchValue> idToSearchValue = new Dictionary<int, SearchValue>();
                foreach (var pair in idToRowEntry) {
                    SearchValue sv = new SearchValue(pair.Value.RuleDisplayString, CultureInfo.CurrentCulture, resultSet.SnapshotQuery.RowIdToColumn(pair.Key), pair.Key);
                    idToSearchValue[pair.Key] = sv;
                }


                //List<DbColumnHitInfo> dbColumnHits = conn.DbMapping.Load<DbColumnHitInfo>("query_id=" + resultSet.SnapshotQuery.DbQuery.Id + " AND result_id=" + resultSet.DbResultSet.Id);
                List<DbColumnHitInfo> dbColumnHits = DbColumnHitInfo.MassLoad(conn, "WHERE query_id=" + resultSet.SnapshotQuery.DbQuery.Id + " AND result_id=" + resultSet.DbResultSet.Id);
                foreach (var dbColumnHit in dbColumnHits) {
                    dbColumnHit.DbQuery = (DbQuery) resultSet.SnapshotQuery.DbQuery;
                    dbColumnHit.DbResultSet = (DbResultSet) resultSet.DbResultSet;

                    ColumnResult columnResult;
                    if(!idToColumnResults.TryGetValue(dbColumnHit.SearchColumnId, out columnResult)) continue;
                    ColumnHitInfo hitInfo = ColumnHitInfoMapper.Map(dbColumnHit,resultSet.SnapshotQuery.Profile.TableInfoManager);
                    columnResult.AddColumnHit(hitInfo);
                    hitInfo.AddResults(LoadSearchValueResults(columnHitIdToSearchValueResult, dbColumnHit, idToSearchValue));
                }
            }
        }


        private static Dictionary<int, List<DbSearchValueResult>> GetColumnHitIdToSearchValueResult(ResultSet resultSet, IDatabase conn) {
            Dictionary<int, List<DbSearchValueResult> > result = new Dictionary<int, List<DbSearchValueResult>>();
            //Use custom loading as the DbMapping.Load is much slower
            List<DbSearchValueResult> dbSearchValueResults = DbSearchValueResult.MassLoad(conn, "WHERE result_id=" + resultSet.DbResultSet.Id);
            foreach(var dbSearchValueResult in dbSearchValueResults) {
                dbSearchValueResult.DbQuery = (DbQuery)resultSet.SnapshotQuery.DbQuery;
                dbSearchValueResult.DbResultSet = (DbResultSet)resultSet.DbResultSet;
                List<DbSearchValueResult> list;
                if (!result.TryGetValue(dbSearchValueResult.DbColumnHitInfoId, out list)) {
                    list = new List<DbSearchValueResult>();
                    result[dbSearchValueResult.DbColumnHitInfoId] = list;
                }
                list.Add(dbSearchValueResult);
            }
            return result;
        }

        private static List<SearchValueResult> LoadSearchValueResults(Dictionary<int, List<DbSearchValueResult>> columnHitIdToSearchValueResult, 
            DbColumnHitInfo dbHitInfo, Dictionary<int, SearchValue> idToSearchValue) {

            List<DbSearchValueResult> dbSearchValueResults;
            if (!columnHitIdToSearchValueResult.TryGetValue(dbHitInfo.Id, out dbSearchValueResults))
                dbSearchValueResults = new List<DbSearchValueResult>();

            List<SearchValueResult> searchValueResults = new List<SearchValueResult>(dbSearchValueResults.Count);

            //foreach (var dbSearchValueResult in dbSearchValueResults) {
                searchValueResults.AddRange(SearchValueResultMapper.Map(dbSearchValueResults, idToSearchValue));
            //}

            return searchValueResults;
        }
    }
}
