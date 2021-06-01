using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvdCommon.Rules;
using AvdCommon.Rules.Factories;
using DbAccess.Structures;
using DbSearchBase.Enums;
using DbSearchDatabase.Interfaces;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures;
using log4net;
using AV.Log;

namespace DbSearchLogic.Structures.TableRelated {
    public class Column : INotifyPropertyChanged {

        internal ILog _log = LogHelper.GetLogger();

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public Column(Query query, IDbColumn dbColumn, int displayIndex) {
            DbColumn = dbColumn;
            DisplayIndex = displayIndex;
            //IsVisible = true;
            SearchParams = new ConfigSearchParams(query.SearchParams);
            SearchParams.FromDbSearchParams(DbColumn.DbConfigSearchParams);
            Query = query;
            query.UserColumnMappings.ColumnMappings.CollectionChanged += ColumnMappings_CollectionChanged;
            RuleSet = new RuleSet();

            //Read back rules if present
            if (!string.IsNullOrEmpty(dbColumn.Rules)) {
                try {
                    RuleSet.FromXml(dbColumn.Rules);
                } catch (Exception ex) {
                    _log.Log(LogLevelEnum.Error, "Konnte Regeln der Spalte " + Name + " nicht lesen." + Environment.NewLine + ex.Message);
                }
            }
        }

        #endregion

        #region EventHandler
        void ColumnMappings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MappedTo = Query.UserColumnMappings.GetMapping(this);
        }
        #endregion EventHandler


        #region Properties
        public RuleSet RuleSet { get; set; }
        internal IDbColumn DbColumn { get; set; }
        public Query Query { get; private set; }

        public DbColumnInfo DbColumnInfo {
            get { return DbColumn.DbColumnInfo; }
        }


        #region IsVisible
        public bool IsVisible {
            get { return DbColumn.IsVisible; }
            set {
                if (IsVisible != value) {
                    DbColumn.IsVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }
        #endregion IsVisible

        #region IsUsedInSearch
        public bool IsUsedInSearch {
            get { return DbColumn.IsUsedInSearch; }
            set {
                if (IsUsedInSearch != value) {
                    DbColumn.IsUsedInSearch = value;
                    OnPropertyChanged("IsUsedInSearch");
                }
            }
        }
        #endregion IsUsedInSearch

        #region IsSelected
        public bool IsSelected {
            get { return DbColumn.IsSelected; }
            set {
                if (IsSelected != value) {
                    DbColumn.IsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion IsSelected

        #region Comment
        public string Comment {
            get { return DbColumn.Comment; }
            set {
                if (Comment != value) {
                    DbColumn.Comment = value;
                    OnPropertyChanged("Comment");
                }
            }
        }
        #endregion Comment

        #region DisplayIndex
        public int DisplayIndex {
            get { return DbColumn.DisplayIndex; }
            set {
                if (DisplayIndex != value) {
                    DbColumn.DisplayIndex = value;
                    OnPropertyChanged("DisplayIndex");
                }
            }
        }
        #endregion DisplayIndex

        #region IsUserDefined
        public bool IsUserDefined {
            get { return DbColumn.IsUserDefined; }
            set {
                if (IsUserDefined != value) {
                    DbColumn.IsUserDefined = value;
                    OnPropertyChanged("IsUserDefined");
                }
            }
        }
        #endregion

        #region MappedTo
        private ColumnMapping _mappedTo;
        public ColumnMapping MappedTo {
            get { return _mappedTo; }
            set {
                if (_mappedTo != value) {
                    _mappedTo = value;
                    OnPropertyChanged("MappedTo");
                }
            }
        }
        #endregion

        public ConfigSearchParams SearchParams { get; set; }

        public string Name {
            get { return DbColumn.Name; }
            set { DbColumn.Name = value; }
        }

        public string OriginalColumnName {
            get { return DbColumn.OriginalColumnName; }
            set { DbColumn.OriginalColumnName = value; }
        }

        #endregion

        #region Methods
        public override bool Equals(object obj) {
            Column other = obj as Column;
            if (obj == null || other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }
        #endregion Methods

        //public Column Clone(Query query) {
        //    Column column = new Column(query, DbColumn.Clone(), DisplayIndex);
        //    return column;
        //}
    }
}
