using System.Collections.ObjectModel;

namespace AvdCommon.DataGridHelper.Interfaces
{
    public interface IDataRow
    {
        ObservableCollection<IDataRowEntry> RowEntries { get; }
        IDataRowEntry this[int index] { get; set; }
        void AddColumn();
    }
}