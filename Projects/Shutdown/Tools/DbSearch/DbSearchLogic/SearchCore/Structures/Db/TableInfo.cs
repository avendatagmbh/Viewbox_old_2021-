using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearchLogic.SearchCore.Structures.Db {
    public class TableInfo : NotifyPropertyChangedBase{
        public TableInfo(string name, long count) {
            Name = name;
            Count = count;
            //ColumnCount = columnCount;
            ColumnToColumnInfo = new Dictionary<string, ColumnInfo>();
        }

        public string Name { get; set; }
        public long Count { get; set; }
        //public int ColumnCount { get; set; }
        public int Id { get; set; }
        public Dictionary<string, ColumnInfo> ColumnToColumnInfo { get; private set; } 

        #region Comment
        private string _comment;
        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    OnPropertyChanged("Comment");
                }
            }
        }
        #endregion Comment

        public bool IsHugeTable {
            get { return ((Count < 0) || (Count > 250000)); }
        }


        #region IComparable Member

        /// <summary>
        /// Compares this with the TableInfo object using the count property.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj) {
            TableInfo oTableInfo = (TableInfo)obj;
            return (this.Count.CompareTo(oTableInfo.Count));
        }

        #endregion

        public void AddColumnInfo(ColumnInfo columnInfo) {
            lock (ColumnToColumnInfo) {
                ColumnInfo result;
                if (!ColumnToColumnInfo.TryGetValue(columnInfo.Name, out result)) {
                    ColumnToColumnInfo.Add(columnInfo.Name, columnInfo);
                } else result.Comment = columnInfo.Comment;
            }
        }

        public ColumnInfo GetColumnInfo(string columnName) {
            ColumnInfo result;
            lock (ColumnToColumnInfo) {
                if(!ColumnToColumnInfo.TryGetValue(columnName, out result)) {
                    result = new ColumnInfo(columnName);
                    ColumnToColumnInfo.Add(columnName, result);
                }
            }
            return result;
        }
    }
}
