// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearchLogic.SearchCore.Structures.Result {
    public class ColumnResult : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        public event CollectionChangeEventHandler ColumnHitsChanged;
        private void OnColumnHitsChanged(IEnumerable<ColumnHitInfo> hits) {
            if(ColumnHitsChanged != null) ColumnHitsChanged(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, hits));
        }
        #endregion Events

        #region Constructor
        public ColumnResult(Column column) {
            //ColumnHits = new ObservableCollectionAsync<TableHit>();
            //ColumnHits = new ObservableCollectionAsync<ColumnHitInfo>();
            ColumnHits = new List<ColumnHitInfo>();
            Column = column;
        }
        #endregion

        #region Properties

        private bool _loaded = false;
        public List<ColumnHitInfo> ColumnHits { get; set; }
        public Column Column { get; set; }
        public string Name { get { return Column.Name; } }
        public int TableHitsCount {
            get {
                return TableHits(0.0f);
            }
        }
        #endregion

        #region Methods
        public void AddTableResult(TableResultSet tableResult) {
            if (tableResult == null) return;
            //Convert the tableResult
            foreach (var columnHit in tableResult.ColumnHits) {
                if (columnHit.SearchColumnName == Column.Name) {
                    lock (ColumnHits) {
                        ColumnHits.AddRange(columnHit.TableColumns);
                    }
                    OnColumnHitsChanged(columnHit.TableColumns);
                }
            }
            OnPropertyChanged("ColumnHits");
            OnPropertyChanged("TableHitsCount");
        }

        public void Load() {
            if (!_loaded) {

                _loaded = true;
            }            
        }

        public void AddColumnHit(ColumnHitInfo hitInfo) {
            lock (ColumnHits) {
                ColumnHits.Add(hitInfo);
            }
            OnPropertyChanged("ColumnHits");
            OnPropertyChanged("TableHitsCount");
        }

        public override string ToString() {
            return "ColumnResult: " + Column.Name + ", Hits: " + ColumnHits.Count;
        }

        public int TableHits(float threshold) {
            HashSet<TableInfo> tables = new HashSet<TableInfo>();
            int result = 0;
            lock (ColumnHits) {
                foreach (var columnHit in ColumnHits) {
                    if (columnHit.HitPercentage >= threshold && !tables.Contains(columnHit.TableInfo)) {
                        result++;
                        tables.Add(columnHit.TableInfo);
                    }
                }
            }
            return result;
        }
        #endregion Methods

    }
}
