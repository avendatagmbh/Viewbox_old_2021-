using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DbAccess;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Manager
{
    public static class FederalGazetteInfoManager
    {
        public static ObservableCollection<FederalGazetteInfo> ActiveFederalGazettes { get; private set; }


        public static void Init() {
            ActiveFederalGazettes = new ObservableCollection<FederalGazetteInfo>();
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = conn.DbMapping.Load<FederalGazetteInfo>();
                tmp.Sort();
                foreach (var gazetteInfo in tmp) {
                    ActiveFederalGazettes.Add(gazetteInfo);
                }
            }
        }


        public static void AddFederalGazette(FederalGazetteInfo federalGazetteInfo) {
            using (IDatabase conn= AppConfig.ConnectionManager.GetConnection()) {
                try
                {                    
                    conn.BeginTransaction();
                    conn.DbMapping.Save(federalGazetteInfo);
                    
                    var tmp = new List<FederalGazetteInfo>(ActiveFederalGazettes);
                    tmp.Add(federalGazetteInfo);
                    tmp.Sort();
                    ActiveFederalGazettes.Clear();
                    foreach (var gazetteInfo in tmp) ActiveFederalGazettes.Add(gazetteInfo);
                    
                    conn.CommitTransaction();
                }
                catch (Exception e) {
                    conn.RollbackTransaction();
                    throw new Exception(e.Message);
                }
            }
        }

        public static void DeleteFederalGazette(FederalGazetteInfo federalGazetteInfo) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    federalGazetteInfo.AccountDeleted = true;
                    conn.DbMapping.Save(federalGazetteInfo);
                    ActiveFederalGazettes.Remove(federalGazetteInfo);
                    conn.CommitTransaction();

                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }
        }

        public static void UpdateFederalGazette(FederalGazetteInfo federalGazetteInfo) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Save(federalGazetteInfo);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }
        }

        
        
    }
}
