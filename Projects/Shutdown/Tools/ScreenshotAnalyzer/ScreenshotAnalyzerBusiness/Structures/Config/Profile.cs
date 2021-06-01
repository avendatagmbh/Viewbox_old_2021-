// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using AvdCommon.Logging;
using AvdCommon.Rules;
using DbAccess.Structures;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Resources;
using ScreenshotAnalyzerDatabase.Factories;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;

namespace ScreenshotAnalyzerBusiness.Structures.Config {
    public class Profile : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public Profile(IDbProfile dbProfile, string name) {
            DbProfile = dbProfile;
            DbProfile.Name = name;
            DbConfig.Hostname = ApplicationManager.ConfigDirectory + "\\" + Name + ".db3";
            Tables = new ObservableCollectionAsync<Table>();
        }

        #endregion


        #region Properties
        public IDbProfile DbProfile { get; set; }
        public bool DatabaseTooOld { get { return DbProfile.DatabaseTooOld; } }
        public bool DatabaseTooNew { get { return DbProfile.DatabaseTooNew; } }
        public string FaultedOnLoad { get; set; }
        public bool IsStatusOk { get { return !DatabaseTooOld && !DatabaseTooNew && string.IsNullOrEmpty(FaultedOnLoad); } }
        public ObservableCollectionAsync<Table> Tables { get; set; }
        private bool _isLoaded;

        #region Name
        public string Name {
            get { return DbProfile.Name; }
            //set {
            //    if (Name != value) {
            //        DbProfile.Name = value;
            //        OnPropertyChanged("Name");
            //    }
            //}
        }
        #endregion

        #region Description
        #endregion
        public string Description {
            get { return DbProfile.Description; }
            set {
                if (Description != value) {
                    DbProfile.Description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public string DisplayString {
            get {
                string error = string.Empty;
                if (DatabaseTooNew) error = ResourcesBusiness.Profile_DisplayString_inactive_ProgramVersionTooOld;
                else if (DatabaseTooOld) error = ResourcesBusiness.Profile_DisplayString_inactive_DbVersionTooOld;
                else if (!string.IsNullOrEmpty(FaultedOnLoad)) error = ResourcesBusiness.Profile_DisplayString_inactive_LoadFailed;
                return Name + error;
            }
        }

        public DbConfig DbConfig {
            get { return DbProfile.DbConfig; }
        }

        #region AccessPath
        //private string _accessPath;
        public string AccessPath {
            get {
                return DbProfile.AccessPath;
            }
            set {
                if (DbProfile.AccessPath != value) {
                    DbProfile.AccessPath = value;
                    OnPropertyChanged("AccessPath");
                }
            }
        }
        #endregion AccessPath

        #endregion

        #region Methods
        public void Load() {
            if (_isLoaded) return;
            try {
                DbProfile.Load();
                Tables.Clear();
                foreach (var dbTable in DbProfile.Tables) {
                    Table table = new Table(this, dbTable);
                    Tables.Add(table);
                }
            } catch (Exception ex) {
                FaultedOnLoad = ex.Message;
                return;
            }
            _isLoaded = true;
        }

        public Table AddTable() {
            Table table = new Table(this, DatabaseObjectFactory.CreateTable(DbProfile));
            Tables.Add(table);
            return table;
        }

        public void RemoveTable(Table table) {
            table.DbTable.Delete();
            Tables.Remove(table);
        }
        #endregion
    }
}
