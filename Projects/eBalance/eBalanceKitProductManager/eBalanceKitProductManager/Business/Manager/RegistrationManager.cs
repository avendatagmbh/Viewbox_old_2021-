using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitProductManager.Business.Structures.DbMapping;
using System.Collections.ObjectModel;

namespace eBalanceKitProductManager.Business.Manager {

    internal static class RegistrationManager {

        static RegistrationManager() {
            Registrations = new ObservableCollection<Registration>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = conn.DbMapping.Load<Registration>();
                foreach (var registration in tmp) {
                    Registrations.Add(registration);
                }
            }
        }

        public static ObservableCollection<Registration> Registrations { get; private set; }
    }
}
