using System.Collections.Generic;
using AvdCommon.DataGridHelper.Interfaces;
using Utils;

namespace AvdCommon.DataGridHelper
{
    public class DataTable
    {
        #region Delegates

        public delegate IDataColumn CreateColumnFunction(string name);

        #endregion

        public DataTable()
        {
            Columns = new List<IDataColumn>();
            Rows = new ObservableCollectionAsync<IDataRow>();
            CreateRowEntryFunc = obj => new DataRowEntry(obj);
            CreateColumnFunc = name => new DataColumn(name);
        }

        public List<IDataColumn> Columns { get; private set; }
        public ObservableCollectionAsync<IDataRow> Rows { get; private set; }
        public DataRow.CreateRowEntryFunc CreateRowEntryFunc { get; set; }
        public CreateColumnFunction CreateColumnFunc { get; set; }

        public void AddColumn(IDataColumn column)
        {
            Columns.Add(column);
            foreach (var row in Rows)
                row.AddColumn();
        }

        public void AddColumn(string name)
        {
            Columns.Add(CreateColumnFunc(name));
            foreach (var row in Rows)
                row.AddColumn();
        }

        public DataRow CreateRow()
        {
            return new DataRow(Columns, CreateRowEntryFunc);
        }

        public void AddRow(IDataRow row)
        {
            Rows.Add(row);
        }
    }
}