using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using AvdCommon.Logging;
using AvdCommon.Rules;
using DbAccess.Structures;
using DbSearchBase.Interfaces;
using DbSearchDatabase.Interfaces;
using DbSearchLogic.Manager;
using DbSearchLogic.SearchCore.Config;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.Config {
    public class Profile : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public Profile(IDbProfile dbProfile) {
            DbProfile = dbProfile;
            Queries = new Queries(this);
            CustomRules = new RuleSet();
            RuleManager.SetProfileRules(this);
            CustomRules.AllRules.CollectionChanged += AllRules_CollectionChanged;
            TableInfoManager = new TableInfoManager(this);
            GlobalSearchConfig = new GlobalSearchConfig(){MaxStringLength = 100000};
        }

        #endregion

        #region Properties
        public IDbProfile DbProfile { get; set; }
        private bool _loaded;


        public bool DatabaseTooOld { get { return DbProfile.DatabaseTooOld; } }
        public bool DatabaseTooNew { get { return DbProfile.DatabaseTooNew; } }
        public string FaultedOnLoad { get; set; }
        public bool IsStatusOk { get { return !DatabaseTooOld && !DatabaseTooNew && string.IsNullOrEmpty(FaultedOnLoad); } }

        #region Name
        public string Name {
            get { return DbProfile.Name; }
            set {
                if (Name != value) {
                    DbProfile.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion

        #region Description
        #endregion
        public string Description {
            get { return DbProfile.Description; }
            set {
                if(Description != value) {
                    DbProfile.Description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public string DisplayString {
            get {
                string error = string.Empty;
                if (DatabaseTooNew) error = " (inaktiv: DbSearch Version ist zu alt)";
                else if (DatabaseTooOld) error = " (inaktiv: Datenbank muss upgegradet werden)";
                else if (!string.IsNullOrEmpty(FaultedOnLoad)) error = " (inaktiv: Fehler beim Laden)";
                return Name + error;
            }
        }

        public Queries Queries { get; set; }

        public DbConfig DbConfigView {
            get { return DbProfile.DbConfigView; }
        }

        //public Dispatcher BackgroundDispatcher { get; set; }
        public RuleSet CustomRules { get; private set; }
        public TableInfoManager TableInfoManager { get; private set; }
        public GlobalSearchConfig GlobalSearchConfig { get; set; }
        #endregion

        #region Methods
        public void Load() {
            if (_loaded) return;
            Queries.Load();
            try {
                CustomRules.FromXml(DbProfile.CustomRules);
            } catch (Exception ex) {
                LogManager.Error("Konnte die Regeln des Profils " + Name + " nicht laden" + Environment.NewLine + ex.Message);                
            }
            _loaded = true;
        }
        #endregion

        #region EventHandler
        void AllRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            DbProfile.CustomRules = CustomRules.ToXml();
            DbProfile.Save();
        }
        #endregion EventHandler

        public void UpdateDatabase() {
            if (!DatabaseTooOld) return;
            DbProfile.UpdateDatabase();

            OnPropertyChanged("DatabaseTooOld");
            OnPropertyChanged("IsStatusOk");
            OnPropertyChanged("DisplayString");
        }
    }
}
