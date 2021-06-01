// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Utils;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Manager {
    /// <summary>
    /// This class provides several period management functions.
    /// </summary>
    public class SystemManager : NotifyPropertyChangedBase {

        public static SystemManager Instance { get { return _instance ?? (_instance = new SystemManager()); } }

        private static SystemManager _instance;

        private SystemManager() {
            _systems = new ObservableCollection<Structures.DbMapping.System>();
            Systems.CollectionChanged += SystemsOnCollectionChanged;
        }

        private void SystemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            OnPropertyChanged("HasAllowedSystems");
            OnPropertyChanged("HasDeletableSystems");
        }

        #region properties

        #region Systems
        private readonly ObservableCollection<Structures.DbMapping.System> _systems;
        public ObservableCollection<Structures.DbMapping.System> Systems { get { return _systems; } }
        #endregion

        #region SystemById
        private readonly Dictionary<int, Structures.DbMapping.System> _systemById =
            new Dictionary<int, Structures.DbMapping.System>();

        public Dictionary<int, Structures.DbMapping.System> SystemById { get { return _systemById; } }
        #endregion

        #region HasAllowedSystems
        public bool HasAllowedSystems { get { return _systems.Any(); } }
        #endregion // HasAllowedSystems

        #region HasDeletableSystems
        public bool HasDeletableSystems { get { return _systems.Any(s => !s.HasAnyAssignedDocument); } }
        #endregion // HasDeletableSystems

        #endregion properties

        #region methods

        #region Init
        public static void Init() {
            Instance.Systems.Clear();
            Instance.SystemById.Clear();

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = conn.DbMapping.Load<Structures.DbMapping.System>();
                tmp.Sort();

                foreach (Structures.DbMapping.System system in tmp) {
                    LogManager.Instance.NewSystem(system, true);
                    Instance.SystemById[system.Id] = system;
                }

                Instance.Systems.Clear();
                foreach (Structures.DbMapping.System c in tmp) Instance.Systems.Add(c);
            }

            Instance.OnPropertyChanged("HasAllowedSystems");
            Instance.OnPropertyChanged("HasDeletableSystems");
        }
        #endregion

        #region AddSystem
        public void AddSystem(Structures.DbMapping.System system) {
            if (system.IsTempObject) return;

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {
                        conn.DbMapping.Save(system);
                        LogManager.Instance.NewSystem(system, false);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }

                    // sorted insert into observable collection
                    var tmp = new List<Structures.DbMapping.System>(Systems);
                    tmp.Add(system);
                    tmp.Sort();
                    Systems.Clear();
                    foreach (Structures.DbMapping.System s in tmp) Systems.Add(s);

                    SystemById[system.Id] = system;
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.SystemManagerAdd + ex.Message);
                }
            }

            OnPropertyChanged("HasAllowedSystems");
            OnPropertyChanged("HasDeletableSystems");
        }
        #endregion

        #region DeleteSystem
        public void DeleteSystem(Structures.DbMapping.System system) {
            if (system.IsTempObject) return;

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                if (conn.DbMapping.ExistsValue(typeof (Document), conn.Enquote("system_id") + "=" + system.Id)) {
                    throw new Exception(ExceptionMessages.SystemInUse);
                }

                conn.BeginTransaction();
                try {
                    LogManager.Instance.DeleteSystem(system);
                    conn.DbMapping.Delete(system);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ExceptionMessages.SystemManagerDelete + ex.Message);
                }

                Systems.Remove(system);
                SystemById.Remove(system.Id);
            }

            OnPropertyChanged("HasAllowedSystems");
            OnPropertyChanged("HasDeletableSystems");
        }
        #endregion

        #region UpdateSystem
        public void UpdateSystem(Structures.DbMapping.System system) {
            if (system.IsTempObject) return;

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    conn.DbMapping.Save(system);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ExceptionMessages.SystemManagerUpdate + ex.Message);
                }
            }
        }
        #endregion

        #region GetSystem
        public Structures.DbMapping.System GetSystem(int id) { return SystemById.ContainsKey(id) ? SystemById[id] : null; }
        #endregion

        #region Exists
        public bool Exists(Structures.DbMapping.System system) {
            if (String.IsNullOrEmpty(system.Name)) return false;
            return Systems.Any(sys => sys.Id != system.Id && sys.Name != null && sys.Name.ToLower().Trim().Equals(system.Name.ToLower().Trim())); 
        }
        #endregion Exists

        public void NotifyAssignedReportChanged() { OnPropertyChanged("HasDeletableSystems"); }
 
        #endregion methods
    }
}