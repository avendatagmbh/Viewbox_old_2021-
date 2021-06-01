using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DbAccess;
using DbSearchDatabase.Results;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.Structures.Result.DataMapper {
    internal class ResultSetLoader {
        private Dictionary<int, ResultSet> _idToResultSet=new Dictionary<int, ResultSet>(); 

        static ResultSetLoader() {
            
        }

        public void LoadResultSets(ResultHistory history) {
            using (IDatabase conn = ConnectionManager.CreateConnection(history.Query.Profile.DbProfile.GetProfileConfig())) {
                conn.Open();

                List<DbResultSet> dbResultSets = conn.DbMapping.Load<DbResultSet>("query_id=" + history.Query.DbQuery.Id);
                //Sort according to date created
                dbResultSets.Sort((resultSet1, resultSet2) => (resultSet1.Timestamp.CompareTo(resultSet2.Timestamp)));
                ResultSet lastResultSet = null;
                foreach (var dbResultSet in dbResultSets) {
                    Query snapshotQuery = Query.FromXml(history.Query.Profile, dbResultSet.QuerySnapshot);
                    if (!_idToResultSet.ContainsKey(dbResultSet.Id)) _idToResultSet[dbResultSet.Id] = new ResultSet(snapshotQuery, dbResultSet);
                    history.AddResultSet(_idToResultSet[dbResultSet.Id]);
                    lastResultSet = _idToResultSet[dbResultSet.Id];
                }
                history.RecentResults = lastResultSet;
            }
            
        }
    }
}
