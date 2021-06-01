// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Data;
using DbAccess;
using DbAccess.Structures;

namespace RegKeyGenerator {
    public class RegViewerModel : Utils.NotifyPropertyChangedBase {

        public RegViewerModel() { SetRegistrations(); }

        public void SetRegistrations() {
            try {
                DbConfig dbConfig = new DbConfig("MySQL") { Hostname = "profiledb", Username = "ebkreg", Password = "Eng6glal" };
                using (var conn = ConnectionManager.CreateConnection(dbConfig)) {
                    Registrations = conn.GetDataTable("SELECT * FROM `e_balance_kit_registration`.`regdata`");
                }
            } catch (Exception ex) {
                
            }
        }

        private DataTable _registrations;

        public DataTable Registrations {
            get { return _registrations; }
            set {
                _registrations = value;
                OnPropertyChanged("Registrations");
            }
        }
    }
}