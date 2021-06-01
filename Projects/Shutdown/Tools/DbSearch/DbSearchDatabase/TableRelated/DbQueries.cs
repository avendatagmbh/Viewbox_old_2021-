// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.Config;
using DbSearchDatabase.DistinctDb;
using DbSearchDatabase.Interfaces;
using log4net;
using AV.Log;

namespace DbSearchDatabase.TableRelated {

    internal class DbQueries : IDbQueries {

        internal static ILog _log = LogHelper.GetLogger();

        #region Constructor
        public DbQueries(IDbProfile dbProfile) {
            DbProfile = (DbProfile) dbProfile;
            //Items = new ObservableCollection<IDbQuery>();
        }
        #endregion

        #region Properties
        private DbProfile DbProfile { get; set; }
        //public ObservableCollection<IDbQuery> Items { get; set; }
        #endregion

        #region Methods
        public void AddFromValidationDatabase(List<IDbQuery> items, List<ImportTable> tables) {
            using (IDatabase profileConn = DbProfile.GetOpenConnection()) {
                List<string> queryTableNames = new List<string>();
                foreach (var table in tables) {
                    if (!table.Use) continue;
                    _log.Log(LogLevelEnum.Info, "Beginne Import der Verprobungstabelle \"" + table.ValidationPath + "\"", true);

                    DbConfig validationConfig = new DbConfig("Access") { Hostname = table.ValidationPath };
                    using (IDatabase queryConn = ConnectionManager.CreateConnection(validationConfig)) {
                        queryConn.Open();
                        DbQuery query = new DbQuery(DbProfile, table.Name);

                        profileConn.DbMapping.Save(query);
                        query.CopyData(queryConn, profileConn);

                        items.Add(query);
                        queryTableNames.Add(query.TableName);

                    }
                }
                _log.Log(LogLevelEnum.Info, "Erstelle Distinct Datenbank", true);
                //Create Distinct database of the added queries
                DbDistincter distincter = new DbDistincter(profileConn.DbConfig, 4, queryTableNames);
                distincter.Start(true);
            }

            _log.Log(LogLevelEnum.Info, "Import abgeschlossen", true);
        }

        public IEnumerable<IDbQuery> Load() {
            using (IDatabase conn = DbProfile.GetOpenConnection()) {
                IEnumerable<DbQuery> queries = conn.DbMapping.Load<DbQuery>();
                foreach (var query in queries) {
                    query.DbProfile = DbProfile;
                }
                return queries;
            }
        }

        public void DeleteQuery(IDbQuery dbQuery) {
            using (IDatabase profileConn = DbProfile.GetOpenConnection(), distinctConn = DbProfile.GetDistinctConnection()) {
                ((DbQuery) dbQuery).Delete(profileConn, distinctConn);
            }
        }

        #endregion Methods
    }
}
