using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;

namespace eBalanceKitBusiness.HyperCubes.Import
{
    public class CsvRow : IDataRow, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        public delegate IDataRowEntry CreateRowEntryFunc(object obj);

        private readonly CreateRowEntryFunc _createRowEntry;

        private CsvRow() {
            RowEntries = new ObservableCollection<IDataRowEntry>();
        }

        public CsvRow(IEnumerable<IDataColumn> columns, CreateRowEntryFunc createRowEntryFunc):this() {
            _createRowEntry = createRowEntryFunc;
            RowEntries = new ObservableCollection<IDataRowEntry>();
            foreach(var column in columns)
                RowEntries.Add(createRowEntryFunc(null));
        }

        public CsvRow(object header, IEnumerable<IDataRowEntry> entries):this() {
            Header = header;
            foreach(var entry in entries)
                RowEntries.Add(entry);
        }
        /*
        public CsvRow(Dictionary<DataColumn,IDataRowEntry> rowDict) {
            RowEntries.Add(entries);
        }
        */

        public ObservableCollection<IDataRowEntry> RowEntries { get; private set; }

        public void AddColumn() {
            RowEntries.Add(_createRowEntry(null));
        }

        #region Header
        private object _header;

        public object Header {
            get { return _header; }
            set {
                if (_header != value) {
                    _header = value;
                    OnPropertyChanged("Header");
                }
            }
        }
        #endregion Header

        #region AssignmentFlag
        private bool _assignmentFlag;

        public bool AssignmentFlag {
            get { return _assignmentFlag; }
            set {
                if (_assignmentFlag != value) {
                    _assignmentFlag = value;
                    OnPropertyChanged("AssignmentFlag");
                }
            }
        }
        #endregion AssignmentFlag

        [IndexerName("Item")]
        public IDataRowEntry this[int index] {
            get { return RowEntries[index]; }
            set { 
                RowEntries[index] = value;
                OnPropertyChanged("Item[]");
                OnPropertyChanged("RowEntries");
            }
        }

    }
}
