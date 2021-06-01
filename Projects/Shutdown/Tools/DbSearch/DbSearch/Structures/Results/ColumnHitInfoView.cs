// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-23
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Structures.Results {
    #region ColumnResultViewComparer

    public class ColumnResultViewComparer : IEqualityComparer<ColumnHitInfoView> {
        public bool IncludeColumn { get; set; }
        public bool Equals(ColumnHitInfoView x, ColumnHitInfoView y) {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            if (IncludeColumn)
                return x.TableName == y.TableName && x.ColumnHitInfo.ColumnName == y.ColumnHitInfo.ColumnName && x.SearchColumn.Name == y.SearchColumn.Name;
            return x.TableName == y.TableName;
            //return x.TableName == y.TableName && x.ColumnName == y.ColumnName;
        }

        public int GetHashCode(ColumnHitInfoView obj) {
            if (IncludeColumn)
                return obj.TableName.GetHashCode() ^ obj.ColumnHitInfo.ColumnName.GetHashCode() ^ obj.SearchColumn.Name.GetHashCode();
            return obj.TableName.GetHashCode();
        }
    }
    #endregion ColumnResultViewComparer

    public class ColumnHitInfoView : NotifyPropertyChangedBase {
        #region Id
        private int _id;
        public int Id {
            get { return _id; }
            set {
                if (_id != value) {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        #endregion Id

        public TableInfo TableInfo { get; private set; }
        public string TableName { get { return TableInfo.Name; } }

        #region ColumnInfo
        private ColumnInfo _columnInfo;
        public ColumnInfo ColumnInfo {
            get {
                if (_columnInfo == null) {
                    _columnInfo = TableInfo.GetColumnInfo(ColumnHitInfo.ColumnName);
                }
                return _columnInfo;
            }
        }
        #endregion ColumnInfo
        //public string ColumnName { get { return ColumnHitInfo.ColumnName; } }
        public string Hits { get; private set; }
        public Column SearchColumn { get; set; }
        private Query Query { get; set; }
        public ColumnHitInfo ColumnHitInfo { get; private set; }

        public ColumnHitInfoView(int id, TableInfo tableInfo, ColumnHitInfo hitInfo, Column searchColumn, Query query) {
            Id = id;
            TableInfo = tableInfo;
            ColumnHitInfo = hitInfo;
            Hits = ColumnHitInfo.HitCount + " / " + (ColumnHitInfo.HitCount + ColumnHitInfo.MissingValuesCount);
            SearchColumn = searchColumn;
            Query = query;

            Query.UserColumnMappings.ColumnMappings.CollectionChanged += ColumnMappings_CollectionChanged;
        }

        void ColumnMappings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged("ColumnMappingAllowed"); 
            OnPropertyChanged("ColumnMapped");
        }


        public bool ColumnMappingAllowed {
            get { return Query.UserColumnMappings.MappingAllowed(new ColumnMapping(SearchColumn, TableInfo, ColumnInfo.Name)); }
        }

        public bool ColumnMapped {
            get { return Query.UserColumnMappings.HasMapping(new ColumnMapping(SearchColumn, TableInfo, ColumnInfo.Name)); }
        }


        public void AddMapping() {
            Query.UserColumnMappings.AddMapping(SearchColumn, TableInfo, ColumnInfo.Name);
        }

        public void RemoveMapping() {
            Query.UserColumnMappings.RemoveMapping(SearchColumn, TableInfo, ColumnInfo.Name);
        }

        public override string ToString() {
            return SearchColumn.Name + " -> " + TableName + "." + ColumnInfo.Name;
        }
    }
}
