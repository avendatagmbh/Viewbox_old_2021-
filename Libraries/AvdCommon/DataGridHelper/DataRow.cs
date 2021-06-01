using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvdCommon.DataGridHelper.Interfaces;

namespace AvdCommon.DataGridHelper
{
    public class DataRow : IDataRow, INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Events

        #region Delegates

        public delegate IDataRowEntry CreateRowEntryFunc(object obj);

        #endregion

        private readonly CreateRowEntryFunc _createRowEntry;

        public DataRow(IEnumerable<IDataColumn> columns, CreateRowEntryFunc createRowEntryFunc)
        {
            _createRowEntry = createRowEntryFunc;
            RowEntries = new ObservableCollection<IDataRowEntry>();
            foreach (var column in columns)
                RowEntries.Add(createRowEntryFunc(null));
        }

        #region IDataRow Members

        public ObservableCollection<IDataRowEntry> RowEntries { get; private set; }

        public void AddColumn()
        {
            RowEntries.Add(_createRowEntry(null));
        }

        [IndexerName("Item")]
        public IDataRowEntry this[int index]
        {
            get { return RowEntries[index]; }
            set
            {
                RowEntries[index] = value;
                OnPropertyChanged("Item[]");
                OnPropertyChanged("RowEntries");
            }
        }

        #endregion
    }
}