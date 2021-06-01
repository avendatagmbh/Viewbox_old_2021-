using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using eBalanceKitProductManager.Business.Structures.Enumerations;
using System.ComponentModel;

namespace eBalanceKitProductManager.Business.Structures.DbMapping {
    
    [DbTable("instances")]
    public class Instance : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

        [DbColumn("id", AllowDbNull = false, AutoIncrement = true)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("serial", AllowDbNull = false, Length = 25)]
        public string SerialNumber {
            get { return _serial; }
            set { 
                _serial = value;
                OnPropertyChanged("SerialNumber");
            }
        }
        public string _serial;

        [DbColumn("company_name", AllowDbNull = true, Length = 64)]
        public string CompanyName {
            get { return _companyName; }
            set { 
                _companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }
        public string _companyName;

        [DbColumn("company_town", AllowDbNull = true, Length = 64)]
        public string CompanyTown { get; set; }

        [DbColumn("company_street", AllowDbNull = true, Length = 128)]
        public string CompanyStreet { get; set; }

        [DbColumn("company_housenumer", AllowDbNull = true, Length = 10)]
        public string CompanyHousenumber { get; set; }

        [DbColumn("company_zip", AllowDbNull = true)]
        public int CompanyZip { get; set; }

        [DbColumn("contact_forename", AllowDbNull = true, Length = 64)]
        public string ContactForename { get; set; }

        [DbColumn("contact_surename", AllowDbNull = true, Length = 64)]
        public string ContactSurename { get; set; }

        [DbColumn("contact_email", AllowDbNull = true, Length = 128)]
        public string ContactEMail { get; set; }

        [DbColumn("version_id")]
        public Version Version { get; set; }

        [DbColumn("timestamp", AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

        [DbColumn("medium", AllowDbNull = false)]
        public EProductionStatus Medium { get; set; }

        [DbColumn("manual", AllowDbNull = false)]
        public EProductionStatus Manual { get; set; }

    }
}
