using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitProductManager.Business.Structures.DbMapping;
using System.Collections.ObjectModel;

namespace eBalanceKitProductManager.Business.Manager {
    
    internal static class VersionManager {

        static VersionManager() {
            Versions = new ObservableCollection<Structures.DbMapping.Version>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = conn.DbMapping.LoadSorted<eBalanceKitProductManager.Business.Structures.DbMapping.Version>("since");
                foreach (var version in tmp) {
                    Versions.Insert(0, version);
                }
            }
        }

        public static ObservableCollection<eBalanceKitProductManager.Business.Structures.DbMapping.Version> Versions { get; private set; }
    }
}
