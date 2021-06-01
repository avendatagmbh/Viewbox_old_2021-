using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AvdCommon.Logging;
using DbAccess;
using DbSearchDatabase.Results;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.Structures.TableRelated;
using System.Linq;

namespace DbSearchLogic.SearchCore.Structures.Result.DataMapper {
    public class SearchValueResultMapper {
        //Store for each SearchValueResult DbSearchValueResult in the database
        //private static readonly Dictionary<SearchValueResult, DbSearchValueResult> _searchValueResultToId = new Dictionary<SearchValueResult, DbSearchValueResult>();
        //As there may be more then one corresponding DbSearchValueResults, we store these SearchValueResults in a seperate dictionary as this case does not happen often (only if Hits.Count > MAX_IDS_PER_ENTRY)
        //private static readonly Dictionary<SearchValueResult, List<DbSearchValueResult>> _searchValueResultToIds = new Dictionary<SearchValueResult, List<DbSearchValueResult>>();


        private static int MAX_IDS_PER_ENTRY = 100000;
        //private static int MAX_IDS_PER_ENTRY = 2;
        public static IEnumerable<DbSearchValueResult> Map(SearchValueResult searchValueResult, ResultSet resultSet, DbColumnHitInfo dbColumnHitInfo) {
            List<DbSearchValueResult> results = new List<DbSearchValueResult>();

            byte[] ids = searchValueResult.Ids.ToArray();
            byte[] types = searchValueResult.Types.ToArray();

            for (int run = 0; run <= searchValueResult.Hits / MAX_IDS_PER_ENTRY; run++) {
                DbSearchValueResult dbSearchValueResult = new DbSearchValueResult {
                                                                                      DbQuery = (DbQuery) resultSet.SnapshotQuery.DbQuery,
                                                                                      DbResultSet = (DbResultSet) resultSet.DbResultSet,
                                                                                      DbColumnHitInfoId = dbColumnHitInfo.Id,
                                                                                      DbColumnHitInfo = dbColumnHitInfo,
                                                                                      RowEntryId = searchValueResult.SearchValue.RowEntryId
                                                                                  };

                using (MemoryStream idStream = new MemoryStream(), typeStream = new MemoryStream(), firstHitStream = new MemoryStream()) {
                    idStream.Write(ids,run*MAX_IDS_PER_ENTRY*4, Math.Min(ids.Length-run*4*MAX_IDS_PER_ENTRY,4*(MAX_IDS_PER_ENTRY)));
                    typeStream.Write(types, run * MAX_IDS_PER_ENTRY, Math.Min(types.Length - run * MAX_IDS_PER_ENTRY, (MAX_IDS_PER_ENTRY)));
                    if (run == 0 && ids.Length > 0) {
                        firstHitStream.Write(ids, 0, 4);
                        firstHitStream.Write(types, 0, 1);
                    }
                    dbSearchValueResult.HitIds = idStream.ToArray();
                    dbSearchValueResult.EntryTypes = typeStream.ToArray();
                    dbSearchValueResult.FirstHit = firstHitStream.ToArray();
                }
                //TODO
                //using (MemoryStream idStream = new MemoryStream(), typeStream = new MemoryStream(), firstHitStream = new MemoryStream()) {
                //    for (int i = run * MAX_IDS_PER_ENTRY; i < Math.Min(searchValueResult.Hits, (run + 1) * MAX_IDS_PER_ENTRY); ++i) {
                //        idStream.Write(BitConverter.GetBytes(searchValueResult.Hits[i].Id), 0, 4);
                //        typeStream.Write(BitConverter.GetBytes((int)searchValueResult.Hits[i].Type), 0, 1);

                //        //Write only the first hit
                //        if (run == 0 && i == 0) {
                //            firstHitStream.Write(BitConverter.GetBytes(searchValueResult.Hits[i].Id), 0, 4);
                //            firstHitStream.Write(BitConverter.GetBytes((int)searchValueResult.Hits[i].Type), 0, 1);
                //        }
                //   }
                //    dbSearchValueResult.HitIds = idStream.ToArray();
                //    dbSearchValueResult.EntryTypes = typeStream.ToArray();
                //    dbSearchValueResult.FirstHit = firstHitStream.ToArray();
                //}

                //Establish mapping between searchvalueresult and database part
                //if (searchValueResult.Hits / MAX_IDS_PER_ENTRY == 0)
                //    ;
                ////_searchValueResultToId[searchValueResult] = dbSearchValueResult;
                //else
                //{
                //    if (run == 0) _searchValueResultToIds[searchValueResult] = new List<DbSearchValueResult>();
                //    _searchValueResultToIds[searchValueResult].Add(dbSearchValueResult);
                //}

                results.Add(dbSearchValueResult);
            }
            return results;
        }

        //Need to map a list of DbSearchValueResults to SearchValueResults as there may be more then one entry in the database corresponding to only one SearchValueResult instance,
        //this is the case if there are more hits then MAX_IDS_PER_ENTRY
        public static List<SearchValueResult> Map(IEnumerable<DbSearchValueResult> dbSearchValueResults, Dictionary<int, SearchValue> idToSearchValue) {
            List<SearchValueResult> searchValueResults = new List<SearchValueResult>();
            Dictionary<int, SearchValueResult> rowIdToSearchValueResult = new Dictionary<int, SearchValueResult>();
            foreach (var dbSearchValueResult in dbSearchValueResults) {
                SearchValueResult svr;
                if (!rowIdToSearchValueResult.TryGetValue(dbSearchValueResult.RowEntryId, out svr)) {
                    svr = new SearchValueResult(idToSearchValue[dbSearchValueResult.RowEntryId]);// { DbId = dbSearchValueResult.Id };
                    //svr.DbIds.Add(dbSearchValueResult.Id);
                    rowIdToSearchValueResult[dbSearchValueResult.RowEntryId] = svr;
                    searchValueResults.Add(svr);
                    //_searchValueResultToId.Add(svr, dbSearchValueResult);
                } else {
                }
                svr.DbIds.Add(dbSearchValueResult.Id);
                //else {
                //    //This SearchValueResult has more hits then MAX_IDS_PER_ENTRY and therefore at least two entries in the database exist
                //    List<DbSearchValueResult> storedDbSearchValueResult;
                //    if(!_searchValueResultToIds.TryGetValue(svr, out storedDbSearchValueResult)) {
                //        _searchValueResultToIds[svr] = new List<DbSearchValueResult>();
                //        storedDbSearchValueResult = _searchValueResultToIds[svr];
                //    }
                //    storedDbSearchValueResult.Add(dbSearchValueResult);
                //}

                if (dbSearchValueResult.FirstHit.Length == 5) {
                    svr.AddHit(BitConverter.ToUInt32(dbSearchValueResult.FirstHit, 0),
                               (SearchValueResultEntryType)
                               Enum.Parse(typeof (SearchValueResultEntryType),
                                          dbSearchValueResult.FirstHit[4].ToString()));
                }
            }
            return searchValueResults;
        }

        public static void LoadAllHits(List<SearchValueResult> searchValueResults, Query query) {
            using (IDatabase conn = ConnectionManager.CreateConnection(query.Profile.DbProfile.GetProfileConfig())) {
                conn.Open();
                conn.SetHighTimeout();

                List<long> idsToLoad = new List<long>();
                foreach (var svr in searchValueResults) {
                    idsToLoad.AddRange(svr.DbIds);
                    //if (svr.DbId != 0) idsToLoad.Add(svr.DbId);
                }
                List<DbSearchValueResult> loadedDbSearchValueResults = new List<DbSearchValueResult>();
                if(idsToLoad.Count > 0)
                    loadedDbSearchValueResults = conn.DbMapping.Load<DbSearchValueResult>("id in (" + string.Join(",", idsToLoad) + ")", true);

                foreach(var svr in searchValueResults) {
                    svr.Clear();
                    List<DbSearchValueResult> dbSearchValueResults = new List<DbSearchValueResult>();
                    
                    //if(_searchValueResultToId.TryGetValue(svr, out storedDbSearchValueResult)) {
                    //    dbSearchValueResults = conn.DbMapping.Load<DbSearchValueResult>("id=" + storedDbSearchValueResult.Id, true);
                    //}else {
                    dbSearchValueResults = loadedDbSearchValueResults.Where(t => svr.DbIds.Contains(t.Id)).ToList();
                    //if (svr.DbId != 0) {
                    //    dbSearchValueResults = loadedDbSearchValueResults.Where(t => t.Id == svr.DbId).ToList();
                    //}
                    //else{
                    //    //List<DbSearchValueResult> dbSearchValueResultsStored;
                    //    //if (_searchValueResultToIds.TryGetValue(svr, out dbSearchValueResultsStored))
                    //    //{
                    //    //    List<long> ids = new List<long>();
                    //    //    foreach (var dbSearchValueResult in dbSearchValueResultsStored) ids.Add(dbSearchValueResult.Id);

                    //    //    dbSearchValueResults = conn.DbMapping.Load<DbSearchValueResult>("id in (" + string.Join(",", ids) + ")", true);
                    //    //}
                    //    //else
                    //    //{
                    //        LogManager.Warning("Konnte das Suchergebnis nicht in der internen Liste finden");
                    //        continue;
                    //    //}
                    //}
                    foreach (var dbSearchValueResult in dbSearchValueResults) {
                        //Add the hits which can be restored from the byte streams
                        for (int i = 0; i < dbSearchValueResult.EntryTypes.Length; i++) {
                            svr.AddHit(BitConverter.ToUInt32(dbSearchValueResult.HitIds, i*4),
                                (SearchValueResultEntryType) Enum.Parse(typeof(SearchValueResultEntryType), dbSearchValueResult.EntryTypes[i].ToString())
                            );
                        }
                    }
                }
            }
        }
    }
}
